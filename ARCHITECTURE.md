# Architecture Documentation

## Solution Structure

```
claims-module/
├── backend/
│   ├── src/
│   │   ├── ClaimsModule.Domain/          # Pure domain — no framework deps
│   │   ├── ClaimsModule.Application/     # CQRS, validators, interfaces, DTOs
│   │   ├── ClaimsModule.Infrastructure/  # Storage, Hangfire, CurrentUserService
│   │   ├── ClaimsModule.Persistence/     # EF Core, repositories, migrations
│   │   └── ClaimsModule.API/             # Controllers, middleware, Program.cs
│   └── tests/
│       └── ClaimsModule.Application.Tests/
├── frontend/                             # Angular 21 + Material
├── docker-compose.yml
├── netlify.toml
└── .github/workflows/
```

**Dependency rule (Clean Architecture):**
```
API → Application ← Domain
 ↓         ↓
Infrastructure  Persistence
```

Domain has zero external dependencies. Application defines interfaces that Infrastructure and Persistence implement.

---

## Data Model

### Core Entities

**`Claim`** — aggregate root. Owns the full lifecycle: parties, risk objects, reserves, documents. All mutations go through domain methods (`TransitionTo`, `AddParty`, `AddDocument`), never via direct property sets.

**`LossEvent`** — 1:1 with Claim. The incident that originated the claim (date, location, cause code, description).

**`ClaimParty`** — people or companies involved (Claimant, Insured, ThirdParty, Witness, Attorney). At least one active Claimant required to open a claim.

**`ClaimReserveComponent`** — one per coverage type per claim (Indemnity, Expense, ALAE, SubrogationRecoverable). `CurrentAmount` is computed from history, never updated directly.

**`ReserveHistory`** — append-only transaction log (event-sourcing pattern). The current reserve balance is the sum of all approved/posted transactions. Records are never updated or deleted.

**`ClaimAuditLog`** — immutable event log. No UPDATE or DELETE ever. Every significant business action produces an entry.

### Entity Relationships

```
Claim ─── 1:1 ─── LossEvent
  │
  ├── 1:N ─── ClaimParty
  ├── 1:N ─── ClaimRiskObject
  ├── 1:N ─── ClaimDocument
  └── 1:N ─── ClaimReserveComponent
                    └── 1:N ─── ReserveHistory

CauseOfLossCode  (reference / lookup)
Policy           (simulated, seeded)
ClaimAuditLog    (append-only event log)
```

### Key Data Conventions
- **Primary Keys**: `UNIQUEIDENTIFIER` with `NEWSEQUENTIALID()` default (avoids index fragmentation)
- **Monetary values**: `DECIMAL(19,4)` — never `float` or `money`
- **Timestamps**: `DATETIMEOFFSET(7)` UTC
- **Soft delete**: `IsDeleted BIT + DeletedAt` on all entities; global EF query filters
- **Tenant isolation**: `OrganisationId UNIQUEIDENTIFIER NOT NULL` on every table
- **Optimistic concurrency**: `ROWVERSION` on `Claim` and `ClaimReserveComponent`

---

## CQRS Flow

Every operation goes through MediatR with a three-stage pipeline:

```
HTTP Request
    ↓
Controller (thin — only maps HTTP to MediatR)
    ↓
MediatR Pipeline:
  1. LoggingBehavior      — structured log per request
  2. ValidationBehavior   — runs FluentValidation async; throws on failure → 422
  3. TransactionBehavior  — wraps Commands in DB transaction (not Queries)
    ↓
Command/Query Handler
    ↓
Domain Entity (business logic lives here)
    ↓
Repository → DbContext → SQL Server
```

**Example: Create Claim**
```
POST /api/claims
  → ClaimsController.Create(CreateClaimCommand)
  → ValidationBehavior runs CreateClaimCommandValidator
      - LossDate not in future
      - LossDescription >= 20 chars
      - CauseOfLossCode exists and active (async DB check)
  → TransactionBehavior begins DB transaction
  → CreateClaimCommandHandler:
      1. Generate claim number atomically (UPDATE…OUTPUT on sequence table)
      2. Claim.Create(claimNumber, orgId, userId) — raises ClaimCreatedEvent
      3. Set LossEvent, Parties, RiskObjects on aggregate
      4. claimRepository.AddAsync(claim)
      5. unitOfWork.SaveChangesAsync()
  → TransactionBehavior commits
  → Response: 201 Created { claimId, claimNumber, warnings[] }
```

---

## Domain Events

Domain events are raised inside aggregate methods and stored in-memory until `SaveChangesAsync`. Infrastructure then processes them to write audit log entries.

| Event | Raised by | Effect |
|---|---|---|
| `ClaimCreatedEvent` | `Claim.Create()` | Audit log: CLAIM_CREATED |
| `ClaimStatusChangedEvent` | `Claim.TransitionTo()` | Audit log: STATUS_CHANGED |
| `ReserveCreatedEvent` | `CreateReserveCommandHandler` | Audit log: RESERVE_CREATED |
| `ReserveApprovedEvent` | `ApproveReserveCommandHandler` | Audit log: RESERVE_APPROVED + enqueue GL job |
| `ReserveRejectedEvent` | `RejectReserveCommandHandler` | Audit log: RESERVE_REJECTED |
| `DocumentUploadedEvent` | `Claim.AddDocument()` | Audit log: DOCUMENT_UPLOADED |

