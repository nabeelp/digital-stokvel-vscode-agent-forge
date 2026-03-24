// Azure Container Apps module
// Provisions Container Apps environment and backend API (NF-02: auto-scaling)

@description('Environment name')
param environment string

@description('Location for resources')
param location string

@description('Project name')
param projectName string

@description('Resource tags')
param tags object

@description('Log Analytics Workspace ID')
param logAnalyticsWorkspaceId string

@description('Application Insights connection string')
param applicationInsightsConnectionString string

@description('Key Vault name for secrets')
param keyVaultName string

@description('PostgreSQL server FQDN')
param postgresServerFqdn string

@description('PostgreSQL database name')
param postgresDatabaseName string

// ============================================================================
// VARIABLES
// ============================================================================

var resourceToken = toLower('${projectName}-${environment}')
var containerAppsEnvironmentName = '${resourceToken}-cae'
var containerAppName = '${resourceToken}-api'

// Scaling configuration based on environment (NF-02)
var minReplicas = environment == 'prod' ? 3 : 1
var maxReplicas = environment == 'prod' ? 10 : 5

// Resource limits based on environment
var cpuCores = environment == 'prod' ? '2.0' : '0.5'
var memorySize = environment == 'prod' ? '4Gi' : '1Gi'

// Container image (placeholder - will be updated by CI/CD)
var containerImage = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

// ============================================================================
// LOG ANALYTICS WORKSPACE (existing)
// ============================================================================

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: split(logAnalyticsWorkspaceId, '/')[8]
}

// ============================================================================
// KEY VAULT (existing)
// ============================================================================

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// ============================================================================
// CONTAINER APPS ENVIRONMENT
// ============================================================================

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppsEnvironmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    zoneRedundant: environment == 'prod' ? true : false
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

// ============================================================================
// CONTAINER APP (BACKEND API)
// ============================================================================

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned' // Managed identity for passwordless auth (SP-05)
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false // HTTPS only (SP-04)
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      secrets: [
        {
          name: 'appinsights-connection-string'
          value: applicationInsightsConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: containerImage
          resources: {
            cpu: json(cpuCores)
            memory: memorySize
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environment == 'prod' ? 'Production' : 'Development'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'appinsights-connection-string'
            }
            {
              name: 'KeyVault__VaultUri'
              value: keyVault.properties.vaultUri
            }
            {
              name: 'PostgreSQL__Host'
              value: postgresServerFqdn
            }
            {
              name: 'PostgreSQL__Database'
              value: postgresDatabaseName
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: 'system-assigned' // Use system-assigned managed identity
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 30
              periodSeconds: 30
              failureThreshold: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 10
              periodSeconds: 10
              failureThreshold: 3
            }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
          {
            name: 'cpu-scaling'
            custom: {
              type: 'cpu'
              metadata: {
                type: 'Utilization'
                value: '70'
              }
            }
          }
        ]
      }
    }
  }
}

// ============================================================================
// OUTPUTS
// ============================================================================

output containerAppName string = containerApp.name
output containerAppId string = containerApp.id
output backendApiUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output systemAssignedIdentityPrincipalId string = containerApp.identity.principalId
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
