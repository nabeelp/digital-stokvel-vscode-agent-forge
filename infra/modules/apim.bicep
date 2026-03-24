// Azure API Management module
// Provisions APIM for USSD gateway abstraction and rate limiting

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

@description('Application Insights ID')
param applicationInsightsId string

@description('Application Insights instrumentation key')
param applicationInsightsInstrumentationKey string

@description('Backend API URL from Container Apps')
param backendApiUrl string

// ============================================================================
// VARIABLES
// ============================================================================

var resourceToken = toLower('${projectName}-${environment}')
var apimName = '${resourceToken}-apim'
var publisherEmail = 'devops@stokvel.bank'
var publisherName = 'Digital Stokvel Banking'

// SKU based on environment
var apimSku = environment == 'prod' ? {
  name: 'Standard'
  capacity: 1
} : {
  name: 'Consumption'
  capacity: 0
}

// ============================================================================
// API MANAGEMENT SERVICE
// ============================================================================

resource apim 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apimName
  location: location
  tags: tags
  sku: apimSku
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Ssl30': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Ssl30': 'false'
    }
    publicNetworkAccess: 'Enabled'
  }
}

// ============================================================================
// APPLICATION INSIGHTS INTEGRATION
// ============================================================================

resource apimLogger 'Microsoft.ApiManagement/service/loggers@2023-05-01-preview' = {
  name: 'appinsights-logger'
  parent: apim
  properties: {
    loggerType: 'applicationInsights'
    credentials: {
      instrumentationKey: applicationInsightsInstrumentationKey
    }
    isBuffered: true
    resourceId: applicationInsightsId
  }
}

// ============================================================================
// BACKEND: CONTAINER APPS API
// ============================================================================

resource backendApi 'Microsoft.ApiManagement/service/backends@2023-05-01-preview' = {
  name: 'stokvel-backend-api'
  parent: apim
  properties: {
    title: 'Digital Stokvel Backend API'
    description: 'Container Apps backend service'
    protocol: 'http'
    url: backendApiUrl
    tls: {
      validateCertificateChain: true
      validateCertificateName: true
    }
  }
}

// ============================================================================
// API: STOKVEL BACKEND API
// ============================================================================

resource api 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  name: 'stokvel-api'
  parent: apim
  properties: {
    displayName: 'Digital Stokvel API'
    apiRevision: '1'
    subscriptionRequired: true
    path: 'api'
    protocols: [
      'https'
    ]
    serviceUrl: backendApiUrl
    isCurrent: true
  }
}

// Global API Policy: Rate limiting and CORS
resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {
  name: 'policy'
  parent: api
  properties: {
    value: '''
      <policies>
        <inbound>
          <base />
          <!-- Rate limiting: 100 requests per minute per subscription key (NF-02) -->
          <rate-limit-by-key calls="100" renewal-period="60" counter-key="@(context.Subscription.Id)" />
          
          <!-- CORS for web dashboard -->
          <cors allow-credentials="false">
            <allowed-origins>
              <origin>https://chairperson-dashboard.stokvel.bank</origin>
            </allowed-origins>
            <allowed-methods>
              <method>GET</method>
              <method>POST</method>
              <method>PUT</method>
              <method>DELETE</method>
            </allowed-methods>
            <allowed-headers>
              <header>*</header>
            </allowed-headers>
          </cors>
          
          <!-- Forward to backend -->
          <set-backend-service backend-id="stokvel-backend-api" />
        </inbound>
        <backend>
          <base />
        </backend>
        <outbound>
          <base />
        </outbound>
        <on-error>
          <base />
        </on-error>
      </policies>
    '''
    format: 'xml'
  }
}

// ============================================================================
// API: USSD GATEWAY (MNO Abstraction)
// ============================================================================

resource ussdApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  name: 'ussd-gateway-api'
  parent: apim
  properties: {
    displayName: 'USSD Gateway API'
    apiRevision: '1'
    subscriptionRequired: true
    path: 'ussd'
    protocols: [
      'https'
    ]
    serviceUrl: backendApiUrl
  }
}

// USSD API Policy: Rate limiting per MNO
resource ussdApiPolicy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {
  name: 'policy'
  parent: ussdApi
  properties: {
    value: '''
      <policies>
        <inbound>
          <base />
          <!-- Rate limiting: 100 requests per minute per MNO -->
          <rate-limit-by-key calls="100" renewal-period="60" counter-key="@(context.Request.IpAddress)" />
          
          <!-- Validate MNO signature (placeholder - implement with JWT or HMAC) -->
          <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized MNO">
            <issuer-signing-keys>
              <key>{{MNO-Signature-Key}}</key>
            </issuer-signing-keys>
          </validate-jwt>
          
          <!-- Forward to backend -->
          <set-backend-service backend-id="stokvel-backend-api" />
        </inbound>
        <backend>
          <base />
        </backend>
        <outbound>
          <base />
        </outbound>
        <on-error>
          <base />
        </on-error>
      </policies>
    '''
    format: 'xml'
  }
}

// ============================================================================
// PRODUCT: USSD GATEWAY (with subscription key)
// ============================================================================

resource ussdProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  name: 'ussd-gateway-product'
  parent: apim
  properties: {
    displayName: 'USSD Gateway Product'
    description: 'Product for MNO USSD gateway integration'
    subscriptionRequired: true
    approvalRequired: true
    state: 'published'
  }
}

resource ussdProductApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  name: 'ussd-gateway-api'
  parent: ussdProduct
  dependsOn: [
    ussdApi
  ]
}

// ============================================================================
// DIAGNOSTIC SETTINGS
// ============================================================================

resource apimDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${apimName}-diagnostics'
  scope: apim
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        category: 'GatewayLogs'
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

output apimName string = apim.name
output apimId string = apim.id
output apimGatewayUrl string = apim.properties.gatewayUrl
output apimDeveloperPortalUrl string = apim.properties.developerPortalUrl
