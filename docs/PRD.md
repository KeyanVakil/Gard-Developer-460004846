# PRD: Maritime Claims & Policy Portal

## 1. Project Overview

**Gard Claims & Policy Portal** is a web application for managing marine insurance policies, tracking vessel information, and processing insurance claims. It provides underwriters and claims handlers with a unified interface to view policy portfolios, register and adjudicate claims, and monitor risk exposure across a fleet.

### Why This Project

Gard is a leading marine insurer whose core business revolves around policies, claims, and vessel risk. This project demonstrates the ability to build the kind of internal business application that Gard's in-house development teams deliver daily — a data-rich, workflow-driven tool that turns complex insurance operations into a clean digital experience. It exercises the full stack from database modeling through API design to interactive UI, mirroring the "cross-technology API" integration work described in the role.

### The Problem It Solves

Marine insurance operations involve interconnected data: vessels have policies, policies have claims, claims have documents and status workflows. Without a unified portal, underwriters juggle spreadsheets and siloed systems. This application consolidates vessel management, policy administration, and claims processing into a single MVC web application with a modern Vue.js dashboard overlay.

---

## 2. Technical Architecture

```
┌─────────────────────────────────────────────────────────┐
│                        Browser                          │
│                                                         │
│  ┌──────────────────┐    ┌───────────────────────────┐  │
│  │  Razor MVC Views │    │  Vue.js Dashboard (SPA)   │  │
│  │  (CRUD pages)    │    │  (Charts, live filters)   │  │
│  └────────┬─────────┘    └────────────┬──────────────┘  │
│           │                           │                  │
└───────────┼───────────────────────────┼──────────────────┘
            │  Server-rendered HTML     │  REST API (JSON)
            │                           │
┌───────────┴───────────────────────────┴──────────────────┐
│              ASP.NET Core MVC Application                │
│                                                          │
│  ┌────────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  MVC Controllers│  │ API Controllers│ │  Services    │ │
│  │  (Razor views)  │  │ (/api/...)     │ │  (Business   │ │
│  │                 │  │                │ │   logic)     │ │
│  └────────┬────────┘  └───────┬────────┘ └──────┬───────┘ │
│           │                   │                  │        │
│           └───────────────────┴──────────────────┘        │
│                            │                              │
│              ┌─────────────┴─────────────┐                │
│              │   Entity Framework Core   │                │
│              │   (Code-First + Migrations)│                │
│              └─────────────┬─────────────┘                │
└────────────────────────────┼──────────────────────────────┘
                             │
                   ┌─────────┴─────────┐
                   │  SQL Server 2022  │
                   │  (Docker)         │
                   └───────────────────┘
```

### Key Components

| Component | Responsibility |
|-----------|---------------|
| **MVC Controllers + Razor Views** | Server-rendered CRUD pages for vessels, policies, and claims. Standard ASP.NET MVC pattern with model binding, validation, and partial views. |
| **API Controllers** | RESTful JSON endpoints (`/api/vessels`, `/api/policies`, `/api/claims`, `/api/dashboard`) consumed by the Vue.js dashboard. |
| **Service Layer** | Business logic: premium calculation, claims status workflow, risk scoring, portfolio aggregation. Injected via DI, independently testable. |
| **Entity Framework Core** | ORM with code-first migrations, LINQ queries, navigation properties. Targets SQL Server. |
| **Vue.js Dashboard** | Single-page TypeScript application embedded in a Razor layout page. Provides interactive charts (claims by status, premium by vessel type, risk heatmap) with client-side filtering. |
| **SQL Server** | Relational store for all domain data. Runs in Docker with seed data applied on startup. |

### Data Flow

1. **Server-rendered flow:** Browser → MVC Controller → Service → EF Core → SQL Server → Razor View → Browser
2. **API flow:** Vue.js app → `fetch()` → API Controller → Service → EF Core → SQL Server → JSON response → Vue.js reactivity updates DOM

