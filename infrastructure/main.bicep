// Main Bicep template for Expense Management System
// Deploys App Service, Managed Identity, Azure SQL, and optionally Azure OpenAI

@description('Location for all resources')
param location string = 'uksouth'

@description('Admin object ID for SQL Server Entra ID authentication')
param adminObjectId string

@description('Admin login for SQL Server Entra ID authentication')
param adminLogin string

@description('Whether to deploy GenAI resources (Azure OpenAI)')
param deployGenAI bool = false

var uniqueSuffix = uniqueString(resourceGroup().id)
var appServiceName = 'app-expensemgmt-${uniqueSuffix}'
var sqlServerName = 'sql-expensemgmt-${uniqueSuffix}'
var databaseName = 'expensedb'

// Deploy Managed Identity
module managedIdentity 'managed-identity.bicep' = {
  name: 'managedIdentityDeployment'
  params: {
    location: location
    identityName: 'mid-expensemgmt-${uniqueSuffix}'
  }
}

// Deploy App Service
module appService 'app-service.bicep' = {
  name: 'appServiceDeployment'
  params: {
    location: location
    appServiceName: appServiceName
    managedIdentityId: managedIdentity.outputs.identityId
  }
}

// Deploy Azure SQL
module azureSQL 'azure-sql.bicep' = {
  name: 'azureSQLDeployment'
  params: {
    location: location
    sqlServerName: sqlServerName
    databaseName: databaseName
    adminObjectId: adminObjectId
    adminLogin: adminLogin
    managedIdentityPrincipalId: managedIdentity.outputs.principalId
  }
}

// Deploy GenAI resources (conditional)
module genAI 'genai.bicep' = if (deployGenAI) {
  name: 'genAIDeployment'
  params: {
    location: location
    managedIdentityPrincipalId: managedIdentity.outputs.principalId
  }
}

// Outputs
output appServiceName string = appService.outputs.appServiceName
output appServiceUrl string = appService.outputs.appServiceUrl
output sqlServerFqdn string = azureSQL.outputs.sqlServerFqdn
output databaseName string = azureSQL.outputs.databaseName
output managedIdentityClientId string = managedIdentity.outputs.clientId
output managedIdentityPrincipalId string = managedIdentity.outputs.principalId

// GenAI outputs (only if deployed)
output openAIEndpoint string = deployGenAI ? genAI.outputs.openAIEndpoint : ''
output openAIModelName string = deployGenAI ? genAI.outputs.openAIModelName : ''
output openAIName string = deployGenAI ? genAI.outputs.openAIName : ''
