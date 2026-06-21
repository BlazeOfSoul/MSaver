# MSaver

MSaver is a personal finance app for tracking income, expenses, and overall balance.

## Architecture

- **Domain‑Driven Design (DDD)**
  - Domain layer contains business rules and pure domain models (User, Category, Transaction, Balance, RefreshToken).
  - Domain errors are modeled with `DomainError` and `DomainException`.

- **Clean Architecture**
  - **Domain** – entities, value objects, enums, domain errors, repository and unit‑of‑work interfaces.
  - **Application** – use cases (Auth, Categories, Transactions, Balance, ExchangeRates), DTOs, services, `Result` type.
  - **Infrastructure** – EF Core + PostgreSQL, repository implementations, `UnitOfWork`, JWT generator, current user, exchange‑rate client.
  - **API** – controllers, base API controller, global error handling middleware.

## Persistence

- Database: **PostgreSQL**
- ORM: **Entity Framework Core**
- Unit of Work ensures all changes in a use case are saved atomically.

## Auth & Security

- **JWT Access + Refresh tokens in HttpOnly cookies**
  - Access token: short-lived, sent as an HttpOnly cookie for authorized API calls.
  - Refresh token: long-lived, stored in DB, sent as an HttpOnly cookie, and rotated on refresh.
  - Tokens are not exposed to frontend JavaScript storage.
  - Maximum 3 active refresh tokens per user.
