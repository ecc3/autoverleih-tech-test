# Autoverleih API

A car rental management REST API built with .NET 10 minimal APIs and PostgreSQL.

## Getting Started

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose

### Run

```bash
docker compose up -d
```

The API will be available at `http://localhost:5000`.

API documentation (Scalar UI): `http://localhost:5000/scalar/v1`

### Seed the Database

To populate the database with sample data:

```bash
docker compose exec db psql -U postgres -d autoverleih -f /dev/stdin < scripts/seed.sql
```

### Stop

```bash
docker compose down       # keep data
docker compose down -v    # wipe database
```

## API Endpoints

### Customers `/api/customers`

| Method   | Path                  | Description        |
| -------- | --------------------- | ------------------ |
| `POST`   | `/api/customers`      | Create a customer  |
| `GET`    | `/api/customers`      | List all customers |
| `GET`    | `/api/customers/{id}` | Get customer by ID |
| `PUT`    | `/api/customers/{id}` | Update a customer  |
| `DELETE` | `/api/customers/{id}` | Delete a customer  |

### Cars `/api/cars`

| Method   | Path             | Description   |
| -------- | ---------------- | ------------- |
| `POST`   | `/api/cars`      | Add a car     |
| `GET`    | `/api/cars`      | List all cars |
| `GET`    | `/api/cars/{id}` | Get car by ID |
| `PUT`    | `/api/cars/{id}` | Update a car  |
| `DELETE` | `/api/cars/{id}` | Delete a car  |

### Rentals `/api/rentals`

| Method | Path                       | Description      |
| ------ | -------------------------- | ---------------- |
| `POST` | `/api/rentals`             | Create a rental  |
| `GET`  | `/api/rentals`             | List all rentals |
| `GET`  | `/api/rentals/{id}`        | Get rental by ID |
| `POST` | `/api/rentals/{id}/return` | Return a car     |
| `POST` | `/api/rentals/{id}/cancel` | Cancel a rental  |

## Project Structure

```
Api/
├── Models/       # Database entities (Customer, Car, Rental)
├── DTOs/         # Request/response records
├── Endpoints/    # Minimal API endpoint definitions
├── Services/     # Business logic
├── Data/         # EF Core DbContext and migrations
└── Program.cs    # Application entry point
```

## Tech Stack

- **.NET 10** (minimal APIs)
- **Entity Framework Core 10** (ORM)
- **PostgreSQL 17** (database)
- **Scalar** (API documentation)
- **Docker** (containerization)