**Why this pattern:** `Claim` doesn't know about audit logging, Hangfire, or email. It signals "something happened" — infrastructure decides what to do. This keeps domain logic pure and testable.

---

## Hangfire Job Design

### GL Posting Simulation (`PostGLReserveChangeJob`)

**Trigger:** Enqueued immediately after reserve auto-approval or manual approval.

**Idempotency key:** `Reserve:{ReserveId}:Change:{ChangeSequence}`

**Re-entrancy guarantee:**
```csharp
// Before any write, check if already posted
if (history.PostingStatus == PostingStatus.Posted)
    return; // no-op — idempotent

// Only then write the audit entry and update status
history.PostingStatus = PostingStatus.Posted;
await auditLogService.LogAsync(..., "GL_POSTING_SIMULATED");
```

If the job fails after all retries (default: 3), it sets `PostingStatus = Failed` and writes `GL_POSTING_FAILED` to the audit log. The job can be manually retried from the Hangfire dashboard.

**Simulated journal entry:**
```
DR Change in Outstanding Reserves / CR Outstanding Loss Reserves, Amount = $25,000.00
```

### SLA Monitoring (`SlaMonitoringJob`)

**Schedule:** `*/15 * * * *` (every 15 minutes)

**Logic:**
1. Find all claims with `Status IN (Draft, Open)` and `UpdatedAt < now - 48h`
2. For each, check last `SLA_BREACH_DETECTED` entry — skip if within 24h (deduplication)
3. Write `SLA_BREACH_DETECTED` audit log entry
4. Does NOT change claim status

**Deduplication rationale:** Without it, the job would write a new entry every 15 minutes for stale claims indefinitely.

---

## Storage Architecture

Two implementations behind `IStorageService`:

```
IStorageService
  ├── LocalFileSystemStorageService  ← selected when StorageProvider=LocalFileSystem
  └── AzureBlobStorageService        ← selected when StorageProvider=AzureBlob
```

**Blob path structure:**
```
claim-documents/{organisationId}/{claimId}/{sanitisedFilename}
```

**Document retrieval:** API generates a SAS URL with 1-hour TTL. The browser downloads the file directly from Azure Blob Storage — the API never proxies bytes.

**Local fallback:** For Docker development, files are stored in `/app/uploads/` volume. The download URL points back to the API which serves the file locally.

---

## Azure Architecture

```
Internet
    │
    ├──→ Netlify CDN ──→ Angular SPA (static files)
    │         │
    │         └──→ HTTPS API calls ──→ Azure App Service (Linux, .NET 9)
    │                                       │
    │                                       ├──→ Azure SQL Database (Basic 5 DTU)
    │                                       │      Central US (regional restriction bypass)
    │                                       │
    │                                       └──→ Azure Blob Storage
    │                                              Standard LRS, West Europe
    │
    └──→ GitHub Actions CI/CD
              ├── Build & test backend
              ├── Run EF migrations
              ├── Deploy to App Service
              └── Build & deploy frontend to Netlify
```

---

## Key Design Decisions

### Reserve Event-Sourcing
**Decision:** Never UPDATE a reserve record. INSERT a new transaction each time.
**Why:** The spec explicitly states this requirement. Event-sourcing gives a complete audit trail of every financial change with no data loss. The `CurrentAmount` on `ClaimReserveComponent` is a computed projection kept in sync for query performance.

### Claim Number Generation
**Decision:** Dedicated `ClaimNumberSequence` table with `UPDATE … OUTPUT INSERTED` atomic increment.
**Why:** Format `CLM-{YYYY}-{7-digit}` requires sequential, gap-free numbers. Database SEQUENCE objects or application-level counters under concurrent load can produce duplicates or gaps. The `UPDATE` approach is atomic by SQL Server's row-level locking.

### Validation in MediatR Pipeline, Not Controllers
**Decision:** FluentValidation registered as `IPipelineBehavior`, runs before every command handler.
**Why:** Controllers are infrastructure. Business rules belong in the application layer. This means validation runs regardless of how the command is triggered (HTTP, background job, test). The `ValidationBehavior` calls `ValidateAsync` to support async validators (e.g., database existence checks).

### Self-Approval Guard at Handler Level
**Decision:** `ApproveReserveCommandHandler` compares `history.SubmittedByUserId != currentUser.UserId`.
**Why:** BR-R-03 is a business rule, not a UI concern. Enforcing it only in the frontend would allow bypassing via direct API calls. The handler is the last line of defense.

### `IStorageService` Abstraction
**Decision:** Interface in Application layer, two implementations in Infrastructure.
**Why:** The Application layer cannot depend on Azure SDK. The interface allows:
- Swapping Azure → Local for Docker development
- Testing with a mock/stub
- Future migration to S3 or GCS without changing business logic

### TransactionBehavior Only on Commands
**Decision:** `TransactionBehavior` checks for `ICommand<TResponse>` marker interface — Queries skip it.
**Why:** Queries are read-only. Wrapping them in transactions adds overhead and can cause lock contention. Commands that change state need transaction guarantees.
