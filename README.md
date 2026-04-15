# Gard Claims & Policy Portal

A full-stack marine insurance management application built with ASP.NET Core MVC, Entity Framework Core, and Vue.js. Designed for underwriters and claims handlers to manage vessel registries, insurance policies, and claims processing through a unified interface.

## Tech Stack

- **Backend:** C# 12, .NET 8, ASP.NET Core MVC
- **ORM:** Entity Framework Core 8 (Code-First + Migrations)
- **Database:** Microsoft SQL Server 2022
- **Frontend:** Razor Views (server-rendered CRUD) + Vue.js 3 (interactive dashboard)
- **Charts:** Chart.js 4
- **Styling:** Bootstrap 5
- **Testing:** xUnit, Moq, FluentAssertions
- **Infrastructure:** Docker, Docker Compose
- **CI:** GitHub Actions

## Features

- **Vessel Registry** -- Full CRUD for insured vessels with IMO number validation, sortable/paginated lists, and cascade visibility into linked policies and claims
- **Policy Administration** -- Create and manage insurance policies (P&I, Hull & Machinery, Cargo, Loss of Hire) with automatic premium calculation based on vessel type and coverage rates
- **Claims Processing** -- File claims against active policies with a full status workflow (Reported -> Under Review -> Approved -> Settled/Denied), complete audit trail, and settlement tracking
- **Interactive Dashboard** -- Vue.js SPA with Chart.js visualizations: summary KPI cards, claims by category (bar chart), policies by coverage (doughnut chart), monthly claims trend (line chart), all filterable by vessel type
- **REST API** -- Full JSON API (`/api/vessels`, `/api/policies`, `/api/claims`, `/api/dashboard`) with pagination, filtering, consistent error envelope, and proper HTTP status codes

## Quick Start

```bash
docker compose up --build
```

The application will be available at **http://localhost:5000**.

The database is automatically seeded with realistic demo data: 15+ vessels, 25+ policies across all coverage types and statuses, and 40+ claims with varied categories and date distributions. The dashboard shows compelling analytics out of the box.

## Project Structure

```
src/GardPortal/
  Controllers/          MVC controllers (Razor views)
  Controllers/Api/      RESTful JSON API controllers
  Models/               EF Core entity classes
  Services/             Business logic (interface + implementation)
  Data/                 DbContext, migrations, seed data
  DTOs/                 API request/response shapes
  ViewModels/           Razor-specific presentation models
  Views/                Razor .cshtml templates
  wwwroot/              Static assets (CSS, Vue.js dashboard)
tests/GardPortal.Tests/
  Unit/                 Service layer unit tests (55+ tests)
  Integration/          API integration tests with WebApplicationFactory
```

## Premium Calculation

Premium = Insured Value x Base Rate x Vessel Type Factor

| Coverage Type | Base Rate |
|--------------|-----------|
| P&I | 0.25% |
| Hull & Machinery | 0.40% |
| Cargo | 0.15% |
| Loss of Hire | 0.20% |

| Vessel Type | Factor |
|------------|--------|
| Bulk Carrier | 1.00 |
| Container Ship | 1.10 |
| RoRo | 1.05 |
| General Cargo | 0.95 |
| Tanker | 1.30 |
| Offshore | 1.40 |
| Passenger Ferry | 1.20 |

## API Examples

```bash
# List vessels
curl http://localhost:5000/api/vessels?page=1&pageSize=10

# Create a policy
curl -X POST http://localhost:5000/api/policies \
  -H "Content-Type: application/json" \
  -d '{"vesselId": 1, "coverageType": "HullAndMachinery", "startDate": "2026-01-01", "endDate": "2026-12-31", "insuredValue": 5000000}'

# File a claim
curl -X POST http://localhost:5000/api/claims \
  -H "Content-Type: application/json" \
  -d '{"policyId": 1, "category": "Collision", "incidentDate": "2026-03-10", "description": "Vessel struck dock during berthing", "estimatedAmount": 250000}'

# Transition claim status
curl -X PATCH http://localhost:5000/api/claims/1/status \
  -H "Content-Type: application/json" \
  -d '{"newStatus": "UnderReview", "notes": "Assigned to adjuster"}'

# Dashboard summary
curl http://localhost:5000/api/dashboard/summary
```

## Running Tests

```bash
dotnet test tests/GardPortal.Tests
```

Tests use EF Core InMemory provider -- no SQL Server instance required.

## Architecture Decisions

- **Single MVC application** rather than microservices -- right-sized for the domain scope
- **Dual rendering**: server-rendered Razor views for CRUD operations, Vue.js SPA for the analytics dashboard
- **Service layer with DI**: all business logic behind interfaces for testability
- **Status state machines**: policy and claim workflows enforce valid transitions, preventing invalid state changes
- **Append-only audit trail**: every claim status transition is logged with timestamp, previous status, and notes
- **Docker-first**: single `docker compose up` gets everything running with seeded data