---

## 3. Tech Stack

| Technology | Role | Rationale |
|-----------|------|-----------|
| **C# 12 / .NET 8** | Application language and runtime | Primary language in job listing |
| **ASP.NET Core MVC** | Web framework | MVC pattern explicitly required; handles both Razor views and API controllers |
| **Entity Framework Core 8** | ORM / data access | Listed in job requirements; code-first with migrations |
| **Razor Views** | Server-side templating | Listed in job requirements; used for CRUD pages |
| **TypeScript** | Frontend language | Listed in job requirements; type-safe frontend code |
| **Vue.js 3** | Dashboard SPA framework | Listed in job requirements; used for the interactive dashboard component |
| **Microsoft SQL Server 2022** | Primary database | Listed in job requirements; runs in Docker via `mcr.microsoft.com/mssql/server` |
| **Docker + Docker Compose** | Local infrastructure | Single `docker compose up` runs everything |
| **xUnit + Moq** | Testing | Standard .NET testing stack |
| **Git** | Version control | Listed in job requirements |

### What's Intentionally Excluded

- **Cosmos DB / Oracle**: The project uses SQL Server as the single relational store. Adding a second database engine would be artificial complexity.
- **Angular**: Vue.js is chosen for the dashboard; using both Vue and Angular would be gratuitous.
- **Azure Stack / Azure AD**: The project runs entirely locally via Docker. No cloud accounts needed.
- **Salesforce / Mule**: These are integration platforms relevant to Gard's production environment but not appropriate for a self-contained demo.
- **Microservices**: A single ASP.NET Core application is the right architecture for this scope.

---

## 4. Features & Acceptance Criteria

### Feature 1: Vessel Registry

Manage the fleet of insured vessels.

| # | Acceptance Criteria |
|---|-------------------|
| 1 | User can list all vessels with sortable columns (name, IMO number, type, flag state, gross tonnage) |
| 2 | User can create a new vessel with validation (IMO number format, required fields) |
| 3 | User can edit and delete vessels; deleting a vessel with active policies shows a confirmation warning |
| 4 | Vessel detail page shows associated policies and claims |
| 5 | Server-side pagination for vessel list (20 per page) |

### Feature 2: Policy Administration

Create and manage insurance policies linked to vessels.

| # | Acceptance Criteria |
|---|-------------------|
| 1 | User can create a policy by selecting a vessel, coverage type (P&I, Hull & Machinery, Cargo, Loss of Hire), start/end dates, and insured value |
| 2 | Annual premium is auto-calculated based on vessel type, tonnage, and coverage type using a configurable rate table |
| 3 | Policies have statuses: Draft → Active → Expired → Cancelled, with valid state transitions enforced |
| 4 | Policy list supports filtering by status, coverage type, and vessel name |
| 5 | Policy detail page shows linked claims and a premium breakdown |

### Feature 3: Claims Processing

Register and manage insurance claims through a workflow.

| # | Acceptance Criteria |
|---|-------------------|
| 1 | User can file a claim against an active policy, specifying incident date, category (collision, grounding, cargo damage, crew injury, machinery breakdown), description, and estimated amount |
| 2 | Claims follow a status workflow: Reported → Under Review → Approved → Settled (or Denied at any stage) |
| 3 | Each status transition is logged with timestamp and optional notes |
| 4 | Settlement amount can differ from estimated amount; both are tracked |
| 5 | Claims list supports filtering by status, category, and date range |

### Feature 4: Dashboard & Analytics (Vue.js)

Interactive dashboard providing portfolio-level insights.

| # | Acceptance Criteria |
|---|-------------------|
| 1 | Dashboard loads via Vue.js SPA consuming the REST API |
| 2 | Shows summary cards: total active policies, open claims count, total insured value, total outstanding claims amount |
| 3 | Bar chart: claims count by category |
| 4 | Pie chart: policies by coverage type |
| 5 | Line chart: claims filed per month (trailing 12 months) |
| 6 | All charts respond to a vessel-type filter dropdown (Bulk Carrier, Tanker, Container, etc.) |

