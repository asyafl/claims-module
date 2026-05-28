# AI Workflow Report

## Tools Used

| Tool | Usage |
|---|---|
| **Claude Code (claude-sonnet-4-6)** | Primary tool — architecture design, full backend implementation, Angular frontend, debugging, documentation |
| **GitHub Copilot** | Inline autocompletion during manual refinements |

The primary workflow was Claude Code running as an interactive agent within VS Code, with full access to the filesystem and terminal. This allowed Claude to scaffold projects, install NuGet packages, run builds, execute migrations, and verify correctness end-to-end without switching contexts.

---

## Workflow Structure

### Phase 1 — Architecture & Scope Analysis
Before writing any code, I loaded both specification documents (DICEUS_Fullstack_Technical_Assessment.docx and Claims_Module_Candidate_Specification.docx) and asked Claude to:
1. Summarize what needs to be built
2. Propose a Clean Architecture solution structure
3. Identify non-obvious design decisions (reserve event-sourcing, claim number generation, state machine)

The output was reviewed and refined before any code was written.

### Phase 2 — Backend Layer by Layer
Implementation followed the dependency graph:
1. Domain (no deps) → 2. Application (interfaces) → 3. Persistence → 4. Infrastructure → 5. API

Each layer was built, compiled, and verified before moving to the next. Claude was provided the full spec text as context for business rules.

### Phase 3 — Database & Seed Data
EF Core configurations were generated with explicit attention to the spec's conventions (DECIMAL(19,4), ROWVERSION, soft delete, OrganisationId). Migrations were created and verified against a live SQL Server in Docker.

### Phase 4 — Frontend
Angular modules, components, services, and interceptors were scaffolded with Claude, then visually verified using Playwright screenshots after every significant change.

### Phase 5 — Azure Deployment & CI/CD
Infrastructure was provisioned via Azure CLI in Cloud Shell. GitHub Actions workflows were written, debugged iteratively based on actual pipeline failure logs.

### Phase 6 — Documentation
README, ARCHITECTURE, and AI-WORKFLOW documents written collaboratively.

---

## Representative Prompts

### Prompt 1 — Architecture Design
**Purpose:** Establish the solution structure before writing code.

> *"We're implementing a vertical slice of an insurance Claims Management System for a DICEUS technical assessment. The spec requires Clean Architecture with MediatR CQRS, EF Core 9, FluentValidation, Hangfire, and Azure Blob Storage. Design the project structure with all 5 layers, identify which entities are aggregate roots, and explain the key design decisions for: (1) reserve event-sourcing, (2) atomic claim number generation, (3) state machine enforcement. The spec says reserves use append-only history — explain the architectural implication."*

**Why this worked:** Rich domain context + specific design questions produced architectural reasoning, not just boilerplate.

### Prompt 2 — Domain Entity with Business Rules
**Purpose:** Implement the Claim aggregate with the state machine.

> *"Implement the Claim aggregate root in C#. It must: (1) enforce the state machine transitions from the spec (Draft→Open→UnderInvestigation→PendingPayment→Closed, etc.), (2) throw DomainException for invalid transitions, (3) check closure conditions before allowing →Closed, (4) raise ClaimCreatedEvent and ClaimStatusChangedEvent as domain events stored in-memory. Use private set for Status and ClaimNumber so they can only change through domain methods."*

**Why this worked:** Specific technical constraints + business rules + explicit requirement for domain events led to correct, testable code.

### Prompt 3 — FluentValidation Async Validator
**Purpose:** Implement the CreateClaim validator with async DB checks.

> *"Write a FluentValidation AbstractValidator for CreateClaimCommand. Rules: LossDate cannot be future (Critical), LossDescription min 20 chars (Critical), CauseOfLossCode must exist and be active in DB via async IReferenceRepository call (Critical). The validator runs in a MediatR pipeline — explain why ValidateAsync must be called instead of Validate to avoid AsyncValidatorInvokedSynchronouslyException."*

**Why this worked:** Asking Claude to explain the WHY behind the async call caught a bug before it happened (and it did happen in the initial build).

### Prompt 4 — Debugging Angular NG0100
**Purpose:** Fix ExpressionChangedAfterItHasBeenCheckedError preventing claims list from rendering.

> *"The Angular ClaimsListComponent loads fine (API returns 200 with data confirmed by Playwright network capture) but the table shows 0 rows. Console has NG0100: ExpressionChangedAfterItHasBeenCheckedError on totalCount. The component initializes loading=false then sets loading=true in ngOnInit which triggers the error. Fix this without changing the component's public interface."*

**Context mattered:** Providing the Playwright network capture output proved the API was working, so Claude focused on the Angular side rather than suggesting to debug the API.

---

## AI-Generated vs Manually Designed

