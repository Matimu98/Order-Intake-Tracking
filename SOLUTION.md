# Solution Design Notes

## Overview

This solution implements a minimal order intake and tracking system with a layered .NET backend and an Angular SPA. The focus is on reliable order capture, duplicate prevention, server-side calculations, and a clear status workflow.

## Architecture

### Backend layers

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Core entities (`Order`, `Customer`, `LineItem`) and `OrderStatus` enum |
| **Application** | Business logic, DTOs, validation, service interfaces |
| **Infrastructure** | In-memory repository implementation |
| **Api** | HTTP endpoints, CORS, dependency injection wiring |

This follows clean architecture principles without over-engineering. Business rules live in `OrderService` and dedicated helpers (`OrderTotalsCalculator`, `OrderStatusTransitionValidator`), keeping controllers/endpoints thin.

### Frontend

Angular 18 standalone components with a simple feature layout:

- **Order list** — newest-first display with optional status filter
- **Order create** — reactive form with dynamic line items
- **Order detail** — totals verification and status updates

## Key Design Decisions

### 1. Idempotent order submission

**Decision**: Use the client-provided `externalReference` as a natural idempotency key.

**Behavior**:
- On first submission → create order, return `201 Created`
- On duplicate submission → return existing order with `200 OK` and `wasDuplicate: true`

**Rationale**: Sales reps resubmit when unsure if an order went through. Returning the existing order gives a consistent, reassuring response without creating duplicates. Case-insensitive matching handles minor input variations.

**Trade-off**: Duplicate submissions with *different* payloads for the same reference are ignored. A production system might compare payloads and return `409 Conflict` if they differ. For this scope, returning the original order matches the stated user experience goal.

### 2. In-memory storage

**Decision**: `ConcurrentDictionary`-backed repository.

**Rationale**: Zero setup, runs locally with one command, sufficient for a take-home demo.

**Trade-off**: Data is lost on restart. A natural next step would be EF Core with SQLite or SQL Server, using a unique index on `ExternalReference`.

### 3. Server-side total calculation

**Decision**: All monetary calculations happen in `OrderTotalsCalculator` on the server.

**Rationale**: Prevents client tampering and ensures a single source of truth. The UI displays server-returned values only.

### 4. Status transition rules

```
Pending   → Confirmed, Cancelled
Confirmed → Fulfilled, Cancelled
Fulfilled → (terminal)
Cancelled → (terminal)
```

Invalid transitions return `400 Bad Request` with a descriptive message (e.g., allowed transitions from the current state).

**Rationale**: Enforces a simple, realistic sales workflow without arbitrary status jumps.

### 5. Minimal API endpoints

Used ASP.NET Core minimal APIs instead of controllers to reduce boilerplate for a small surface area.

### 6. CORS

Configured to allow `http://localhost:4200` for local Angular development. In production, this would be tightened to specific deployed origins.

## Validation

| Rule | Enforcement |
|------|-------------|
| External reference required | Application service |
| Positive integer quantities | Application service |
| Non-negative unit prices | Application service |
| At least one line item | Application service |
| Valid status transitions | `OrderStatusTransitionValidator` |

## Testing Strategy

Unit tests cover the highest-risk business rules:

- Server-side total calculation
- Duplicate reference handling (including case insensitivity)
- Quantity/price validation
- Status transition enforcement
- Orders listed newest first

Integration/API tests were omitted to stay within the timebox; the service layer tests cover the critical paths.

## Possible Extensions

- Persistent storage (SQLite/PostgreSQL) with unique constraint on `ExternalReference`
- Authentication/authorization for sales reps
- Audit log for status changes
- Payload comparison on duplicate submission
- Pagination for large order lists
- OpenAPI-generated TypeScript client

## Trade-offs Summary

| Choice | Benefit | Cost |
|--------|---------|------|
| In-memory store | Fast setup | No durability |
| Idempotent return on duplicate | Great UX for resubmission | Ignores payload mismatches |
| Layered architecture | Testable, maintainable | More projects than a single-file API |
| No auth | Simpler demo | Not production-ready |