### Feature 5: Cross-System API

RESTful API layer suitable for integration with other systems.

| # | Acceptance Criteria |
|---|-------------------|
| 1 | All CRUD operations available via `/api/vessels`, `/api/policies`, `/api/claims` endpoints |
| 2 | Consistent JSON envelope: `{ "data": ..., "errors": [...] }` |
| 3 | Supports pagination via `?page=1&pageSize=20` query parameters |
| 4 | Supports filtering: e.g., `/api/claims?status=UnderReview&category=Collision` |
| 5 | `/api/dashboard/summary` returns aggregated metrics for the dashboard |
| 6 | API returns appropriate HTTP status codes (200, 201, 400, 404, 409) |

### Feature 6: Seed Data & Demo Scenario

Realistic sample data that makes the demo compelling.

| # | Acceptance Criteria |
|---|-------------------|
| 1 | Database is seeded with 15+ vessels of varied types (bulk carriers, tankers, container ships, RoRo) |
| 2 | 25+ policies across all coverage types and statuses |
| 3 | 40+ claims across all categories and statuses, with realistic date distribution |
| 4 | Seed data creates a compelling dashboard view out of the box |
| 5 | Seed runs automatically on first `docker compose up` via EF Core migrations |

---

## 5. Data Models

### Entity Relationship Diagram

```
┌──────────────┐       ┌──────────────────┐       ┌──────────────────┐
│   Vessel     │       │     Policy       │       │     Claim        │
├──────────────┤       ├──────────────────┤       ├──────────────────┤
│ Id (PK)      │──1:N──│ Id (PK)          │──1:N──│ Id (PK)          │
│ Name         │       │ VesselId (FK)    │       │ PolicyId (FK)    │
│ ImoNumber    │       │ PolicyNumber     │       │ ClaimNumber      │
│ VesselType   │       │ CoverageType     │       │ Category         │
│ FlagState    │       │ Status           │       │ Status           │
│ GrossTonnage │       │ StartDate        │       │ IncidentDate     │
│ YearBuilt    │       │ EndDate          │       │ Description      │
│ CreatedAt    │       │ InsuredValue     │       │ EstimatedAmount  │
│ UpdatedAt    │       │ AnnualPremium    │       │ SettledAmount    │
│              │       │ CreatedAt        │       │ CreatedAt        │
│              │       │ UpdatedAt        │       │ UpdatedAt        │
└──────────────┘       └──────────────────┘       └────────┬─────────┘
                                                           │
                                                      1:N  │
                                                           │
                                                  ┌────────┴─────────┐
                                                  │  ClaimHistory    │
                                                  ├──────────────────┤
                                                  │ Id (PK)          │
                                                  │ ClaimId (FK)     │
                                                  │ FromStatus       │
                                                  │ ToStatus         │
                                                  │ Notes            │
                                                  │ ChangedAt        │
                                                  └──────────────────┘
```

### Enumerations

```csharp
public enum VesselType
{
    BulkCarrier, Tanker, ContainerShip, RoRo, GeneralCargo, Offshore, PassengerFerry
}

public enum CoverageType
{
    ProtectionAndIndemnity,  // P&I
    HullAndMachinery,
    Cargo,
    LossOfHire
}

public enum PolicyStatus
{
    Draft, Active, Expired, Cancelled
}

public enum ClaimCategory
{
    Collision, Grounding, CargoDamage, CrewInjury, MachineryBreakdown, Pollution, Other
}

public enum ClaimStatus
{
    Reported, UnderReview, Approved, Settled, Denied
}
```

### Key Constraints

