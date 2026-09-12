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

## Chapter 2 — *getOne / create for products (to be written)*
## Chapter 3 — *categories (to be written)*
## Chapter 4 — *JWT authentication (to be written)*
## Chapter 5 — *performance logging middleware (to be written)*
## Chapter 6 — *CQRS, unit tests, docker (to be written)*