| Component | Origin | Notes |
|---|---|---|
| Clean Architecture project structure | AI-suggested, human-approved | Reviewed against spec before implementing |
| Domain entities and value objects | AI-generated, human-refined | Reviewed private setters, naming |
| State machine logic in `Claim.TransitionTo` | AI-generated | Verified against FRS Section 4.2 transition table |
| Business rules (BR-C-01..07, BR-R-01..06) | AI-generated from spec | Each rule cross-checked against specification |
| EF Core Fluent API configurations | AI-generated | Verified DECIMAL precision, ROWVERSION, soft delete |
| Seed data (claims, policies, cause codes) | AI-generated | Verified GUIDs are stable, data matches spec |
| MediatR pipeline behaviors | AI-generated | Reviewed transaction scope (commands only) |
| Hangfire idempotency logic | AI-generated | Verified idempotency key format matches spec |
| Angular components (HTML + SCSS) | AI-generated | Visually verified via Playwright screenshots |
| Azure CLI provisioning commands | AI-generated | Debugged iteratively (provider registration, region restrictions) |
| This documentation | Collaborative | Structure AI, content human-verified |

---

## Where AI Got It Wrong — Corrections

### Error 1: Synchronous FluentValidation in MediatR Pipeline

**What AI generated:**
```csharp
var failures = validators
    .Select(v => v.Validate(context))  // synchronous
    .SelectMany(r => r.Errors)
    .ToList();
```

**What happened:** `AsyncValidatorInvokedSynchronouslyException` at runtime because `CreateClaimCommandValidator` uses `MustAsync` for the CauseOfLossCode database check.

**How I found it:** Build succeeded but first API call threw the exception with a clear message.

**Fix:**
```csharp
var results = await Task.WhenAll(
    validators.Select(v => v.ValidateAsync(context, cancellationToken)));
```

**Lesson:** AI generated the correct-looking synchronous pattern from training data. The async version requires knowing that MediatR pipeline behaviors can and should use `await`. The fix was straightforward once the error message was read carefully.

---

### Error 2: AutoMapper `IReadOnlyList<T>` Mapping Failure

**What AI generated:**
```csharp
// In ClaimProfile:
ctx.Mapper.Map<IReadOnlyList<ClaimPartyDto>>(src.Parties)
```

**What happened:** `AutoMapperMappingException: Missing type map configuration` at runtime. AutoMapper cannot map to `IReadOnlyList<T>` — it only supports `List<T>` as the target collection type.

**How I found it:** Error appeared on the policies search endpoint during smoke testing with Playwright network capture.

**Fix:**
```csharp
ctx.Mapper.Map<List<ClaimPartyDto>>(src.Parties)
```
And all DTOs changed from `IReadOnlyList<T>` to `List<T>` for mapped properties.

**Additionally:** AutoMapper's `ConstructUsing` on records tries to map properties after construction. Fixed by adding `.ForAllMembers(o => o.Ignore())` to all profile mappings.

**Lesson:** AI used `IReadOnlyList` throughout because it's the idiomatic C# return type. The AutoMapper limitation is not obvious from the interface — it required runtime testing to discover.

---

### Error 3: `SqlServerRetryingExecutionStrategy` with Manual Transactions

**What AI generated:**
```csharp
options.UseSqlServer(connStr,
    b => b.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
```

**What happened:** `InvalidOperationException: The configured execution strategy does not support user-initiated transactions`.

**Root cause:** `EnableRetryOnFailure` wraps operations in its own retry strategy, which is incompatible with the `TransactionBehavior` that calls `BeginTransactionAsync` manually.

**Fix:** Removed `EnableRetryOnFailure`. For assessment context (Docker + Azure with reliable connections), retry-on-failure adds no value and breaks the unit-of-work pattern.

**Lesson:** AI added retry logic as a "best practice" without considering the interaction with the transaction behavior pattern in the same codebase. Testing in a real running environment caught this immediately.

---

## Which Parts Benefited Most from AI

**High value:**
- **Boilerplate at scale:** 10+ EF Core configurations with identical patterns (HasKey, HasDefaultValueSql, HasPrecision, HasQueryFilter) — AI generated these consistently and correctly
- **MediatR wiring:** Registering behaviors, validators, and handlers with correct generic constraints
- **AutoMapper profiles:** Complex nested mappings with ConstructUsing
- **Angular module declarations:** Material imports, lazy-loaded routing, interceptor registration

**Medium value:**
- **Domain logic:** AI generated correct structure but business rules required careful cross-checking against the spec. Every `BR-C-XX` and `BR-R-XX` rule was verified manually.
- **Hangfire jobs:** The idempotency pattern was correct but required reviewing the interaction with EF Core's change tracker

**Lower value (required significant manual work):**
- **Azure deployment:** Every step required manual debugging (provider registration, regional SQL restrictions, basic auth disabled, Static Web App Docker action failure). AI provided commands but couldn't predict environment-specific failures.
- **Angular change detection:** The `NG0100` issue and `ChangeDetectorRef.markForCheck()` solution required understanding Angular's change detection cycle — AI's first suggestions didn't work.

---

## AI Interaction History

This assessment was implemented entirely within a Claude Code session. The full conversation is available at:

**GitHub repository:** https://github.com/asyafl/claims-module

The session covered approximately 200+ exchanges spanning architecture design, all backend layers, frontend components, deployment debugging, and documentation. Key moments:
- Architecture plan was written to `.claude/plans/` and approved before implementation
- Each layer built and verified before proceeding to the next
- Runtime errors debugged with actual error messages provided to Claude
- Visual verification via Playwright screenshots for Angular UI