- `Vessel.ImoNumber` — unique, 7-digit format (IMO + 7 digits)
- `Policy.PolicyNumber` — unique, auto-generated (e.g., `POL-2026-00001`)
- `Claim.ClaimNumber` — unique, auto-generated (e.g., `CLM-2026-00001`)
- A claim can only be filed against a policy with status `Active`
- `Claim.SettledAmount` is nullable (only set when status = `Settled`)
- `ClaimHistory` is append-only; records are never updated or deleted

### Premium Calculation

Premium is computed as: `InsuredValue * BaseRate * VesselTypeFactor`

| Coverage Type | Base Rate |
|--------------|-----------|
| P&I | 0.0025 |
| Hull & Machinery | 0.0040 |
| Cargo | 0.0015 |
| Loss of Hire | 0.0020 |

| Vessel Type | Factor |
|------------|--------|
| Tanker | 1.3 |
| Offshore | 1.4 |
| Bulk Carrier | 1.0 |
| Container Ship | 1.1 |
| RoRo | 1.05 |
| General Cargo | 0.95 |
| Passenger Ferry | 1.2 |

---

## 6. API Design

### Base URL: `/api`

All API endpoints return JSON. Request bodies are JSON.

### Vessels

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/vessels` | List vessels (paginated, sortable) |
| GET | `/api/vessels/{id}` | Get vessel with policies and claims summary |
| POST | `/api/vessels` | Create vessel |
| PUT | `/api/vessels/{id}` | Update vessel |
| DELETE | `/api/vessels/{id}` | Delete vessel (fails if active policies exist) |

### Policies

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/policies` | List policies (paginated, filterable by status, coverage type, vessel) |
| GET | `/api/policies/{id}` | Get policy with claims |
| POST | `/api/policies` | Create policy (premium auto-calculated) |
| PUT | `/api/policies/{id}` | Update policy (only in Draft status) |
| PATCH | `/api/policies/{id}/status` | Transition policy status |

### Claims

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/claims` | List claims (paginated, filterable by status, category, date range) |
| GET | `/api/claims/{id}` | Get claim with history |
| POST | `/api/claims` | File new claim |
| PATCH | `/api/claims/{id}/status` | Transition claim status (with notes) |
| PATCH | `/api/claims/{id}/settle` | Set settlement amount and mark settled |

### Dashboard

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/dashboard/summary` | Aggregate metrics (counts, totals) |
| GET | `/api/dashboard/claims-by-category` | Claims grouped by category |
| GET | `/api/dashboard/policies-by-coverage` | Policies grouped by coverage type |
| GET | `/api/dashboard/claims-trend` | Monthly claims count (trailing 12 months) |

All list endpoints accept `?vesselType=Tanker` for dashboard filtering.

### Request/Response Examples

**POST `/api/claims`**
```json
// Request
{
  "policyId": 5,
  "category": "Collision",
  "incidentDate": "2026-03-10",
  "description": "Vessel struck dock during berthing in heavy winds",
  "estimatedAmount": 250000.00
}

// Response (201 Created)
{
  "data": {
    "id": 41,
    "claimNumber": "CLM-2026-00041",
    "policyId": 5,
    "category": "Collision",
    "status": "Reported",
    "incidentDate": "2026-03-10",
    "description": "Vessel struck dock during berthing in heavy winds",
    "estimatedAmount": 250000.00,
    "settledAmount": null,
    "createdAt": "2026-04-15T10:30:00Z"
  },
  "errors": []
}
```

**PATCH `/api/claims/41/status`**
```json
// Request
{
  "newStatus": "UnderReview",
  "notes": "Assigned to adjuster. Awaiting surveyor report."
}

// Response (200 OK)
{
  "data": {
    "id": 41,
    "claimNumber": "CLM-2026-00041",
    "status": "UnderReview",
    "history": [
      {
        "fromStatus": "Reported",
        "toStatus": "UnderReview",
        "notes": "Assigned to adjuster. Awaiting surveyor report.",
        "changedAt": "2026-04-15T11:00:00Z"
      }
    ]
  },
  "errors": []
}
```

