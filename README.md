# Order Intake & Tracking

A small internal web app for recording customer purchase orders and tracking their statuses. Built with ASP.NET Core (.NET 8) and Angular 18.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Node.js 18+](https://nodejs.org/) (tested with Node 24)
- npm (included with Node.js)

## Quick Start

### 1. Run the API

```bash
cd OrderIntakeTracking.Api
dotnet run
```

The API starts at `http://localhost:5273`. Swagger UI is available at `http://localhost:5273/swagger` in Development.

### 2. Run the Angular client

In a second terminal:

```bash
cd client
npm install
npm start
```

The UI is available at `http://localhost:4200`.

> **Windows note:** Project paths with spaces or `&` can break npm's default CLI shims. This repo's `package.json` scripts call the Angular CLI via `node` directly so `npm start` works from paths like `Order Intake & Tracking`.

### 3. Run backend tests

From the solution root:

```bash
dotnet test
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/orders` | Submit a new order (idempotent by external reference) |
| `GET` | `/api/orders` | List orders, newest first |
| `GET` | `/api/orders/{id}` | Get order details |
| `PATCH` | `/api/orders/{id}/status` | Update order status |

## Solution Structure

```
OrderIntakeTracking.sln
├── OrderIntakeTracking.Api/           # ASP.NET Core minimal API
├── OrderIntakeTracking.Application/   # Services, DTOs, interfaces
├── OrderIntakeTracking.Domain/        # Entities and enums
├── OrderIntakeTracking.Infrastructure/# In-memory repository
├── OrderIntakeTracking.Tests/         # Unit tests
└── client/                            # Angular 18 frontend
```

## Key Behaviors

- **Duplicate prevention**: Re-submitting the same external reference returns the existing order instead of creating a duplicate.
- **Server-side totals**: Line totals, subtotal, and total are calculated on the server.
- **Status workflow**: Pending → Confirmed/Cancelled; Confirmed → Fulfilled/Cancelled. Terminal states (Fulfilled, Cancelled) cannot be changed.

See [SOLUTION.md](SOLUTION.md) for design decisions and trade-offs.
