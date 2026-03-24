// Application Insights and Log Analytics module
// Provisions monitoring infrastructure for Digital Stokvel

@description('Environment name')
param environment string

@description('Location for resources')
param location string

@description('Project name')
param projectName string

@description('Resource tags')
param tags object

// ============================================================================
// VARIABLES
// ============================================================================

var resourceToken = toLower('${projectName}-${environment}')
var logAnalyticsName = '${resourceToken}-logs'
var appInsightsName = '${resourceToken}-appinsights'

// Log retention based on environment
var logRetentionDays = environment == 'prod' ? 90 : 30

// ============================================================================
// LOG ANALYTICS WORKSPACE
// ============================================================================

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: logRetentionDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: environment == 'prod' ? 10 : 1
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// ============================================================================
// APPLICATION INSIGHTS
// ============================================================================

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    RetentionInDays: logRetentionDays
  }
}

// ============================================================================
// ALERTS - NF-11 Requirements
// ============================================================================

// Action Group for alert notifications
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${resourceToken}-alerts-actiongroup'
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'StokvAlerts'
    enabled: true
    emailReceivers: [
      {
        name: 'DevOpsTeam'
        emailAddress: 'devops@stokvel.bank'
        useCommonAlertSchema: true
      }
    ]
    smsReceivers: environment == 'prod' ? [
      {
        name: 'OnCallSMS'
        countryCode: '27'
        phoneNumber: '0821234567' // Replace with actual on-call number
      }
    ] : []
  }
}

// Alert 1: API Response Time > 1 second (NF-01)
resource apiResponseTimeAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${resourceToken}-api-response-time-alert'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when API response time exceeds 1 second (95th percentile)'
    severity: 2
    enabled: true
    scopes: [
      appInsights.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'ResponseTime'
          metricName: 'requests/duration'
          dimensions: []
          operator: 'GreaterThan'
          threshold: 1000
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Alert 2: Error Rate > 5%
resource errorRateAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${resourceToken}-error-rate-alert'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when API error rate exceeds 5%'
    severity: 1
    enabled: true
    scopes: [
      appInsights.id
    ]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'FailedRequests'
          metricName: 'requests/failed'
          dimensions: []
          operator: 'GreaterThan'
          threshold: 5
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Alert 3: USSD Success Rate < 90% (Custom metric - configured in backend)
resource ussdSuccessAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: '${resourceToken}-ussd-success-alert'
  location: location
  tags: tags
  properties: {
    description: 'Alert when USSD session success rate drops below 90%'
    severity: 2
    enabled: true
    evaluationFrequency: 'PT5M'
    scopes: [
      logAnalytics.id
    ]
    windowSize: 'PT10M'
    criteria: {
      allOf: [
        {
          query: 'customMetrics | where name == "USSDSessionCompleted" | summarize SuccessRate = (todouble(sum(valueCount)) / todouble(count())) * 100'
          timeAggregation: 'Average'
          dimensions: []
          operator: 'LessThan'
          threshold: 90
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [
        actionGroup.id
      ]
    }
  }
}

// Alert 4: API Downtime > 5 minutes
resource apiDowntimeAlert 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: '${resourceToken}-api-downtime-alert'
  location: location
  tags: tags
  properties: {
    description: 'Alert when API is down for more than 5 minutes'
    severity: 0
    enabled: true
    evaluationFrequency: 'PT1M'
    scopes: [
      logAnalytics.id
    ]
    windowSize: 'PT5M'
    criteria: {
      allOf: [
        {
          query: 'requests | where success == false and resultCode startswith "5" | summarize FailedCount = count() by bin(timestamp, 1m)'
          timeAggregation: 'Count'
          dimensions: []
          operator: 'GreaterThan'
          threshold: 10
          failingPeriods: {
            numberOfEvaluationPeriods: 5
            minFailingPeriodsToAlert: 5
          }
        }
      ]
    }
    actions: {
      actionGroups: [
        actionGroup.id
      ]
    }
  }
}

// ============================================================================
// OUTPUTS
// ============================================================================

output logAnalyticsWorkspaceId string = logAnalytics.id
output logAnalyticsWorkspaceName string = logAnalytics.name
output logAnalyticsCustomerId string = logAnalytics.properties.customerId

output applicationInsightsId string = appInsights.id
output applicationInsightsName string = appInsights.name
output applicationInsightsConnectionString string = appInsights.properties.ConnectionString
output applicationInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey

output actionGroupId string = actionGroup.id