### Error Response Format

```json
{
  "data": null,
  "errors": [
    { "field": "incidentDate", "message": "Incident date cannot be in the future" },
    { "field": "policyId", "message": "Policy is not active" }
  ]
}
```

---

## 7. Testing Strategy

### Unit Tests (xUnit + Moq)

**Service layer** — the primary target for unit tests:

- `PremiumCalculator`: Verify premium formula for each vessel type / coverage type combination
- `ClaimWorkflowService`: Test all valid and invalid status transitions (Reported → Under Review: valid; Settled → Reported: invalid)
- `PolicyWorkflowService`: Test status transitions and business rules (can't delete vessel with active policies, can't file claim on expired policy)
- `Vessel/Policy/Claim validation`: Required fields, IMO number format, date range validity

**Target: 40+ unit tests** covering all business logic and validation rules.

### Integration Tests (WebApplicationFactory)

Use ASP.NET Core's `WebApplicationFactory<Program>` with an in-memory database or test container:

- Full API request/response cycle for each endpoint
- Verify pagination, filtering, and sorting behavior
- Verify error responses for invalid input
- Verify cascade behavior (claim history created on status change)

**Target: 15+ integration tests** covering API contract.

### What's Not Tested

- Vue.js components (no Jest/Vitest setup — keep scope focused on backend strength)
- Razor views (tested implicitly through integration tests hitting MVC routes)
- Docker infrastructure (tested by running `docker compose up`)

---

## 8. Infrastructure & Deployment

### Docker Compose Services

| Service | Image | Port | Purpose |
|---------|-------|------|---------|
| `web` | Custom Dockerfile (.NET 8 SDK → runtime) | 5000 | ASP.NET Core application |
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` | 1433 | SQL Server database |

### docker-compose.yml Overview

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "GardDemo@2026"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/lib/mssql/data
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "GardDemo@2026" -Q "SELECT 1" -C
      interval: 10s
      timeout: 5s
      retries: 5

  web:
    build: .
    ports:
      - "5000:8080"
    environment:
      ConnectionStrings__DefaultConnection: "Server=db;Database=GardPortal;User Id=sa;Password=GardDemo@2026;TrustServerCertificate=True"
    depends_on:
      db:
        condition: service_healthy

volumes:
  sqldata:
```

### Startup Behavior

1. SQL Server starts and passes health check
2. ASP.NET Core application starts
3. EF Core applies pending migrations automatically (`Database.Migrate()`)
4. Seed data is inserted if the database is empty
5. Application is accessible at `http://localhost:5000`

No cloud accounts, no API keys, no external dependencies.

---

## 9. Project Structure

