param baseName string = resourceGroup().name
param location string = resourceGroup().location

resource env 'Microsoft.App/managedEnvironments@2023-08-01-preview' = {
  name: '${baseName}env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'azure-monitor'
    }
  }
}

output id string = env.id