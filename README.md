# Digital Stokvel Banking

[![Backend CI](https://github.com/your-org/digital-stokvel-banking/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/your-org/digital-stokvel-banking/actions/workflows/backend-ci.yml)
[![Mobile CI](https://github.com/your-org/digital-stokvel-banking/actions/workflows/mobile-ci.yml/badge.svg)](https://github.com/your-org/digital-stokvel-banking/actions/workflows/mobile-ci.yml)
[![Web CI](https://github.com/your-org/digital-stokvel-banking/actions/workflows/web-ci.yml/badge.svg)](https://github.com/your-org/digital-stokvel-banking/actions/workflows/web-ci.yml)
[![Infrastructure](https://github.com/your-org/digital-stokvel-banking/actions/workflows/infra-deploy.yml/badge.svg)](https://github.com/your-org/digital-stokvel-banking/actions/workflows/infra-deploy.yml)
[![codecov](https://codecov.io/gh/your-org/digital-stokvel-banking/branch/main/graph/badge.svg)](https://codecov.io/gh/your-org/digital-stokvel-banking)

**A bank-native feature set that brings South Africa's deeply rooted cultural savings practice (stokvels) into the formal financial ecosystem.**

## Overview

Digital Stokvel Banking enables 11 million stokvel participants to earn interest on their R50+ billion in annual savings while building credit profiles and accessing formal financial services. This platform provides:

- **Interest-bearing group savings accounts** - Earn 3.5-5.5% p.a. on pooled deposits
- **USSD support for feature phones** - Financial inclusion via *120*STOKVEL#
- **Transparent digital ledger** - Immutable contribution tracking visible to all members
- **Dual-approval payout system** - Chairperson + Treasurer verification
- **Multi-language support** - English, isiZulu, Sesotho, Xhosa, Afrikaans
- **Credit profile building** - Consistent contributions reported to credit bureaus (Phase 2)

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Backend API | ASP.NET Core Web API | .NET 10 |
| Database | PostgreSQL | 16.x |
| Mobile App | React Native | 0.73+ |
| Web Dashboard | React + Vite | 18+ |
| Infrastructure | Azure Bicep | Latest |
| Hosting | Azure Container Apps | Latest |

## Project Structure

```
digital-stokvel-banking/
├── src/
│   ├── backend/          # .NET 10 solution with API, Core, Infrastructure, Services
│   ├── mobile/           # React Native cross-platform mobile app
│   └── web/              # React Vite web dashboard for Chairpersons
├── infra/                # Azure Bicep infrastructure as code
├── docs/                 # Architecture diagrams, API specs, user guides
└── .github/workflows/    # CI/CD pipelines
```

## Getting Started

### Prerequisites

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Azure CLI** - [Install Guide](https://docs.microsoft.com/cli/azure/install-azure-cli)
- **Git** - [Download](https://git-scm.com/downloads)

### Local Development Setup

#### 1. Clone Repository

```bash
git clone https://github.com/your-org/digital-stokvel-banking.git
cd digital-stokvel-banking
```

#### 2. Start Local Database

```bash
docker-compose up -d
```

This starts PostgreSQL 16 on `localhost:5432` with credentials in `docker-compose.yml`.

#### 3. Backend Setup

```bash
cd src/backend
dotnet restore
dotnet build
dotnet ef database update --project DigitalStokvel.Infrastructure --startup-project DigitalStokvel.API
dotnet run --project DigitalStokvel.API
```

Backend API runs at `https://localhost:7001`

#### 4. Mobile App Setup

```bash
cd src/mobile
npm install
npx react-native run-android  # For Android
npx react-native run-ios       # For iOS (macOS only)
```

#### 5. Web Dashboard Setup

```bash
cd src/web/chairperson-dashboard
npm install
npm run dev
```

Web dashboard runs at `http://localhost:5173`

## Environment Variables

### Backend (.NET)

Create `src/backend/DigitalStokvel.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=digitalstokvel;Username=postgres;Password=postgres"
  },
  "Azure": {
    "KeyVaultUri": "https://your-keyvault.vault.azure.net/",
    "ServiceBusConnection": "<from-keyvault>",
    "BlobStorageConnection": "<from-keyvault>"
  },
  "Jwt": {
    "SecretKey": "<development-secret-key>",
    "Issuer": "digital-stokvel-api",
    "Audience": "digital-stokvel-client",
    "ExpirationMinutes": 60
  }
}
```

### Mobile App

Create `src/mobile/.env`:

```
API_BASE_URL=https://localhost:7001/api
FIREBASE_API_KEY=<your-firebase-key>
FIREBASE_PROJECT_ID=<your-firebase-project>
```

### Web Dashboard

Create `src/web/chairperson-dashboard/.env`:

```
VITE_API_BASE_URL=https://localhost:7001/api
```

## Testing

### Backend Tests

```bash
cd src/backend
dotnet test
```

### Mobile Tests

```bash
cd src/mobile
npm test
```

### Web Tests

```bash
cd src/web/chairperson-dashboard
npm test
```

## Deployment

### Automated CI/CD (GitHub Actions)

The project uses GitHub Actions for continuous integration and deployment:

- **Backend**: Automatic deployment to dev on push to `main`, manual approval required for production
- **Mobile**: Android APK builds on every push, iOS builds require signing certificates
- **Web**: Automatic deployment to Azure Static Web Apps
- **Infrastructure**: Manual trigger via GitHub Actions workflow

#### Trigger Full Stack Deployment

```bash
# Using GitHub CLI
gh workflow run deploy.yml -f environment=prod

# Or via GitHub Actions UI:
# Actions → Deploy Application → Run workflow → Select environment
```

#### View Deployment Status

```bash
gh run list --workflow=deploy.yml --limit 5
gh run view <run-id>
```

#### Required Secrets

See [.github/SECRETS.md](.github/SECRETS.md) for complete secret configuration guide.

**Minimum required for Phase 0**:
- `AZURE_CREDENTIALS` — Azure service principal
- `ACR_LOGIN_SERVER`, `ACR_USERNAME`, `ACR_PASSWORD` — Container registry
- `AZURE_STATIC_WEB_APPS_API_TOKEN_DEV` — Web deployment token
- `SQL_ADMIN_OBJECT_ID` — PostgreSQL admin user

### Manual Infrastructure Provisioning

```bash
cd infra
az login
az deployment group create \
  --resource-group digital-stokvel-dev \
  --template-file main.bicep \
  --parameters @parameters/dev.parameters.json
```

### Manual Application Deployment

```bash
# Deploy backend to Container Apps
az containerapp update \
  --name digitalstokvel-api-dev \
  --resource-group digital-stokvel-dev \
  --image your-acr.azurecr.io/digitalstokvel-api:latest

# Deploy web to Static Web Apps (handled by GitHub Actions)
```

### Database Migrations

Apply migrations via GitHub Actions:

```bash
gh workflow run database-migration.yml \
  -f environment=dev \
  -f migration_action=apply
```

Or manually:

```bash
cd src/backend
dotnet ef database update \
  --project DigitalStokvel.Infrastructure \
  --startup-project DigitalStokvel.API \
  --context StokvelDbContext
```

### Environments

| Environment | Purpose | Approval Required | Auto-Deploy |
|-------------|---------|-------------------|-------------|
| **dev** | Development testing | No | Yes (on push to `main`) |
| **staging** | Pre-production validation | 1 reviewer | No (manual trigger) |
| **prod** | Production | 2 reviewers | No (manual trigger) |

## Architecture

### High-Level Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Mobile App     │────▶│  Azure Container │────▶│  PostgreSQL     │
│  (React Native) │     │  Apps            │     │  Flexible Server│
└─────────────────┘     └──────────────────┘     └─────────────────┘
                              │
                              │
┌─────────────────┐           │
│  USSD Gateway   │───────────┘
│  (MNO APIs)     │
└─────────────────┘

┌─────────────────┐
│  Web Dashboard  │
│  (React + Vite) │
└─────────────────┘
```

For detailed architecture diagrams, see [docs/architecture/](docs/architecture/).

## API Documentation

OpenAPI specifications available at:
- Local: `https://localhost:7001/swagger`
- Production: `https://api.digitalstokvel.co.za/swagger`

## Data Residency & Compliance

**Critical Compliance Requirements:**
- All data must remain in **South Africa** per SARB regulations
- Azure regions: **South Africa North (primary)**, **South Africa West (DR)**
- **POPIA** and **FICA** compliance mandatory
- **PCI DSS** not required (no card storage, bank-to-bank transfers only)
- **Encryption at rest** (Azure Storage SSE) and **in transit** (TLS 1.3)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development workflow and coding standards.

## License

Proprietary - All rights reserved.

## Support

- **Technical Support:** dev-team@bank.co.za
- **Product Questions:** product@bank.co.za
- **Documentation:** [docs/](docs/)

## MVP Goals

- **500 active stokvel groups** within 3 months
- **5,000 members onboarded**
- **R5M in pooled deposits**
- **R25K in interest revenue**
- **30% USSD adoption**
- **NPS >60**

---

**Version:** 1.0  
**Last Updated:** 2026-03-24  
**Status:** Phase 0 (Foundation) - In Progress
