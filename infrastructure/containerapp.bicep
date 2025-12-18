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

@description('Concurrent requests per replica for HTTP scaling')
param concurrentRequests int = 10

@description('Custom domain name for the Container App (optional)')
param customDomain string = ''

// Reference to the existing Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppsEnvironmentName
  scope: resourceGroup(containerAppsEnvironmentResourceGroup)
}

// Managed certificate for custom domain (only created if customDomain is provided)
module managedCertificate 'managedcertificate.bicep' = if (!empty(customDomain)) {
  name: 'managedCertificateDeployment'
  scope: resourceGroup(containerAppsEnvironmentResourceGroup)
  params: {
    containerAppsEnvironmentName: containerAppsEnvironmentName
    customDomain: customDomain
    location: location
  }
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
        customDomains: !empty(customDomain) ? [
          {
            name: customDomain
            certificateId: managedCertificate.outputs.certificateId
            bindingType: 'SniEnabled'
          }
        ] : []
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
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: string(concurrentRequests)
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
