![.NET](https://img.shields.io/badge/.NET-8-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue)
![Docker](https://img.shields.io/badge/Docker-ready-blue)
![Swagger](https://img.shields.io/badge/API-Swagger-green)
![Tests](https://img.shields.io/badge/tests-xUnit-success)

# Tender Analytics Service

Backend service for importing Ukrainian public procurement data from the Prozorro API, storing it in PostgreSQL, and exposing analytical REST endpoints.

The application filters tenders according to predefined business rules, downloads tender details with limited parallelism, persists normalized data using an idempotent upsert workflow, and provides aggregated analytics for budget savings, procurers, and suppliers.

## Highlights

- Clean Architecture
- PostgreSQL schema designed for analytical queries
- Parallel tender detail downloads with configurable concurrency
- Idempotent imports and supplier deduplication
- Resilient external HTTP calls with retry and timeout policies
- Docker Compose startup
- Automatic EF Core migrations
- Swagger/OpenAPI documentation
- PostgreSQL health checks
- Global exception handling
- Unit tests for mapping and import workflows

---

## Features

- Import a single tender by identifier
- Import tenders from the paginated public feed
- Filter feed items by status and creation date before downloading details
- Filter full tender data by CPV classification
- Store only active contracts
- Update existing tenders instead of creating duplicates
- Deduplicate suppliers by identifier or normalized name
- Calculate total budget savings
- Return Top procurers by total contract value
- Return Top suppliers by total contract value
- Expose application and database health status

---

## Business Rules

Only tenders matching the following criteria are imported:

- Status: `complete`
- Creation date: from `2025-12-01T00:00:00Z` up to, but not including, `2026-01-01T00:00:00Z`
- CPV classification: `09310000-5`

Only contracts with status `active` are stored and included in analytics.

Suppliers are deduplicated using:

1. Supplier identifier, when available
2. Normalized supplier name, when the identifier is missing

Budget savings are calculated as:

```text
Total expected tender amount - Total active contract amount
```

---

## Architecture

The solution follows Clean Architecture principles.

```text
                          REST API
                              │
                              ▼
                    +-------------------+
                    |        API        |
                    | Controllers       |
                    | Middleware        |
                    | Swagger           |
                    | Health Checks     |
                    +-------------------+
                              │
                              ▼
                    +-------------------+
                    |    Application    |
                    | Services          |
                    | Interfaces        |
                    | DTOs              |
                    | Mapping           |
                    | Business Rules    |
                    +-------------------+
                         │           │
                         │           ▼
                         │    +-------------------+
                         │    |      Domain       |
                         │    | Entities          |
                         │    | Relationships     |
                         │    +-------------------+
                         ▼
                    +-------------------+
                    |  Infrastructure   |
                    | EF Core           |
                    | PostgreSQL        |
                    | Repositories      |
                    | Analytics         |
                    | HttpClient        |
                    | Resilience        |
                    +-------------------+
                         │           │
                         ▼           ▼
                    PostgreSQL   Public API
```

### Domain

Contains the core persistence entities:

- `Tender`
- `Contract`
- `Supplier`
- `ContractSupplier`

### Application

Contains:

- Import workflow
- External API contracts
- Mapping logic
- Service interfaces
- Import and analytics DTOs
- Business validation rules

### Infrastructure

Contains:

- EF Core `DbContext`
- PostgreSQL entity configurations
- Repository implementations
- External API client
- Resilience policies
- Analytical database queries
- Database migrations

### API

Contains:

- REST controllers
- Global exception middleware
- Swagger configuration
- Health check endpoint
- Automatic migration startup logic

---

## Technology Stack

| Category | Technology |
|---|---|
| Framework | .NET 8 |
| API | ASP.NET Core |
| ORM | Entity Framework Core |
| Database | PostgreSQL 16 |
| HTTP client | `HttpClientFactory` |
| Resilience | `Microsoft.Extensions.Http.Resilience` / Polly |
| Documentation | Swagger / OpenAPI |
| Testing | xUnit, Moq, FluentAssertions |
| Containerization | Docker, Docker Compose |
| Frontend | React, TypeScript, Vite |
| Frontend tooling | ESLint |
---

## Project Structure

```text
.
├── frontend
│   ├── public
│   ├── src
│   │   ├── api
│   │   ├── components
│   │   ├── pages
│   │   └── types
│   ├── .env.example
│   ├── package.json
│   └── vite.config.ts
├── src
│   ├── TenderAnalytics.Api
│   ├── TenderAnalytics.Application
│   ├── TenderAnalytics.Domain
│   └── TenderAnalytics.Infrastructure
├── tests
│   └── TenderAnalytics.Tests
├── docker-compose.yml
└── TenderAnalytics.sln
```

---

## Import Workflow

```text
Paginated feed
      │
      ▼
Filter by status and creation date
      │
      ▼
Download candidate tender details in parallel
      │
      ▼
Validate CPV, status, date, and required fields
      │
      ▼
Map external DTOs to domain entities
      │
      ▼
Reuse or create suppliers
      │
      ▼
Idempotent transactional upsert
      │
      ▼
PostgreSQL analytics
```

The HTTP download stage uses configurable limited concurrency. Database writes are performed sequentially within the scoped import service because EF Core `DbContext` is not thread-safe.

Existing tenders are updated instead of duplicated. Existing contracts and supplier links are replaced with the latest imported state.

---

## REST API

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/import/tender/{id}` | Imports one tender if it matches the business rules |
| `POST` | `/api/import/feed` | Processes the paginated feed using the supplied import settings |
| `GET` | `/api/analytics/savings` | Returns expected amount, contract amount, and total savings |
| `GET` | `/api/analytics/top-procurers?limit=5` | Returns procurers ordered by total contract value |
| `GET` | `/api/analytics/top-suppliers?limit=5` | Returns suppliers ordered by total contract value |
| `GET` | `/health` | Checks application and PostgreSQL health |

### Import a single tender

```http
POST /api/import/tender/{id}
```

Example:

```bash
curl -X POST \
  http://localhost:8080/api/import/tender/c674c3ea480a4eb1a089976ec840c6d9
```

### Import from the feed

```http
POST /api/import/feed
Content-Type: application/json
```

Example request:

```json
{
  "dateFrom": "2025-12-01T00:00:00Z",
  "dateTo": "2026-01-01T00:00:00Z",
  "maxPages": 5,
  "maxConcurrency": 8
}
```

Example:

```bash
curl -X POST \
  http://localhost:8080/api/import/feed \
  -H "Content-Type: application/json" \
  -d '{
    "dateFrom": "2025-12-01T00:00:00Z",
    "dateTo": "2026-01-01T00:00:00Z",
    "maxPages": 5,
    "maxConcurrency": 8
  }'
```

### Budget savings

```http
GET /api/analytics/savings
```

Example response:

```json
{
  "expectedAmount": 8639477.97,
  "contractAmount": 8087108.40,
  "savingsAmount": 552369.57,
  "currency": "UAH"
}
```

### Top procurers

```http
GET /api/analytics/top-procurers?limit=5
```

### Top suppliers

```http
GET /api/analytics/top-suppliers?limit=5
```

### Health check

```http
GET /health
```

---

## Running with Docker

The recommended way to start the project is Docker Compose.

```bash
docker compose up --build
```

The command starts:

- PostgreSQL on `localhost:5432`
- API on `localhost:8080`

The API automatically applies pending EF Core migrations during startup.

### Swagger

```text
http://localhost:8080/swagger
```

### Health check

```text
http://localhost:8080/health
```

Run containers in detached mode:

```bash
docker compose up --build -d
```

Stop containers:

```bash
docker compose down
```

Stop containers and remove the PostgreSQL volume:

```bash
docker compose down -v
```

---

## Running Locally

### Prerequisites

- .NET 8 SDK
- Docker Desktop or a locally available PostgreSQL 16 instance

### Start PostgreSQL

```bash
docker compose up -d postgres
```

### Restore and build

```bash
dotnet restore
dotnet build
```

### Run the API

```bash
dotnet run --project src/TenderAnalytics.Api
```

The local development URLs are:

```text
Swagger: http://localhost:5297/swagger
Health:  http://localhost:5297/health
```

---

## Frontend

The repository includes a lightweight React dashboard built with Vite and TypeScript.

### Configure the API URL

Copy the example environment file:

```bash
cd frontend
cp .env.example .env
```

On Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Default local configuration:

```env
VITE_API_URL=http://localhost:5297
```

When the API is running through Docker:

```env
VITE_API_URL=http://localhost:8080
```

The `.env.example` file is committed to the repository.  
The local `.env` file is ignored by Git.

### Install dependencies

```bash
cd frontend
npm install
```

### Run the dashboard

```bash
npm run dev
```

The dashboard is available at:

```text
http://localhost:5173
```

### Validate the frontend

```bash
npm run lint
npm run build
```

## Database

The normalized schema includes:

- `tenders`
- `contracts`
- `suppliers`
- `contract_suppliers`

Important indexes include:

- CPV, status, and creation date
- Procuring entity data
- Contract tender identifier
- Contract award identifier
- Unique supplier identifier
- Normalized supplier name
- Supplier identifier on the join table

Monetary values use fixed-precision decimal columns.

Dates are converted to UTC before persistence.

---

## Reliability and Error Handling

External requests use a standard resilience pipeline with:

- Exponential retry
- Maximum retry attempts
- Per-attempt timeout
- Total request timeout

The API includes a global exception middleware that returns a consistent JSON error response containing:

- HTTP status code
- Safe error message
- Trace identifier

A failure while downloading or saving one feed item is recorded in the import result without stopping the entire feed operation.

---

## Tests

Run all tests:

```bash
dotnet test
```

Current tests cover:

- Tender mapping
- Filtering inactive contracts
- Supplier mapping
- UTC date conversion
- Invalid source data
- Single tender import
- Status, CPV, and date validation
- Feed processing
- Download failure handling
- Import request validation

---

## Design Decisions and Trade-offs

### Only active contracts are stored

The external response can contain active and cancelled contracts. Analytics use only active contracts to avoid counting cancelled contract amounts.

### Suppliers are linked to contracts

The source API associates suppliers with awards. During ETL, the supplier relationship is resolved through `contract.awardID` and stored against the contract because contract amounts are the basis of supplier analytics.

### Procuring entity data is stored on the tender

The analytical requirement only needs grouping by procuring entity. Keeping these fields on the tender avoids an additional join and simplifies the ETL workflow.

### Parallel HTTP, sequential database writes

Tender details are downloaded concurrently to reduce network wait time. Persistence remains sequential within a request to avoid concurrent use of a scoped EF Core `DbContext`.

### Feed traversal

The public feed uses cursor pagination and is ordered by modification time. The service follows `next_page.uri`; it does not manufacture cursor offsets manually.

### Multiple suppliers per contract

When a contract has several suppliers, the full contract amount is attributed to each linked supplier because the specification does not define an allocation rule.

---

## Future Improvements

- Background import worker using `BackgroundService`
- Incremental synchronization with persisted feed offsets
- Import job status and progress tracking
- Batch persistence for larger import volumes
- PostgreSQL materialized views for expensive analytics
- Redis caching
- Authentication and authorization
- Prometheus metrics and distributed tracing
- CI/CD pipeline
- Frontend containerization and Docker Compose integration
- Charts and extended dashboard filters
---

## License

This project was created as a technical assessment.