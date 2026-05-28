# Claims Management System — DICEUS Technical Assessment

A fullstack enterprise Claims Management System implementing FNOL Intake and Reserve Management workflows, built with .NET 9 Clean Architecture and Angular 21.

---

## Live Deployment

| Service | URL |
|---|---|
| **Frontend** | https://claims-module-abc123.netlify.app *(update with actual URL)* |
| **Backend API / Swagger** | https://claims-module-api.azurewebsites.net/swagger |
| **Hangfire Dashboard** | https://claims-module-api.azurewebsites.net/hangfire |

### Test Credentials

| Role | Email | Password |
|---|---|---|
| Claims Handler | `handler@claims.io` | `Handler123!` |
| Claims Supervisor | `supervisor@claims.io` | `Supervisor123!` |
| Claims Manager | `manager@claims.io` | `Manager123!` |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 9 / C# 13 |
| API | ASP.NET Core Web API |
| CQRS | MediatR 12 |
| ORM | Entity Framework Core 9 |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 13 |
| Background Jobs | Hangfire 1.8 |
| Database | SQL Server 2022 |
| File Storage | Azure Blob Storage / Local Filesystem |
| Frontend | Angular 21 + Angular Material |
| Auth | JWT Bearer (mock) |

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [Node.js 22+](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com)

---

## Local Development Setup

### 1. Clone the repository

```bash
git clone https://github.com/asyafl/claims-module.git
cd claims-module
```

### 2. Run with Docker Compose (recommended)

```bash
docker compose up --build
```

This starts:
- **API** on http://localhost:5000 (Swagger at http://localhost:5000/swagger)
- **SQL Server** on localhost:1433
- **Frontend** on http://localhost:4201
- **Hangfire Dashboard** at http://localhost:5000/hangfire

Migrations and seed data are applied automatically on first start.

### 3. Run locally without Docker

**Backend:**
```bash
cd backend
dotnet restore
dotnet ef database update \
  --project src/ClaimsModule.Persistence \
  --startup-project src/ClaimsModule.API
dotnet run --project src/ClaimsModule.API
```

**Frontend:**
```bash
cd frontend
npm install
ng serve
```
Frontend available at http://localhost:4200.

---

## Environment Variables

All configuration is in `backend/src/ClaimsModule.API/appsettings.json`. For production, these are set as Azure App Service Application Settings:

| Key | Description | Default (local) |
|---|---|---|
| `ConnectionStrings__Default` | SQL Server connection string | Docker SQL Server |
| `StorageProvider` | `LocalFileSystem` or `AzureBlob` | `LocalFileSystem` |
| `AzureBlob__ConnectionString` | Azure Blob Storage connection string | *(empty)* |
| `AzureBlob__Container` | Blob container name | `claim-documents` |
| `Jwt__Secret` | JWT signing key (min 256 bits) | see appsettings.json |
| `Jwt__Issuer` | JWT issuer | `claims-module` |
| `OrganisationId` | Fixed tenant GUID | `00000000-0000-0000-0000-000000000001` |
| `Hangfire__Dashboard` | Enable Hangfire UI | `true` |

---

## Database Migrations

```bash
cd backend

# Apply all migrations
dotnet ef database update \
  --project src/ClaimsModule.Persistence \
  --startup-project src/ClaimsModule.API

# Create a new migration
dotnet ef migrations add <MigrationName> \
  --project src/ClaimsModule.Persistence \
  --startup-project src/ClaimsModule.API \
  --output-dir Migrations
```

Seed data is applied automatically via EF Core `HasData`:
- 10 Cause of Loss Codes
- 5 simulated Policies
- 6 Claims with all lifecycle statuses (Draft → Closed)

---

## Azure Deployment

### Infrastructure

| Resource | Name | Tier |
|---|---|---|
| Resource Group | `claims-module-rg` | — |
| App Service Plan | `claims-plan` | F1 (Free) |
| App Service | `claims-module-api` | .NET 9 Linux |
| Azure SQL Server | `claims-sql-final` | West Europe → Central US |
| Azure SQL Database | `ClaimsModule` | Basic 5 DTU |
| Storage Account | `claimsstore2026` | Standard LRS |
| Blob Container | `claim-documents` | — |
| Frontend | Netlify | Free |

### CI/CD Pipeline

Two GitHub Actions workflows:

**`deploy-backend.yml`** — triggers on push to `main` (backend changes):
1. Setup .NET 9
2. Restore & Build
3. Run EF Core migrations against Azure SQL
4. Publish & Deploy to Azure App Service

**`deploy-frontend.yml`** — triggers on push to `main` (frontend changes):
1. Setup Node.js 22
2. `npm ci` + `npm run build --configuration=production`
3. Deploy via SWA CLI to Netlify

---

## Application Walkthrough

### Claims Dashboard
Lists all claims with paginated table, status color badges, and filters by status/date/cause of loss. Click any row to open the Claim Detail screen.

### FNOL Intake (3-step form)
1. **Policy & Loss Details** — typeahead policy search, loss date, cause of loss code
2. **Parties & Risk Objects** — add claimants, witnesses; add affected assets
3. **Initial Reserve & Review** — optional reserve with real-time authority threshold indicator

### Claim Detail (5 tabs)
- **Overview** — loss event details, claim metadata
- **Parties** — all parties with role badges
- **Reserves** — component cards, transaction history, Approve/Reject (supervisor+)
- **Documents** — upload/download with Azure Blob Storage
- **Audit Log** — immutable chronological event trail

### Role-Based Access
- `handler` — create claims, upload documents, open reserves ≤ $10,000
- `supervisor` — approve/reject reserves up to $100,000
- `manager` — approve/reject reserves up to $10,000,000, set override flag
