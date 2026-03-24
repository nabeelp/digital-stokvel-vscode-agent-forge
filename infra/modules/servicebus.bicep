// Azure Service Bus module
// Provisions Service Bus for async message processing

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
var serviceBusNamespaceName = '${resourceToken}-sb'

// SKU based on environment
var serviceBusSku = environment == 'prod' ? 'Standard' : 'Basic'

// ============================================================================
// SERVICE BUS NAMESPACE
// ============================================================================

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  tags: tags
  sku: {
    name: serviceBusSku
    tier: serviceBusSku
  }
  properties: {
    minimumTlsVersion: '1.2' // TLS 1.2+ enforcement (SP-04)
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true // Force Azure AD authentication (SP-05)
    zoneRedundant: environment == 'prod' ? true : false
  }
}

// ============================================================================
// QUEUES
// ============================================================================

// Queue: Contribution processing
resource contributionQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'contribution-processing'
  parent: serviceBusNamespace
  properties: {
    maxDeliveryCount: 5 // Dead-letter after 5 retries
    lockDuration: 'PT5M' // 5-minute lock duration
    defaultMessageTimeToLive: 'P14D' // 14 days
    deadLetteringOnMessageExpiration: true
    enableBatchedOperations: true
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
  }
}

// Queue: Payout processing
resource payoutQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'payout-processing'
  parent: serviceBusNamespace
  properties: {
    maxDeliveryCount: 5
    lockDuration: 'PT5M'
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
    enableBatchedOperations: true
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
  }
}

// Queue: Notification dispatch (SMS + push)
resource notificationQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'notification-dispatch'
  parent: serviceBusNamespace
  properties: {
    maxDeliveryCount: 5
    lockDuration: 'PT3M'
    defaultMessageTimeToLive: 'P7D' // 7 days
    deadLetteringOnMessageExpiration: true
    enableBatchedOperations: true
  }
}

// Queue: Interest capitalization (monthly batch job)
resource interestQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: 'interest-capitalization'
  parent: serviceBusNamespace
  properties: {
    maxDeliveryCount: 3
    lockDuration: 'PT10M' // Longer lock for batch processing
    defaultMessageTimeToLive: 'P30D' // 30 days
    deadLetteringOnMessageExpiration: true
    enableBatchedOperations: true
  }
}

// ============================================================================
// TOPICS AND SUBSCRIPTIONS
// ============================================================================

// Topic: Group events (for audit and analytics)
resource groupEventsTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  name: 'group-events'
  parent: serviceBusNamespace
  properties: {
    defaultMessageTimeToLive: 'P14D'
    maxSizeInMegabytes: 1024
    enableBatchedOperations: true
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
  }
}

// Subscription: Audit logging
resource auditSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  name: 'audit-subscription'
  parent: groupEventsTopic
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
  }
}

// Subscription: Analytics processing
resource analyticsSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  name: 'analytics-subscription'
  parent: groupEventsTopic
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
  }
}

// ============================================================================
// DIAGNOSTIC SETTINGS
// ============================================================================

resource serviceBusDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${serviceBusNamespaceName}-diagnostics'
  scope: serviceBusNamespace
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'OperationalLogs'
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

output serviceBusNamespaceName string = serviceBusNamespace.name
output serviceBusNamespaceId string = serviceBusNamespace.id
output serviceBusFqdn string = '${serviceBusNamespaceName}.servicebus.windows.net'
