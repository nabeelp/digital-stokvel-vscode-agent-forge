# CI/CD Workflows

This directory contains GitHub Actions workflows for Digital Stokvel Banking platform.

## 📋 Workflows Overview

### Core Workflows

| Workflow | Trigger | Purpose | Status |
|----------|---------|---------|--------|
| [`backend-ci.yml`](workflows/backend-ci.yml) | Push/PR to `main` (backend/**) | Build, test, analyze, deploy backend API | Phase 0 ✅ |
| [`mobile-ci.yml`](workflows/mobile-ci.yml) | Push/PR to `main` (mobile/**) | Build Android/iOS apps, run tests | Phase 0 ✅ |
| [`web-ci.yml`](workflows/web-ci.yml) | Push/PR to `main` (web/**) | Build and deploy web dashboard | Phase 0 ✅ |
| [`infra-deploy.yml`](workflows/infra-deploy.yml) | Manual / Push (infra/**) | Deploy Azure infrastructure (Bicep) | Phase 0 ✅ |
| [`deploy.yml`](workflows/deploy.yml) | Manual | Full stack deployment orchestration | Phase 0 ✅ |

### Quality Gates

| Workflow | Trigger | Purpose | Status |
|----------|---------|---------|--------|
| [`pr-validation.yml`](workflows/pr-validation.yml) | Pull Request to `main` | Validate code quality, tests, coverage | Phase 0 ✅ |

### Database Management

| Workflow | Trigger | Purpose | Status |
|----------|---------|---------|--------|
| [`database-migration.yml`](workflows/database-migration.yml) | Manual | Apply/rollback EF Core migrations | Phase 0 ✅ |

---

## 🚀 Deployment Process

### Development Deployment (Automatic)
1. Developer pushes code to `main` branch
2. Relevant CI workflow triggers (backend/mobile/web)
3. Tests run, code coverage validated (≥70%)
4. Docker images built and pushed to ACR (backend)
5. Deployment to **dev** environment (auto-approve)
6. Smoke tests run
7. Deployment to **prod** environment (requires manual approval)

### Production Deployment (Manual)
1. Trigger `deploy.yml` workflow manually
2. Select environment: `prod`
3. Workflow orchestrates:
   - Infrastructure deployment (`infra-deploy.yml`)
   - Backend API build and deploy
   - Web dashboard build and deploy
   - Smoke tests
4. Requires 2 reviewers to approve production deployment
5. Deployment summary posted to GitHub

### Database Migration (Manual)
1. Trigger `database-migration.yml` workflow
2. Select environment and action: `apply`, `rollback`, or `status`
3. Workflow:
   - Retrieves connection string from Key Vault
   - Runs EF Core migrations
   - Validates post-migration state

---

## 🔑 Required Secrets

See [SECRETS.md](SECRETS.md) for comprehensive secret configuration guide.

### Phase 0 Minimum Requirements
- `AZURE_CREDENTIALS` — Service principal for Azure authentication
- `ACR_LOGIN_SERVER`, `ACR_USERNAME`, `ACR_PASSWORD` — Container registry access
- `AZURE_STATIC_WEB_APPS_API_TOKEN_DEV` — Web deployment token
- `SQL_ADMIN_OBJECT_ID` — PostgreSQL admin user Object ID

---

## 🌍 Environments

Configure in **Settings → Environments**:

| Environment | Protection Rules | Approvers | Deployment Trigger |
|-------------|------------------|-----------|-------------------|
| `dev` | None | None | Auto-deploy on `main` push |
| `staging` | 1 reviewer | Team leads | Manual approval |
| `prod` | 2 reviewers | Senior engineers | Manual approval |

---

## 📊 CI/CD Metrics

### Build Times (Target)
- Backend build + test: ~3 minutes
- Mobile Android build: ~5 minutes
- Mobile iOS build: ~8 minutes (macOS runner)
- Web build: ~2 minutes
- Infrastructure deployment: ~10 minutes

### Test Coverage Requirements
- Backend: ≥70% line coverage (enforced in PR validation)
- Mobile: ≥70% line coverage (enforced in PR validation)
- Web: No strict requirement for Phase 0

---

## 🛠️ Manual Workflow Triggers

### Deploy Full Stack
```bash
gh workflow run deploy.yml \
  -f environment=prod \
  -f skip_tests=false
```

### Deploy Infrastructure Only
```bash
gh workflow run infra-deploy.yml \
  -f environment=dev \
  -f skip_validation=false
```

### Apply Database Migrations
```bash
gh workflow run database-migration.yml \
  -f environment=prod \
  -f migration_action=apply
```

### Check Migration Status
```bash
gh workflow run database-migration.yml \
  -f environment=prod \
  -f migration_action=status
```

### Rollback Migration
```bash
gh workflow run database-migration.yml \
  -f environment=prod \
  -f migration_action=rollback \
  -f rollback_target=20250101000000_InitialCreate
```

---

## 🔍 Pull Request Validation

All PRs to `main` trigger `pr-validation.yml`:

**Checks**:
- ✅ Backend: Build, test, coverage ≥70%, SonarCloud analysis
- ✅ Mobile: Lint, type check, Jest tests
- ✅ Web: Lint, type check, build
- ✅ Infrastructure: Bicep lint, what-if analysis
- ✅ Breaking changes detection

**PR merge requirements**:
- All validation jobs pass
- Code coverage meets threshold
- 1 reviewer approval
- No breaking changes (or documented with migration plan)

---

## 📦 Artifact Retention

| Artifact | Retention Period | Size Limit |
|----------|------------------|------------|
| Backend build | 7 days | ~50 MB |
| Android APK | 7 days | ~100 MB |
| iOS IPA | 7 days | ~150 MB (if built) |
| Web build | 7 days | ~10 MB |
| Test results | 30 days | ~5 MB |

---

## 🐛 Troubleshooting

### Workflow fails with "Secret not found"
1. Verify secret is configured: **Settings → Secrets and variables → Actions**
2. Check secret name matches exactly (case-sensitive)
3. For environment-specific secrets, check environment configuration

### Docker build fails: "ACR authentication failed"
1. Verify `ACR_USERNAME` and `ACR_PASSWORD` are correct
2. Enable ACR admin user:
   ```bash
   az acr update --name digitalstokvel --admin-enabled true
   ```

### Code coverage check fails
1. Ensure tests are generating coverage reports
2. Check coverage threshold in `pr-validation.yml` (default: 70%)
3. Add more unit tests to increase coverage

### iOS build skipped
- iOS builds require macOS runner and signing certificates
- Optional for Phase 0 MVP
- Configure `IOS_CERTIFICATE`, `IOS_PROVISIONING_PROFILE`, `IOS_CERTIFICATE_PASSWORD` secrets

### Migration fails: "Connection string not found"
1. Verify Key Vault name in workflow matches deployed infrastructure
2. Ensure service principal has Key Vault `get` permission:
   ```bash
   az keyvault set-policy --name kv-stokvel-dev \
     --spn {clientId} --secret-permissions get
   ```

---

## 📚 References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure Container Apps Deploy Action](https://github.com/Azure/container-apps-deploy-action)
- [Azure Static Web Apps Deploy Action](https://github.com/Azure/static-web-apps-deploy)
- [Digital Stokvel PRD](../docs/digital-stokvel-prd.md)
- [Infrastructure Documentation](../infra/README.md)

---

## 🔒 Security

- All secrets stored in GitHub Secrets (encrypted at rest)
- Service principals follow principle of least privilege
- Production deployments require manual approval
- Secrets rotation: Every 90 days (automated reminders)
- Audit logs: Available in GitHub Actions history

---

## 📈 Phase 1 Enhancements (Post-MVP)

- [ ] Add E2E tests (Playwright for web, Appium for mobile)
- [ ] Integrate load testing (Azure Load Testing)
- [ ] Add automated rollback on failed smoke tests
- [ ] Deploy preview environments for PRs
- [ ] Add Slack/Teams notifications for deployment status
- [ ] Implement blue-green deployment strategy
- [ ] Add performance benchmarking in CI
- [ ] Automate secret rotation

---

**Last Updated**: 2026-03-24  
**Maintained by**: DevOps Team  
**Contact**: devops@digitalstokvel.co.za
