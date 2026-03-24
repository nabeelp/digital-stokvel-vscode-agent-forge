// Azure Blob Storage module
// Provisions storage for ledger exports and compliance documents

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

var resourceToken = toLower('${projectName}${environment}')
// Ensure minimum 3 characters for storage account name
var storageAccountName = take('${replace(resourceToken, '-', '')}store', 24) // Max 24 chars, alphanumeric only

// SKU based on environment
var storageSku = environment == 'prod' ? 'Standard_GRS' : 'Standard_LRS' // Geo-redundant for prod

// ============================================================================
// STORAGE ACCOUNT
// ============================================================================

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: storageSku
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot' // Frequently accessed ledger exports
    supportsHttpsTrafficOnly: true // TLS 1.2+ enforcement (SP-04)
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false // No anonymous access
    encryption: {
      services: {
        blob: {
          enabled: true
          keyType: 'Account'
        }
        file: {
          enabled: true
          keyType: 'Account'
        }
      }
      keySource: 'Microsoft.Storage' // AES-256 encryption at rest (SP-04)
    }
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow' // Dev: allow; Prod: use 'Deny' with private endpoint
    }
  }
}

// ============================================================================
// BLOB CONTAINERS
// ============================================================================

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
}

// Container: Group ledger exports (PDFs)
resource ledgerExportsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'ledger-exports'
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

// Container: Compliance documents (FICA uploads)
resource complianceDocsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'compliance-documents'
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

// Container: Audit logs (7-year retention per NF-07)
resource auditLogsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'audit-logs'
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

// ============================================================================
// LIFECYCLE MANAGEMENT POLICY
// ============================================================================

resource lifecyclePolicy 'Microsoft.Storage/storageAccounts/managementPolicies@2023-01-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    policy: {
      rules: [
        {
          name: 'MoveLedgerExportsToCool'
          enabled: true
          type: 'Lifecycle'
          definition: {
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterModificationGreaterThan: 90
                }
              }
            }
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'ledger-exports/'
              ]
            }
          }
        }
        {
          name: 'MoveAuditLogsToArchive'
          enabled: true
          type: 'Lifecycle'
          definition: {
            actions: {
              baseBlob: {
                tierToArchive: {
                  daysAfterModificationGreaterThan: 365
                }
              }
            }
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'audit-logs/'
              ]
            }
          }
        }
      ]
    }
  }
}

// ============================================================================
// DIAGNOSTIC SETTINGS
// ============================================================================

resource storageDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${storageAccountName}-diagnostics'
  scope: blobService
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'StorageRead'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 30
        }
      }
      {
        category: 'StorageWrite'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 90
        }
      }
      {
        category: 'StorageDelete'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 90
        }
      }
    ]
    metrics: [
      {
        category: 'Transaction'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 30
        }
      }
    ]
  }
}

// ============================================================================
// OUTPUTS
// ============================================================================

output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
