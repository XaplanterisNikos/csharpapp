# Solution Notes

This document explains **how** I approached the assignment — the problems I found and the
reasoning behind each decision — rather than just the final code. It complements the commit
history: the commits show *what* changed, this document shows *why*.

I worked one topic at a time: read the existing code, note the problems, decide on an
approach, then implement.

---

## Chapter 1 — HTTP client refactoring

**Task:** *"The use of http client is not so much efficient. Let's make a different, more
solid, approach/implementation."*

**What I found**

- `ProductsService` created its `HttpClient` manually (`new HttpClient()`) while being
  registered as a **singleton** — one long-lived client for the whole app.
- It set `BaseAddress` on that shared instance on **every call**.
- It fetched data in three manual steps (`GetAsync` → `ReadAsStringAsync` → `Deserialize`)
  and then called `.AsReadOnly()` on a result that could be `null`.
- `AddDefaultConfiguration` called `services.BuildServiceProvider()` during registration
  (analyzer warning ASP0000).

**Why these matter**

A manual `HttpClient` forces a bad trade-off: a new client per call risks *socket
exhaustion*, while one client forever risks *stale DNS*. Mutating `BaseAddress` on a shared
instance is a latent thread-safety issue. `Deserialize` can return `null`. And
`BuildServiceProvider()` builds a second DI container needlessly.

**Decisions**

- Moved to **`IHttpClientFactory`** (the intended "more solid approach"), which pools and
  rotates handlers, solving socket exhaustion and stale DNS together.
- Registered `ProductsService` as a **typed client** (single API, injected type-safely)
  rather than a named one.
- Set `BaseAddress` **once at registration**, removing the per-call mutation.
- Added a **Polly** retry policy for transient faults with **exponential backoff**. This
  connects the three previously-unused settings: `LifeTime` → handler lifetime,
  `RetryCount` → retries, `SleepDuration` → base delay.
- Removed the old `AddSingleton` registration: a typed client is **transient**, so the
  factory can rotate the handler (a singleton would capture it — a *captive dependency*).
- Passed `IConfiguration` in explicitly instead of `BuildServiceProvider()`.
- Replaced the manual fetch/deserialize with `GetFromJsonAsync<T>` plus a null guard, and
  added XML doc comments (with `<inheritdoc />` on the implementation).

**Verified:** `dotnet build` succeeds; `GET /api/v1/getproducts` returns `200 OK` with the
product list.

**Forward note:** the settings mix relative-path styles — `products` (no leading slash)
resolves correctly against a base ending in `/api/v1/`, but `Categories` and `Auth` start
with a leading slash, which resets the path to the host root. To be addressed with categories
and JWT auth.

---

## Chapter 2 — getOne / create for products

**Task:** *"Right now only the getAll method is supported for products. We have to implement
getOne and create methods also."*

**Decisions**

- Added `GetProduct(id)` and `CreateProduct(request)` to `IProductsService` and its typed
  client, plus `getproduct/{id:int}` and `createproduct` endpoints.
- Kept the **existing (non-RESTful) naming style** (`getproduct`, `createproduct`) for
  consistency with the pre-existing `getproducts`, and noted it as a deliberate choice. A
  RESTful scheme (`GET products/{id}`, `POST products`) would be the alternative.
- Introduced a dedicated **`CreateProductRequest`** DTO. The create contract genuinely
  differs from the product response: the API expects a flat `categoryId` (int), while the
  `Product` response carries a nested `category` object. Reusing `Product` would pollute the
  response model and risk over-posting, so a separate request DTO keeps the contract clean.
- Modeled **"not found" as data, not an exception**: `GetProduct` returns `Product?`, and the
  endpoint returns a clean `404 NotFound`. Genuinely unexpected failures (5xx, network) are
  left to surface to the existing `ProblemDetails`.
- Used **`201 Created`** for creation, and `PostAsJsonAsync` for the request body.

**Discovered while testing (live)**

This external API returns **`400 Bad Request`**, not `404`, for a missing product id. Because
the route is constrained to `{id:int}` and the URL is built by us, a `400`/`404` here can only
mean "no such product" — so both are treated as not-found and mapped to a clean `404`. This is
a deliberate, documented compromise scoped to this specific endpoint; it does **not** swallow
other non-success codes.

**Verified:** `getproduct/{existing id}` → `200`; `getproduct/{missing id}` → `404` (no
exception); `createproduct` → `201 Created` with a new id. Confirmed that sending `categoryId`
returns a full `Product` with nested `category`, validating the separate request DTO.

**Open items (deferred to Chapter 5 — middleware):**
1. Handling of genuinely unexpected errors (what the client sees, what gets logged).
2. Whether to formalize the "400-as-not-found" quirk more robustly.
3. Whether responses should carry descriptive messages (a `404` currently has no body).

---

## Chapter 3 — categories

**Task:** *"Add implementation for categories."*

**Decisions**

- Mirrored the products pattern: `ICategoriesService` and a typed-client `CategoriesService`,
  with `getcategories`, `getcategory/{id:int}` and `createcategory` endpoints in the same
  naming style. Implemented getAll, getOne **and** create for parity with products.
- Added a dedicated **`CreateCategoryRequest`** DTO (name, image), for the same
  request-vs-response reasoning as products.
- Reused the same "not found as null → 404" and "this API returns 400 for a missing id"
  handling established in Chapter 2.

**The leading-slash fix (predicted in Chapter 1)**

The `Categories` setting was `"/categories"`. Combined with a base address ending in
`/api/v1/`, a **leading slash makes it an absolute-path reference**: it keeps only the host and
discards the base path, so the request would hit `https://api.escuelajs.co/categories` instead
of `.../api/v1/categories`. The fix is to align the value with the working `products` entry —
i.e. **remove** the leading slash (`/categories` → `categories`), not add one everywhere. For
a base that ends in `/`, relative paths must **not** start with `/`. This is a data/config fix
at the root cause rather than code that masks the inconsistency.

**Structure & DRY**

- Refactored `HttpConfiguration` so each typed client is registered through a single generic
  helper `AddApiClient<TInterface, TImplementation>` that applies the shared base address,
  handler lifetime and retry policy — so those live in one place for all clients.
- Reorganized DTOs into per-feature folders (`Products`, `Categories`). Folder names are
  **plural** on purpose: a folder named `Category` produces a namespace that clashes with the
  `Category` type (CS0118 — "namespace used like a type"), so plural namespaces avoid the
  ambiguity.
- Removed a pre-existing duplicate `Microsoft.AspNetCore.OpenApi` package reference (NU1504).

**Verified:** `getcategories` → `200` with the category list (confirming the slash fix live);
`getcategory/{existing}` → `200`, `getcategory/{missing}` → `404`; `createcategory` → `201`.

---

## Chapter 4 — *JWT authentication (to be written)*
## Chapter 5 — *performance logging middleware (to be written)*
## Chapter 6 — *CQRS, unit tests, docker (to be written)*
