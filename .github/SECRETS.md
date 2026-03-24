# GitHub Secrets Configuration

This document lists all required GitHub secrets for Digital Stokvel Banking CI/CD pipelines.

## 📋 Table of Contents
- [Azure Credentials](#azure-credentials)
- [Container Registry](#container-registry)
- [Mobile Build Secrets](#mobile-build-secrets)
- [Web Deployment](#web-deployment)
- [Database Configuration](#database-configuration)
- [Optional Secrets](#optional-secrets)

---

## Azure Credentials

### `AZURE_CREDENTIALS`
**Required for**: Infrastructure deployment, Container Apps deployment, Key Vault access  
**Type**: Service Principal JSON  
**Description**: Azure service principal credentials with Contributor role on subscription

**How to create**:
```bash
az ad sp create-for-rbac \
  --name "github-actions-digital-stokvel" \
  --role Contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth
```

**Format**:
```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

**Used in workflows**:
- `backend-ci.yml` (deploy jobs)
- `infra-deploy.yml` (all jobs)
- `deploy.yml` (all jobs)
- `database-migration.yml` (all jobs)

---

## Container Registry

### `ACR_LOGIN_SERVER`
**Required for**: Docker image push/pull  
**Type**: String  
**Description**: Azure Container Registry login server URL

**Format**: `digitalstokvel.azurecr.io`

**How to get**:
```bash
az acr show --name digitalstokvel --query loginServer -o tsv
```

### `ACR_USERNAME`
**Required for**: Docker authentication  
**Type**: String  
**Description**: ACR username (usually the ACR name)

**Format**: `digitalstokvel`

### `ACR_PASSWORD`
**Required for**: Docker authentication  
**Type**: String (Secret)  
**Description**: ACR admin password

**How to get**:
```bash
az acr credential show --name digitalstokvel --query "passwords[0].value" -o tsv
```

**Security note**: Prefer managed identities in production. Admin credentials are used for Phase 0 simplicity.

**Used in workflows**:
- `backend-ci.yml` (docker-build-push job)
- `deploy.yml` (build-backend job)

---

## Mobile Build Secrets

### `ANDROID_KEYSTORE_BASE64`
**Required for**: Android release APK signing  
**Type**: String (Base64-encoded)  
**Description**: Android signing keystore file (release.keystore) encoded in base64

**How to create**:
```bash
# Generate keystore
keytool -genkey -v -keystore release.keystore -alias digitalstokvel \
  -keyalg RSA -keysize 2048 -validity 10000

# Encode to base64
base64 release.keystore | tr -d '\n' > release.keystore.base64

# Copy content to GitHub secret
cat release.keystore.base64
```

### `KEYSTORE_PASSWORD`
**Required for**: Android release APK signing  
**Type**: String (Secret)  
**Description**: Password for the keystore file

### `KEY_ALIAS`
**Required for**: Android release APK signing  
**Type**: String  
**Description**: Key alias in keystore

**Format**: `digitalstokvel`

### `KEY_PASSWORD`
**Required for**: Android release APK signing  
**Type**: String (Secret)  
**Description**: Password for the key alias

**Used in workflows**:
- `mobile-ci.yml` (build-android job)

**Phase 0 Note**: These secrets are optional for Phase 0. If not provided, debug builds will be used.

---

## iOS Build Secrets (Optional for Phase 0)

### `IOS_CERTIFICATE`
**Required for**: iOS IPA signing  
**Type**: String (Base64-encoded)  
**Description**: iOS distribution certificate (.p12 file) encoded in base64

**How to create**:
```bash
# Export certificate from Keychain as .p12
# Encode to base64
base64 -i certificate.p12 | tr -d '\n' > certificate.base64
```

### `IOS_CERTIFICATE_PASSWORD`
**Required for**: iOS IPA signing  
**Type**: String (Secret)  
**Description**: Password for the .p12 certificate

### `IOS_PROVISIONING_PROFILE`
**Required for**: iOS IPA signing  
**Type**: String (Base64-encoded)  
**Description**: iOS provisioning profile (.mobileprovision) encoded in base64

**How to create**:
```bash
base64 -i profile.mobileprovision | tr -d '\n' > profile.base64
```

**Used in workflows**:
- `mobile-ci.yml` (build-ios job)

**Phase 0 Note**: iOS builds are skipped if these secrets are not configured. Required for production releases.

---

## Web Deployment

### `AZURE_STATIC_WEB_APPS_API_TOKEN_DEV`
**Required for**: Web dashboard deployment to dev environment  
**Type**: String (Secret)  
**Description**: Azure Static Web Apps deployment token for dev environment

**How to get**:
```bash
az staticwebapp secrets list \
  --name digitalstokvel-web-dev \
  --query "properties.apiKey" -o tsv
```

### `AZURE_STATIC_WEB_APPS_API_TOKEN_PROD`
**Required for**: Web dashboard deployment to production  
**Type**: String (Secret)  
**Description**: Azure Static Web Apps deployment token for production

**Used in workflows**:
- `web-ci.yml` (deploy jobs)
- `deploy.yml` (deploy-web job)

---

## Database Configuration

### `SQL_ADMIN_OBJECT_ID`
**Required for**: PostgreSQL Entra ID authentication setup  
**Type**: String (GUID)  
**Description**: Microsoft Entra ID Object ID for PostgreSQL admin user

**How to get**:
```bash
az ad signed-in-user show --query id -o tsv
```

**Format**: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

**Used in workflows**:
- `infra-deploy.yml` (deploy job)

---

## Optional Secrets

### `SONAR_TOKEN`
**Required for**: SonarCloud code analysis  
**Type**: String (Secret)  
**Description**: SonarCloud authentication token

**How to get**: Generate from SonarCloud dashboard (https://sonarcloud.io/account/security)

### `SONAR_ORGANIZATION`
**Required for**: SonarCloud code analysis  
**Type**: String  
**Description**: SonarCloud organization key

**Format**: `your-org-name`

**Used in workflows**:
- `backend-ci.yml` (SonarCloud scan step)

### `APPCENTER_API_TOKEN`
**Required for**: App Center mobile distribution  
**Type**: String (Secret)  
**Description**: App Center API token for distributing mobile builds

**How to get**: Generate from App Center dashboard (https://appcenter.ms/settings/apitokens)

**Used in workflows**:
- `mobile-ci.yml` (distribute job)

### `API_BASE_URL_DEV`
**Type**: String  
**Description**: Backend API base URL for dev environment

**Format**: `https://digitalstokvel-api-dev.azurecontainerapps.io`

### `API_BASE_URL_PROD`
**Type**: String  
**Description**: Backend API base URL for production

**Format**: `https://api.digitalstokvel.co.za`

**Used in workflows**:
- `web-ci.yml` (build step)

---

## Setup Checklist

Use this checklist to verify all required secrets are configured:

### Phase 0 - MVP (Required)
- [ ] `AZURE_CREDENTIALS`
- [ ] `ACR_LOGIN_SERVER`
- [ ] `ACR_USERNAME`
- [ ] `ACR_PASSWORD`
- [ ] `AZURE_STATIC_WEB_APPS_API_TOKEN_DEV`
- [ ] `SQL_ADMIN_OBJECT_ID`

### Phase 0 - Optional (Recommended)
- [ ] `AZURE_STATIC_WEB_APPS_API_TOKEN_PROD`
- [ ] `ANDROID_KEYSTORE_BASE64`
- [ ] `KEYSTORE_PASSWORD`
- [ ] `KEY_ALIAS`
- [ ] `KEY_PASSWORD`
- [ ] `SONAR_TOKEN`
- [ ] `SONAR_ORGANIZATION`

### Phase 1 - Production (Required before go-live)
- [ ] All Phase 0 secrets
- [ ] `IOS_CERTIFICATE`
- [ ] `IOS_CERTIFICATE_PASSWORD`
- [ ] `IOS_PROVISIONING_PROFILE`
- [ ] `APPCENTER_API_TOKEN`
- [ ] `API_BASE_URL_PROD`

---

## Repository Settings

### Environment Configuration

Create environments in GitHub repository settings:

1. **dev**
   - No protection rules
   - Auto-deploy on main branch
   - Secrets: Development-specific tokens

2. **staging** (Optional for Phase 0)
   - Require 1 reviewer
   - Deploy after approval
   - Secrets: Staging-specific tokens

3. **prod**
   - Require 2 reviewers
   - Restrict to main branch only
   - Deployment protection rules enabled
   - Secrets: Production-specific tokens

### Branch Protection Rules

Configure branch protection for `main`:
- Require pull request reviews (1 reviewer)
- Require status checks to pass:
  - `Backend Validation`
  - `Mobile Validation`
  - `Web Validation`
  - `Infrastructure Validation`
- Require linear history
- Do not allow force pushes
- Require deployments to succeed before merging (optional)

---

## Security Best Practices

1. **Rotate secrets regularly**: Change ACR passwords, service principal secrets every 90 days
2. **Use managed identities**: Migrate from service principals to managed identities post-Phase 0
3. **Limit secret access**: Use environment-specific secrets, not global repository secrets
4. **Audit secret usage**: Review GitHub Actions logs for unauthorized access attempts
5. **Enable secret scanning**: GitHub Advanced Security secret scanning (if available)
6. **Use Key Vault references**: Store sensitive values in Azure Key Vault, reference in workflows

---

## Troubleshooting

### Common Issues

**Issue**: `AZURE_CREDENTIALS` authentication fails  
**Solution**: Verify service principal has Contributor role:
```bash
az role assignment list --assignee {clientId} --output table
```

**Issue**: ACR authentication fails  
**Solution**: Enable admin user on ACR:
```bash
az acr update --name digitalstokvel --admin-enabled true
```

**Issue**: Android keystore decoding fails  
**Solution**: Ensure base64 encoding has no line breaks:
```bash
base64 release.keystore | tr -d '\n'
```

**Issue**: Key Vault access denied  
**Solution**: Grant service principal Key Vault access:
```bash
az keyvault set-policy --name kv-stokvel-prod \
  --spn {clientId} \
  --secret-permissions get list
```

---

## Support

For issues with secret configuration:
1. Check workflow logs for detailed error messages
2. Verify secret names match exactly (case-sensitive)
3. Ensure secrets are configured at correct scope (repository vs. environment)
4. Review Azure RBAC permissions for service principal
5. Contact DevOps team: devops@digitalstokvel.co.za
