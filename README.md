# Forest Management System

A full-stack web application for managing forest land properties, cadastral units, forest stands, and timber activities. Built for forest management companies with company-level access control, activity tracking, Excel exports, and an interactive map interface.

## Tech Stack

**Backend**
- ASP.NET Core 10.0 / C# 12
- PostgreSQL 17 + EF Core 10 (Npgsql)
- ASP.NET Core Identity + JWT authentication (access + refresh tokens)
- ClosedXML (Excel export)
- Swagger / OpenAPI

**Frontend**
- SvelteKit 2 + TypeScript 5
- Tailwind CSS 4
- Leaflet (interactive maps)
- Vite 7

**Deployment**
- Docker + Docker Compose
- nginx (reverse proxy + SSL via LetsEncrypt)
- watchtower (automatic image updates)

## Architecture

Clean layered architecture:

```
WebApp (Controllers, Program.cs)
    ↓
App.BLL (Services: Auth, Companies, LandProperties, Cadasters, ForestStands, Activities, Dashboard, Export)
    ↓
App.DAL / App.DAL.EF (Repositories, UnitOfWork, EF Core DbContext)
    ↓
App.Domain / App.Contracts / Base.Domain (Entities, Enums, Identity)
```

## Domain Model

- **Companies** — forest management companies
- **Land Properties** — owned by companies, located in Estonian counties, with FSC certification flag
- **Cadastral Units** — parcels within a land property (soil quality, area types, volume metrics)
- **Forest Stands** — numbered sections within cadastral units (area, volume, active/inactive)
- **Activities** — timber work, planting, clearing, grant applications linked to forest stands or cadastral units
- **Users** — Admin role + regular users (activity ownership enforced)

## Quick Start

### Prerequisites

- .NET 10 SDK
- Node.js 22
- PostgreSQL 17
- npm

### Backend

```bash
cd ForestManagement
dotnet restore ForestManagement.sln
dotnet run --project ForestManagement/WebApp
```

API available at `http://localhost:5255`. Swagger UI at `/swagger`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend available at `http://localhost:5173`.

## Configuration

Create `ForestManagement/WebApp/appsettings.json` (or use user secrets / environment variables):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=forestmanagement;Username=postgres;Password=yourpassword"
  },
  "JWT": {
    "Key": "your-64-character-random-secret-key-here",
    "Issuer": "ForestManagement",
    "Audience": "ForestManagementUsers",
    "ExpiresInMinutes": 60,
    "RefreshTokenExpiresInDays": 7
  },
  "SeedAdmin": {
    "Email": "admin@example.com",
    "Password": "SecurePassword123!",
    "Username": "admin"
  }
}
```

## Deployment

Production deployment uses Docker Compose with four services behind an nginx reverse proxy.

### Docker Compose Services

| Service | Role | Ports |
|---|---|---|
| `postgres` | PostgreSQL 17 database | 5432 (internal) |
| `backend` | ASP.NET Core API | 5255 (internal) |
| `frontend` | SvelteKit Node server | 3000 (internal) |
| `nginx` | Reverse proxy + SSL | 80, 443 (exposed) |
| `watchtower` | Auto-updates images every 5 min | — |

### Deploy Steps

1. Build and push images to a registry (e.g. GitHub Container Registry):

```bash
docker build -t ghcr.io/<user>/forest-management-backend:latest -f Dockerfile.backend .
docker build -t ghcr.io/<user>/forest-management-frontend:latest -f Dockerfile.frontend .
docker push ghcr.io/<user>/forest-management-backend:latest
docker push ghcr.io/<user>/forest-management-frontend:latest
```

2. Copy `docker-compose.yml`, `nginx.conf`, and `.env` to the server.

3. Start the stack:

```bash
docker compose up -d
```

### Environment Variables

Create a `.env` file:

```env
DB_USER=forestapp
DB_PASSWORD=<strong-password>
DB_NAME=forestmanagement
JWT_SECRET=<64-character-random-string>
GITHUB_USER=<your-github-username>
FRONTEND_URL=https://app.yourdomain.com
SEED_ADMIN_EMAIL=admin@example.com
SEED_ADMIN_PASSWORD=<admin-password>
SEED_ADMIN_USERNAME=admin
```

### SSL

nginx is configured for LetsEncrypt certificates. Place certificates at:
- `/etc/letsencrypt/live/<domain>/fullchain.pem`
- `/etc/letsencrypt/live/<domain>/privkey.pem`

## CI/CD

GitHub Actions workflow (`.github/workflows/deploy.yml`) builds Docker images on push to `main` and deploys to staging / production VPS via SSH.
