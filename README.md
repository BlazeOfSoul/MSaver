# MSaver

MSaver is a personal finance app for tracking income, expenses, and overall balance.

## Architecture

- Domain-Driven Design (DDD)
  - Domain layer contains business rules and pure domain models such as users, categories, transactions, accounts, and refresh tokens.
  - Domain errors are modeled with `DomainError` and `DomainException`.
- Clean Architecture
  - Domain: entities, value objects, enums, domain errors, repository and unit-of-work interfaces.
  - Application: use cases, DTOs, services, and the `Result` type.
  - Infrastructure: EF Core, PostgreSQL, repository implementations, JWT generation, current user access, and exchange-rate integration.
  - API: controllers, base API controller, and global error handling middleware.

## Persistence

- Database: PostgreSQL
- ORM: Entity Framework Core
- Unit of Work ensures all changes in a use case are saved atomically.

## Auth & Security

- JWT access and refresh tokens
  - Access tokens are short-lived and used for authorized API calls.
  - Refresh tokens are long-lived, stored in the database, and rotated on refresh.
  - A user can have up to 3 active refresh tokens.

## Local development

Run the API directly from this repository when you are not using Docker:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=MSaverDb;Username=postgres;Password=<postgres-password>"
dotnet user-secrets set "JwtSettings:Key" "local-dev-signing-key-change-me-32-bytes-min"
dotnet user-secrets set "ExchangeRateApi:ApiKey" "<exchange-rate-api-key>"
dotnet run --launch-profile http
```

The `http` launch profile listens on `http://localhost:5200`, which matches the frontend development proxy in `MSaver.Frontend/proxy.conf.json`.

Keep real connection strings, JWT signing keys, and exchange-rate API keys in .NET user secrets or environment variables. The committed `appsettings*.json` files intentionally keep secret values empty.
