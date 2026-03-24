// Azure Key Vault module
// Provisions Key Vault for secrets management (SP-05 requirement)

@description('Environment name')
param environment string

@description('Location for resources')
param location string

@description('Project name')
param projectName string

@description('Resource tags')
param tags object

@description('Log Analytics Workspace ID for diagnostics')
param logAnalyticsWorkspaceId string

// ============================================================================
// VARIABLES
// ============================================================================

var resourceToken = toLower('${projectName}-${environment}')
var keyVaultName = take('${replace(resourceToken, '-', '')}kv', 24) // Max 24 chars, alphanumeric only

// ============================================================================
// KEY VAULT
// ============================================================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard' // Standard tier (not Premium unless HSM required)
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true // Use RBAC instead of access policies (SP-05)
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: environment == 'prod' ? true : null // Production only
    publicNetworkAccess: 'Enabled' // For dev; use private endpoints for prod
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow' // Dev: allow; Prod: should use 'Deny' with private endpoint
    }
  }
}

// ============================================================================
// DIAGNOSTIC SETTINGS
// ============================================================================

resource keyVaultDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${keyVaultName}-diagnostics'
  scope: keyVault
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'AuditEvent'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 90
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 90
        }
      }
    ]
  }
}

// ============================================================================
// PLACEHOLDER SECRETS (to be populated by backend deployment)
// ============================================================================

// Note: These secrets will be populated by the backend deployment pipeline
// or manual configuration post-deployment. Initial values are placeholders.

resource postgresConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'PostgreSQLConnectionString'
  parent: keyVault
  properties: {
    value: 'Placeholder - will be set by deployment pipeline'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
}

resource communicationServicesSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'AzureCommunicationServicesConnectionString'
  parent: keyVault
  properties: {
    value: 'Placeholder - will be set manually'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
}

resource ussdGatewayApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'USSDGatewayAPIKey'
  parent: keyVault
  properties: {
    value: 'Placeholder - will be set manually'
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
}

resource coreBankingApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'CoreBankingAPIKey'
  parent: keyVault
  properties: {
    value: 'MockedValue-MVP' // Mocked for MVP
    contentType: 'text/plain'
    attributes: {
      enabled: true
    }
  }
}

// ============================================================================
// OUTPUTS
// ============================================================================

output keyVaultName string = keyVault.name
output keyVaultId string = keyVault.id
output keyVaultUri string = keyVault.properties.vaultUri
