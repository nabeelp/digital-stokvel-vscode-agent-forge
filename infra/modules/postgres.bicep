// PostgreSQL Flexible Server module
// Provisions PostgreSQL with Entra ID authentication (SP-05)

@description('Environment name')
param environment string

@description('Location for resources')
param location string

@description('Project name')
param projectName string

@description('Azure AD Object ID for PostgreSQL Admin')
param adminObjectId string

@description('Azure AD Admin display name')
param adminLogin string

@description('Resource tags')
param tags object

@description('Log Analytics Workspace ID for diagnostics')
param logAnalyticsWorkspaceId string

// ============================================================================
// VARIABLES
// ============================================================================

var resourceToken = toLower('${projectName}-${environment}')
var postgresServerName = '${resourceToken}-postgres'
var databaseName = 'digitalstokvel'

// SKU based on environment
var postgresSku = environment == 'prod' ? {
  name: 'Standard_D4s_v3'
  tier: 'GeneralPurpose'
} : {
  name: 'Standard_D2s_v3'
  tier: 'GeneralPurpose'
}

// Storage size based on environment
var storageSize = environment == 'prod' ? 131072 : 32768 // 128GB prod, 32GB dev

// Backup retention (NF-08: 35 days)
var backupRetentionDays = 35

// ============================================================================
// POSTGRESQL FLEXIBLE SERVER
// ============================================================================

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: postgresServerName
  location: location
  tags: tags
  sku: postgresSku
  properties: {
    version: '16' // PostgreSQL 16.x
    administratorLogin: 'pgadmin' // Not used with Entra ID, but required parameter
    administratorLoginPassword: 'TempP@ssw0rd!${uniqueString(resourceGroup().id)}' // Not used with Entra ID
    storage: {
      storageSizeGB: storageSize
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: environment == 'prod' ? 'Enabled' : 'Disabled'
    }
    highAvailability: environment == 'prod' ? {
      mode: 'ZoneRedundant'
      standbyAvailabilityZone: '2'
    } : {
      mode: 'Disabled'
    }
    maintenanceWindow: {
      customWindow: 'Enabled'
      dayOfWeek: 0 // Sunday
      startHour: 2 // 02:00 SAST (minimize user impact)
      startMinute: 0
    }
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Disabled' // Passwordless authentication (SP-05)
    }
  }
}

// ============================================================================
// ENTRA ID ADMIN CONFIGURATION
// ============================================================================

resource postgresAdministrator 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2023-03-01-preview' = {
  name: adminObjectId
  parent: postgresServer
  properties: {
    principalType: 'User'
    principalName: adminLogin
    tenantId: subscription().tenantId
  }
}

// ============================================================================
// FIREWALL RULES
// ============================================================================

// Allow Azure services to connect
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  name: 'AllowAzureServices'
  parent: postgresServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Allow specific IP ranges for developer access (parameterized in production)
resource allowDevelopers 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = if (environment == 'dev') {
  name: 'AllowDevelopers'
  parent: postgresServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255' // Dev only - restrict in prod
  }
}

// ============================================================================
// DATABASE
// ============================================================================

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  name: databaseName
  parent: postgresServer
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// ============================================================================
// POSTGRESQL EXTENSIONS (PostGIS for future geo-features)
// ============================================================================

resource postgisExtension 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-03-01-preview' = {
  name: 'azure.extensions'
  parent: postgresServer
  properties: {
    value: 'POSTGIS,UUID-OSSP'
    source: 'user-override'
  }
}

// ============================================================================
// DIAGNOSTIC SETTINGS
// ============================================================================

resource postgresDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${postgresServerName}-diagnostics'
  scope: postgresServer
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'PostgreSQLLogs'
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
          days: 30
        }
      }
    ]
  }
}

// ============================================================================
// OUTPUTS
// ============================================================================

output postgresServerName string = postgresServer.name
output postgresServerId string = postgresServer.id
output postgresServerFqdn string = postgresServer.properties.fullyQualifiedDomainName
output databaseName string = database.name

// Connection string format for Key Vault storage
output connectionStringFormat string = 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=${databaseName};Username={username};Password={password};SSL Mode=Require'
