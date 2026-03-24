# CI/CD Quick Reference Card 🚀

## One-Line Deployment Commands

```bash
# Deploy everything to production
gh workflow run deploy.yml -f environment=prod

# Deploy infrastructure only
gh workflow run infra-deploy.yml -f environment=dev

# Apply database migrations
gh workflow run database-migration.yml -f environment=prod -f migration_action=apply

# Check workflow status
gh run list --limit 5
```

## Critical Secrets (Must Configure First)

```bash
# 1. Create service principal
az ad sp create-for-rbac --name "github-actions-digital-stokvel" \
  --role Contributor --scopes /subscriptions/{id} --sdk-auth
# → Copy output to AZURE_CREDENTIALS

# 2. Get ACR credentials
az acr show --name digitalstokvel --query loginServer -o tsv
# → Save as ACR_LOGIN_SERVER
az acr credential show --name digitalstokvel --query "passwords[0].value" -o tsv
# → Save as ACR_PASSWORD

# 3. Get Static Web Apps token
az staticwebapp secrets list --name digitalstokvel-web-dev --query "properties.apiKey" -o tsv
# → Save as AZURE_STATIC_WEB_APPS_API_TOKEN_DEV
```

## Workflow Triggers

| Workflow | Automatic | Manual |
|----------|-----------|--------|
| Backend CI | ✅ Push to `main` (`src/backend/**`) | ❌ |
| Mobile CI | ✅ Push to `main` (`src/mobile/**`) | ❌ |
| Web CI | ✅ Push to `main` (`src/web/**`) | ❌ |
| Infrastructure | ⚠️ Optional push (`infra/**`) | ✅ Yes |
| Full Deploy | ❌ | ✅ Yes |
| PR Validation | ✅ All PRs to `main` | ❌ |
| Database Migration | ❌ | ✅ Yes |

## Deployment Flow

```
Code Push → Tests (≥70% coverage) → Build → Deploy DEV → Smoke Tests → Approve → Deploy PROD
```

## Files Created

| File | Purpose |
|------|---------|
| `.github/workflows/backend-ci.yml` | Backend build, test, deploy |
| `.github/workflows/mobile-ci.yml` | Mobile build (Android/iOS) |
| `.github/workflows/web-ci.yml` | Web build, deploy |
| `.github/workflows/infra-deploy.yml` | Infrastructure, migrations |
| `.github/workflows/deploy.yml` | Full stack orchestration |
| `.github/workflows/pr-validation.yml` | PR quality gates |
| `.github/workflows/database-migration.yml` | DB migration management |
| `src/backend/DigitalStokvel.API/Dockerfile` | Backend container image |
| `.github/SECRETS.md` | Secret setup guide |
| `.github/README.md` | CI/CD documentation |
| `CI_CD_IMPLEMENTATION_SUMMARY.md` | This document |

## Environment Protection

| Environment | Reviewers | Auto-Deploy |
|-------------|-----------|-------------|
| dev | 0 | ✅ Yes |
| staging | 1 | ❌ No |
| prod | 2 | ❌ No |

## Phase 0 Status

✅ **Complete**: Backend, web, infrastructure, PR validation, database migrations  
⚠️ **Optional**: iOS builds (requires certificates), SonarCloud, App Center  
🔴 **Before First Deploy**: Configure 6 critical secrets + create environments

## Help

- Full docs: [.github/README.md](.github/README.md)
- Secrets guide: [.github/SECRETS.md](.github/SECRETS.md)
- Troubleshooting: Check workflow logs in GitHub Actions
