# Autoverleih

A car rental management system with a .NET 10 REST API and a React frontend.

## Getting Started

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose

### Run

```bash
docker compose up -d
```

- Frontend: `http://localhost:5173`
- API: `http://localhost:5000`
- API docs (Scalar UI): `http://localhost:5000/scalar/v1`

### Seed the Database

To populate the database with sample data:

```bash
docker compose exec -T db psql -U postgres -d autoverleih < scripts/seed.sql
```

### Stop

```bash
docker compose down       # keep data
docker compose down -v    # wipe database
```

### Local Development

The frontend container mounts `Frontend/src` as a volume, so changes to source files are reflected immediately via Vite HMR.

To run the frontend outside Docker:

```bash
cd Frontend
npm install
npm run dev
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
Frontend/
├── src/
│   ├── api/          # Axios client and service modules
│   ├── components/   # Shared components (Layout)
│   ├── pages/        # Page components (customers, cars, rentals)
│   └── types/        # TypeScript type definitions

Api/
├── Models/           # Database entities (Customer, Car, Rental)
├── DTOs/             # Request/response records
├── Endpoints/        # Minimal API endpoint definitions
├── Services/         # Business logic
├── Data/             # EF Core DbContext and migrations
└── Program.cs        # Application entry point

Api.Tests/
└── Services/         # xUnit service-layer tests
```

## Tech Stack

- **React 19** with TypeScript (frontend)
- **Vite** (build tool)
- **Material UI 7** (component library)
- **.NET 10** minimal APIs (backend)
- **Entity Framework Core 10** (ORM)
- **PostgreSQL 17** (database)
- **Scalar** (API documentation)
- **Docker** (containerization)
