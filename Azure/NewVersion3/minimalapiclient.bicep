param name string
param location string = resourceGroup().location
param containerAppEnvironmentId string
param repositoryImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
param envVars array = []
param allowExternalIngress bool = false
param targetIngressPort int = 80
param registry string
param registryUsername string
param minReplicas int = 1
param maxReplicas int = 1
@secure()
param registryPassword string

param workloadProfileName string = 'Consumption'

resource containerApp 'Microsoft.App/containerApps@2025-02-02-preview' = {
  name: name
  location: location
  properties: {
    managedEnvironmentId: containerAppEnvironmentId
    workloadProfileName: workloadProfileName
    configuration: {
      secrets: [
        {
          name: 'container-registry-password'
          value: registryPassword
        }
      ]
      registries: [
        {
          server: registry
          username: registryUsername
          passwordSecretRef: 'container-registry-password'
        }
      ]
      ingress: {
        external: allowExternalIngress
        targetPort: targetIngressPort
      }
    }
    template: {
      containers: [
        {
          image: repositoryImage
          name: name
          env: envVars
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
