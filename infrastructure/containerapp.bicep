@description('Name of the Container App')
param containerAppName string

@description('Location for the Container App')
param location string = resourceGroup().location

@description('Name of the Container Apps Environment')
param containerAppsEnvironmentName string

@description('Resource group of the Container Apps Environment')
param containerAppsEnvironmentResourceGroup string

@description('Container image to deploy')
param containerImage string

@description('Container registry server')
param containerRegistryServer string = 'ghcr.io'

@description('Container registry username')
@secure()
param containerRegistryUsername string

@description('Container registry password')
@secure()
param containerRegistryPassword string

@description('Minimum replica count')
param minReplicas int = 0

@description('Maximum replica count')
param maxReplicas int = 10

@description('CPU threshold for scaling')
param cpuThreshold int = 80

@description('Memory threshold for scaling')
param memoryThreshold int = 80

// Reference to the existing Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppsEnvironmentName
  scope: resourceGroup(containerAppsEnvironmentResourceGroup)
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: [
        {
          server: containerRegistryServer
          username: containerRegistryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistryPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'fritz-app'
          image: containerImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'cpu-scaling'
            custom: {
              type: 'cpu'
              metadata: {
                type: 'Utilization'
                value: string(cpuThreshold)
              }
            }
          }
          {
            name: 'memory-scaling'
            custom: {
              type: 'memory'
              metadata: {
                type: 'Utilization'
                value: string(memoryThreshold)
              }
            }
          }
        ]
      }
    }
  }
}

@description('FQDN of the Container App')
output fqdn string = containerApp.properties.configuration.ingress.fqdn

@description('Container App URL')
output url string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
