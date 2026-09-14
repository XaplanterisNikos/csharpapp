# Solution Notes

This document explains **how** I approached the assignment — the problems I found and the
reasoning behind each decision — rather than just the final code. It complements the commit
history: the commits show _what_ changed, this document shows _why_.

I worked one topic at a time: read the existing code, note the problems, decide on an
approach, then implement.

---

## Chapter 1 — HTTP client refactoring

**Task:** _"The use of http client is not so much efficient. Let's make a different, more
solid, approach/implementation."_

**What I found**

- `ProductsService` created its `HttpClient` manually (`new HttpClient()`) while being
  registered as a **singleton** — one long-lived client for the whole app.
- It set `BaseAddress` on that shared instance on **every call**.
- It fetched data in three manual steps (`GetAsync` → `ReadAsStringAsync` → `Deserialize`)
  and then called `.AsReadOnly()` on a result that could be `null`.
- `AddDefaultConfiguration` called `services.BuildServiceProvider()` during registration
  (analyzer warning ASP0000).

**Why these matter**

A manual `HttpClient` forces a bad trade-off: a new client per call risks _socket
exhaustion_, while one client forever risks _stale DNS_. Mutating `BaseAddress` on a shared
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
  factory can rotate the handler (a singleton would capture it — a _captive dependency_).
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

**Task:** _"Right now only the getAll method is supported for products. We have to implement
getOne and create methods also."_

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

---

## Chapter 3 — categories

**Task:** _"Add implementation for categories."_

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
- Reorganized DTOs into per-feature folders (`Products`, `Categories`).

**Verified:** `getcategories` → `200` with the category list (confirming the slash fix live);
`getcategory/{existing}` → `200`, `getcategory/{missing}` → `404`; `createcategory` → `201`.

---

## Chapter 4 — JWT authentication

**Task:** _"The 3rd party service supports JWT authentication. Implement and support it, using the credentials in appsettings.json."_

**The idea**

The credentials in appsettings belong to the third-party API (escuelajs). So this is _outbound_ auth: our app logs in to the third-party service and sends a token on its calls. We are not protecting our own endpoints — we are the client that authenticates.

The flow is simple: log in once with email + password, get back a token, and put that token on every following call as an `Authorization: Bearer ...` header. The password is sent only once (at login); after that we travel with the token.

**Steps we took**

1. Two small DTOs for the login call: what we send (email, password) and what we get back (the token).
2. A `TokenProvider` that logs in, keeps the token in memory, and hands it out. It is a singleton so the whole app shares one token, and it logs in only once instead of on every call.
3. An `AuthenticationDelegatingHandler` — a small piece that sits on the outgoing calls, adds the token automatically, and if the token is rejected (401) refreshes it once and retries. This keeps the token logic out of the services: they just call the API and the token is added behind the scenes.
4. Wiring: the business clients (products, categories) get the handler; a separate login-only client does **not** get it — otherwise the login call would trigger the handler, which would log in again, forever (an endless loop). Keeping that client separate breaks the loop.

**A couple of decisions**

- When a token is rejected we simply log in again (we already hold the credentials), instead of using the refresh token. Simpler, and it needs no extra settings. The refresh-token flow would matter more if we did not hold the password.
- Added a startup check so the app fails immediately, with a clear message, if a required setting is missing — instead of failing later on the first call.
- Fixed the `Auth` path the same way as `Categories` (removed the leading slash), and also trim it in code so a stray slash can't break it.

**On the credentials:** they stay in appsettings because the assignment asks for that and the project must run as-is. In a real project they would move to user-secrets (dev) or environment variables / a secrets vault (prod), and a real password would never be committed.

**Verified (live, via logs):** the first call to `getproducts` logs in once; the second call reuses the cached token and does not log in again. Logging in against the third-party API directly also returns a valid token.

---

## Chapter 5 — request performance logging

**Task:** _"We must measure and log the performance of the requests. Create a middleware to achieve this."_

**The idea**

Middleware is a small piece that sits on the request pipeline: every incoming request passes through it on the way in and the response passes back through it on the way out. That gives a natural place to start a timer before the request is handled and stop it after — and log how long it took. (It is the same pattern as the auth handler from Chapter 4, just for incoming requests instead of outgoing calls.)

**Steps we took**

1. A `RequestPerformanceMiddleware` class that starts a `Stopwatch`, passes the request down the pipeline, and then logs the method, path, status code and elapsed milliseconds.
2. A small extension method (`UseRequestPerformanceLogging`) so it registers cleanly in one line.
3. Registered it **first** in the pipeline, so the time measured covers the whole request.

