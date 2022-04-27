param location string = resourceGroup().location

var shared_config = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Development'
  }
  {
    name: 'StorageConnectionString'
    value: format('DefaultEndpointsProtocol=https;AccountName=${storage.outputs.storageName};AccountKey=${storage.outputs.accountKey};EndpointSuffix=core.windows.net')
  }
]

resource acr 'Microsoft.ContainerRegistry/registries@2021-09-01' = {
  name: toLower('${resourceGroup().name}acr')
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

module env 'environment.bicep' = {
  name: 'containerAppEnvironment'
  params: {
    location: location
  }
}

module storage 'storage.bicep' = {
  name: toLower('${resourceGroup().name}strg')
  params: {
    location: location
  }
}

module silo 'container-app-no-ingress.bicep' = {
  name: 'silo'
  params: {
    location: location
    name: 'silo'
    containerAppEnvironmentId: env.outputs.id
    registry: acr.name
    registryPassword: acr.listCredentials().passwords[0].value
    registryUsername: acr.listCredentials().username
    maxReplicas: 1
    envVars : shared_config
  }
}

module dashboard 'container-app-with-ingress.bicep' = {
  name: 'dashboard'
  params: {
    location: location
    name: 'dashboard'
    containerAppEnvironmentId: env.outputs.id
    registry: acr.name
    registryPassword: acr.listCredentials().passwords[0].value
    registryUsername: acr.listCredentials().username
    allowExternalIngress: true
    targetIngressPort: 8080
    maxReplicas: 1
    envVars : shared_config
  }
}

module minimalapiclient 'container-app-with-ingress.bicep' = {
  name: 'minimalapiclient'
  params: {
    location: location
    name: 'minimalapiclient'
    containerAppEnvironmentId: env.outputs.id
    registry: acr.name
    registryPassword: acr.listCredentials().passwords[0].value
    registryUsername: acr.listCredentials().username
    allowExternalIngress: true
    targetIngressPort: 80
    maxReplicas: 1
    envVars : shared_config
  }
}

module workerserviceclient 'container-app-no-ingress.bicep' = {
  name: 'workerserviceclient'
  params: {
    location: location
    name: 'workerserviceclient'
    containerAppEnvironmentId: env.outputs.id
    registry: acr.name
    registryPassword: acr.listCredentials().passwords[0].value
    registryUsername: acr.listCredentials().username
    maxReplicas: 1
    envVars : shared_config
  }
}

