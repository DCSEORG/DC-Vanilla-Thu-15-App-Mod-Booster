// App Service Bicep template

@description('Location for the App Service')
param location string

@description('Name of the App Service')
param appServiceName string

@description('Resource ID of the user-assigned managed identity')
param managedIdentityId string

var appServicePlanName = 'plan-${appServiceName}'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: []
    }
  }
}

output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output appServiceId string = appService.id
