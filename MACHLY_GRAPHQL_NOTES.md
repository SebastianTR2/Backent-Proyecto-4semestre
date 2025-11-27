# Machly GraphQL Integration Notes

This document explains the hybrid REST + GraphQL architecture implemented in `Machly.Api`.

## Overview

Machly now supports both REST (via Controllers) and GraphQL (via HotChocolate) in the same application.
- **REST Endpoint**: `/api/...` (Existing)
- **GraphQL Endpoint**: `/graphql` (New)

## Getting Started

1. Run the `Machly.Api` project.
2. Open your browser to `https://localhost:5001/graphql` (or your local port).
3. This will open **Banana Cake Pop**, the GraphQL IDE.

## Authentication

GraphQL uses the same JWT authentication as the REST API.
To execute queries that require authorization:
1. Obtain a JWT token via the REST login endpoint (`/api/auth/login`).
2. In Banana Cake Pop, go to the **Authorization** tab.
3. Select **Bearer** type and paste your token.

## Available Operations

### Queries (Read)

- `machines`: List all machines.
- `machineById(id: String!)`: Get a specific machine.
- `myBookings`: Get bookings for the authenticated user (Renter).
- `me`: Get current user profile.

**Example:**
```graphql
query {
  machines {
    id
    title
    category
    pricePerDay
    location {
      coordinates
    }
  }
}
```

### Mutations (Write)

- `createBooking`: Create a new booking (Role: RENTER).
- `createMachine`: Create a new machine (Role: PROVIDER).

**Example:**
```graphql
mutation {
  createBooking(input: {
    machineId: "6565...",
    start: "2025-01-01T10:00:00Z",
    end: "2025-01-05T10:00:00Z"
  }) {
    id
    status
    totalPrice
  }
}
```

## Architecture Details

- **Folder Structure**: `Machly.Api/GraphQL` contains Queries, Mutations, and Inputs.
- **Reusability**: Resolvers inject existing `Repositories` and `Services` (e.g., `MachineRepository`, `BookingService`).
- **Authorization**: Uses `[Authorize]` attributes from `HotChocolate.AspNetCore.Authorization`.