**A couple of decisions**

- Wrote our own middleware because the task asks to "create a middleware". Serilog also has a built-in request logger (`UseSerilogRequestLogging`) that would do this in production; writing our own shows the mechanism.
- Used structured logging (method, path, status, ms as separate fields) rather than one joined string, so the values stay searchable in the logs.
- Wrapped the work in `try/finally`, so the timing is logged even if something further down throws.
- Logged only safe metadata — method, path, status, time. On purpose we do **not** log headers, body or query values, because those can contain tokens or personal data.

**Verified (live):** every call now produces a log line such as `HTTP GET /api/v1/getproducts responded 200 in 342 ms`, including error responses (e.g. a 404).

---

## Chapter 6 — CQRS, tests and docker

CQRS is done first; unit tests and docker follow.

### CQRS with MediatR

**Task:** _"Using CQRS pattern will be considered as a strong plus."_

**The idea**

CQRS means separating operations that **read** (queries) from operations that **write** (commands). Each operation becomes its own small object with its own handler, instead of one service holding everything.

I added it **last, on purpose**, as a refactor over the existing services rather than building it from the start. This was clean precisely because the services were already behind interfaces — the endpoints and services didn't need rewriting, only a new layer on top. A "strong plus" is an optional layer; being able to add it painlessly at the end is the payoff of putting sensible boundaries in place early.

**How it fits our code**

The flow changed from `endpoint → service → API` to `endpoint → mediator → handler → service → API`. The handlers are deliberately **thin**: they orchestrate, and delegate the actual HTTP work to the existing services. Those services act as a **gateway** that hides the typed `HttpClient`, the auth handler and the resilience policy. In a typical app the handler would talk to a `DbContext`; here the "database" is a third-party API behind authentication, so a gateway is the right equivalent. CQRS separates reads from writes — it does not forbid a data-access abstraction.

**Steps**

1. A command or query per operation (get all, get by id, create — for both products and categories), each implementing MediatR's `IRequest<T>`.
2. A handler per request that calls the matching service method.
3. Endpoints now send requests through `ISender`, so they no longer depend on the service implementation — only on the request object.
4. Registered MediatR through an `AddApplication` extension inside the Application project, so each layer registers its own services.

### Request validation (FluentValidation)

Added a `ValidationBehavior` into the MediatR pipeline — a small piece that runs before every handler and validates the request first. If a rule fails, it stops the request before the handler runs. Validators were added for the two create commands (e.g. title required, price ≥ 0, category image must be a valid URL — the same invalid-image case that had caused a 400 earlier). This is where CQRS and FluentValidation fit together: the validator sits in the request pipeline, so validation is central rather than scattered across endpoints.

**Verified:** all six endpoints work through the mediator exactly as before; an invalid create command is stopped by the validator with a clear message; a valid one still returns `201`.

### Unit tests

**Task:** _"Add unit testing."_

Added an xUnit test project under `tests/`, referencing the Application project. The tests cover the CQRS handlers and the validators — the logic added in this chapter.

**Approach**

- Each handler test uses **Moq** to replace the service with a stand-in, so the test checks only the handler's own logic without any real HTTP call, network or token. This keeps the tests fast, deterministic and isolated (true unit tests, not integration tests).
- Handler tests follow the Arrange–Act–Assert shape: set up the fake service, call `Handle`, then assert the result and that the service was called once. The get-by-id handlers also have a test for the not-found path (service returns null → handler returns null).
- Validator tests need no mocks: they build the validator directly, feed it a valid and an invalid command, and check the result — including the invalid-image-URL case.
- Used **xUnit's built-in assertions** rather than FluentAssertions, because FluentAssertions moved to a commercial licence (v8+); avoiding it keeps the delivered code free of licence obligations.

**Verified:** `dotnet test` → 14 tests, all passing, and the solution builds with no warnings.

### Docker support

**Task:** _"Add docker support."_

Added a multi-stage `Dockerfile` at the repository root, plus a `.dockerignore`.

- **Build stage** (`sdk` image): copies the project files first and restores (so package restore is cached and only re-runs when a dependency changes), then copies the source and publishes a Release build.
- **Runtime stage** (`aspnet` image): a small image that copies only the published output from the build stage and runs it. The source code and build tools stay out of the final image, keeping it small.
- `.dockerignore` keeps `bin/`, `obj/` and `.git/` out of the build context.

**Verified:** `docker build` produces an image, `docker run -p 8080:8080` serves the API, and the endpoints respond as expected from inside the container.