```
Gard-Developer-460004846/
├── docs/
│   └── PRD.md
├── src/
│   └── GardPortal/                      # ASP.NET Core MVC project
│       ├── GardPortal.csproj
│       ├── Program.cs                    # App configuration and startup
│       ├── Controllers/
│       │   ├── VesselsController.cs      # MVC controller (Razor views)
│       │   ├── PoliciesController.cs
│       │   ├── ClaimsController.cs
│       │   ├── DashboardController.cs    # Serves the Vue.js dashboard page
│       │   └── Api/
│       │       ├── VesselsApiController.cs
│       │       ├── PoliciesApiController.cs
│       │       ├── ClaimsApiController.cs
│       │       └── DashboardApiController.cs
│       ├── Models/
│       │   ├── Vessel.cs
│       │   ├── Policy.cs
│       │   ├── Claim.cs
│       │   ├── ClaimHistory.cs
│       │   └── Enums.cs
│       ├── Services/
│       │   ├── IVesselService.cs
│       │   ├── VesselService.cs
│       │   ├── IPolicyService.cs
│       │   ├── PolicyService.cs
│       │   ├── IClaimService.cs
│       │   ├── ClaimService.cs
│       │   ├── IPremiumCalculator.cs
│       │   ├── PremiumCalculator.cs
│       │   └── IDashboardService.cs
│       │   └── DashboardService.cs
│       ├── Data/
│       │   ├── GardDbContext.cs
│       │   ├── Migrations/
│       │   └── SeedData.cs
│       ├── ViewModels/                   # View-specific models for Razor
│       │   ├── VesselListViewModel.cs
│       │   ├── PolicyFormViewModel.cs
│       │   └── ClaimDetailViewModel.cs
│       ├── DTOs/                         # API request/response shapes
│       │   ├── ApiResponse.cs
│       │   ├── VesselDto.cs
│       │   ├── PolicyDto.cs
│       │   ├── ClaimDto.cs
│       │   └── DashboardDto.cs
│       ├── Views/
│       │   ├── Shared/
│       │   │   ├── _Layout.cshtml
│       │   │   └── _ValidationScripts.cshtml
│       │   ├── Vessels/
│       │   │   ├── Index.cshtml
│       │   │   ├── Details.cshtml
│       │   │   └── Create.cshtml
│       │   ├── Policies/
│       │   │   ├── Index.cshtml
│       │   │   ├── Details.cshtml
│       │   │   └── Create.cshtml
│       │   ├── Claims/
│       │   │   ├── Index.cshtml
│       │   │   ├── Details.cshtml
│       │   │   └── Create.cshtml
│       │   └── Dashboard/
│       │       └── Index.cshtml          # Hosts Vue.js SPA
│       └── wwwroot/
│           ├── css/
│           │   └── site.css
│           └── js/
│               └── dashboard/            # Vue.js app (built via Vite)
│                   ├── package.json
│                   ├── tsconfig.json
│                   ├── vite.config.ts
│                   └── src/
│                       ├── App.vue
│                       ├── main.ts
│                       ├── components/
│                       │   ├── SummaryCards.vue
│                       │   ├── ClaimsByCategory.vue
│                       │   ├── PoliciesByCoverage.vue
│                       │   ├── ClaimsTrend.vue
│                       │   └── VesselTypeFilter.vue
│                       └── services/
│                           └── api.ts
├── tests/
│   └── GardPortal.Tests/
│       ├── GardPortal.Tests.csproj
│       ├── Unit/
│       │   ├── PremiumCalculatorTests.cs
│       │   ├── ClaimWorkflowTests.cs
│       │   ├── PolicyWorkflowTests.cs
│       │   └── ValidationTests.cs
│       └── Integration/
│           ├── VesselsApiTests.cs
│           ├── PoliciesApiTests.cs
│           ├── ClaimsApiTests.cs
│           └── DashboardApiTests.cs
├── Dockerfile
├── docker-compose.yml
├── GardPortal.sln
└── README.md
```

### Module Responsibilities

| Directory | What Lives Here |
|-----------|----------------|
| `Controllers/` | HTTP handling only — delegates to services immediately |
| `Controllers/Api/` | JSON API endpoints for the Vue.js dashboard and external consumers |
| `Services/` | All business logic. Interface + implementation pairs for DI and testability |
| `Models/` | EF Core entity classes. No logic beyond computed properties |
| `Data/` | DbContext, migrations, seed data |
| `ViewModels/` | Razor-specific presentation models (never leaked to API layer) |
| `DTOs/` | API request/response shapes with validation attributes |
| `Views/` | Razor `.cshtml` templates |
| `wwwroot/js/dashboard/` | Self-contained Vue.js 3 + TypeScript app, built with Vite, output copied to `wwwroot/js/dashboard/dist/` |
| `tests/Unit/` | Fast tests with mocked dependencies |
| `tests/Integration/` | Tests that hit the full HTTP pipeline with `WebApplicationFactory` |
