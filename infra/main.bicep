// Digital Stokvel Banking - Azure Infrastructure
// Main Bicep template for deploying all Azure resources
targetScope = 'resourceGroup'

// ============================================================================
// PARAMETERS
// ============================================================================

@description('Environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

@description('Azure region for resources (South Africa North per SARB data residency)')
@allowed([
  'southafricanorth'
  'southafricawest'
])
param location string = 'southafricanorth'

@description('Project name used for resource naming')
param projectName string = 'digitalstokvel'

@description('Azure AD (Entra ID) Object ID for PostgreSQL Admin')
param postgresAdminObjectId string

@description('Azure AD (Entra ID) Admin display name')
param postgresAdminLogin string

@description('Cost center for billing tracking')
param costCenter string = 'DigitalBanking'

@description('Resource owner email')
param ownerEmail string

@description('Tags to apply to all resources')
param tags object = {
  Environment: environment
  Project: 'DigitalStokvel'
  CostCenter: costCenter
  Owner: ownerEmail
  ManagedBy: 'Bicep'
}

// ============================================================================
// VARIABLES
// ============================================================================

var resourceToken = toLower('${projectName}-${environment}')

// ============================================================================
// MODULE DEPLOYMENTS
// ============================================================================

// 1. Monitoring & Logging (deployed first for diagnostic dependencies)
module monitoring 'modules/monitoring.bicep' = {
  name: '${resourceToken}-monitoring-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    tags: tags
  }
}

// 2. Key Vault (deployed early for secrets management)
module keyVault 'modules/keyvault.bicep' = {
  name: '${resourceToken}-keyvault-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// 3. Storage Account (for ledger exports and audit logs)
module storage 'modules/storage.bicep' = {
  name: '${resourceToken}-storage-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// 4. Service Bus (for async message processing)
module serviceBus 'modules/servicebus.bicep' = {
  name: '${resourceToken}-servicebus-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// 5. PostgreSQL Database (core data persistence)
module postgres 'modules/postgres.bicep' = {
  name: '${resourceToken}-postgres-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    adminObjectId: postgresAdminObjectId
    adminLogin: postgresAdminLogin
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
  }
}

// 6. Container Apps (backend API hosting)
module containerApps 'modules/container-apps.bicep' = {
  name: '${resourceToken}-containerapps-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
    applicationInsightsConnectionString: monitoring.outputs.applicationInsightsConnectionString
    keyVaultName: keyVault.outputs.keyVaultName
    postgresServerFqdn: postgres.outputs.postgresServerFqdn
    postgresDatabaseName: postgres.outputs.databaseName
  }
}

// 7. API Management (API gateway for USSD and rate limiting)
module apim 'modules/apim.bicep' = {
  name: '${resourceToken}-apim-deployment'
  params: {
    environment: environment
    location: location
    projectName: projectName
    tags: tags
    logAnalyticsWorkspaceId: monitoring.outputs.logAnalyticsWorkspaceId
    applicationInsightsId: monitoring.outputs.applicationInsightsId
    applicationInsightsInstrumentationKey: monitoring.outputs.applicationInsightsInstrumentationKey
    backendApiUrl: containerApps.outputs.backendApiUrl
  }
}

// ============================================================================
// RBAC ROLE ASSIGNMENTS
// ============================================================================

// Assign Container App managed identity access to Key Vault
module containerAppKeyVaultRbac 'modules/rbac-keyvault.bicep' = {
  name: '${resourceToken}-rbac-keyvault'
  params: {
    keyVaultName: keyVault.outputs.keyVaultName
    principalId: containerApps.outputs.systemAssignedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Assign Container App managed identity access to Storage
module containerAppStorageRbac 'modules/rbac-storage.bicep' = {
  name: '${resourceToken}-rbac-storage'
  params: {
    storageAccountName: storage.outputs.storageAccountName
    principalId: containerApps.outputs.systemAssignedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Assign Container App managed identity access to Service Bus
module containerAppServiceBusRbac 'modules/rbac-servicebus.bicep' = {
  name: '${resourceToken}-rbac-servicebus'
  params: {
    serviceBusNamespaceName: serviceBus.outputs.serviceBusNamespaceName
    principalId: containerApps.outputs.systemAssignedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ============================================================================
// OUTPUTS
// ============================================================================

@description('Container Apps backend API URL')
output backendApiUrl string = containerApps.outputs.backendApiUrl

@description('API Management gateway URL')
output apimGatewayUrl string = apim.outputs.apimGatewayUrl

@description('PostgreSQL server FQDN')
output postgresServerFqdn string = postgres.outputs.postgresServerFqdn

@description('PostgreSQL database name')
output postgresDatabaseName string = postgres.outputs.databaseName

@description('Key Vault name')
output keyVaultName string = keyVault.outputs.keyVaultName

@description('Key Vault URI')
output keyVaultUri string = keyVault.outputs.keyVaultUri

@description('Application Insights connection string')
output applicationInsightsConnectionString string = monitoring.outputs.applicationInsightsConnectionString

@description('Application Insights instrumentation key')
output applicationInsightsInstrumentationKey string = monitoring.outputs.applicationInsightsInstrumentationKey

@description('Log Analytics workspace ID')
output logAnalyticsWorkspaceId string = monitoring.outputs.logAnalyticsWorkspaceId

@description('Storage account name')
output storageAccountName string = storage.outputs.storageAccountName

@description('Service Bus namespace name')
output serviceBusNamespaceName string = serviceBus.outputs.serviceBusNamespaceName

@description('Container App system-assigned managed identity principal ID')
output containerAppIdentityPrincipalId string = containerApps.outputs.systemAssignedIdentityPrincipalId
//   name: 'apimDeployment'
//   params: {
//     environment: environment
//     location: location
//     appName: appName
//   }
// }

// module monitoring 'modules/monitoring.bicep' = {
//   name: 'monitoringDeployment'
//   params: {
//     environment: environment
//     location: location
//     appName: appName
//   }
// }

// module storage 'modules/storage.bicep' = {
//   name: 'storageDeployment'
//   params: {
//     environment: environment
//     location: location
//     appName: appName
//   }
// }

// module serviceBus 'modules/servicebus.bicep' = {
//   name: 'serviceBusDeployment'
//   params: {
//     environment: environment
//     location: location
//     appName: appName
//   }
// }

output resourceGroupName string = resourceGroup().name
output location string = location
output environment string = environment
