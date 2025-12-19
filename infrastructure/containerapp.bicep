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
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-07-01' existing = {
  name: containerAppsEnvironmentName
  scope: resourceGroup(containerAppsEnvironmentResourceGroup)
}

resource containerApp 'Microsoft.App/containerApps@2025-07-01' = {
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
        // Custom domain configuration with automatic certificate management
        // Using bindingType 'Auto' allows Azure to automatically create and bind the managed certificate
        // This solves the circular dependency issue where the certificate requires the domain to exist first
        customDomains: !empty(customDomain) ? [
          {
            name: customDomain
            bindingType: 'Auto'
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

// Managed certificate for custom domain (only created if customDomain is provided)
// NOTE: The certificate MUST be deployed in the Container Apps Environment's resource group,
// not the app's resource group. This is an Azure platform requirement for managed certificates.
// The certificate is created AFTER the container app to ensure the custom domain exists first.
// With bindingType 'Auto', Azure automatically binds the certificate once it's provisioned.
module managedCertificate 'managedcertificate.bicep' = if (!empty(customDomain)) {
  name: 'managedCertificateDeployment'
  scope: resourceGroup(containerAppsEnvironmentResourceGroup)
  params: {
    containerAppsEnvironmentName: containerAppsEnvironmentName
    customDomain: customDomain
    location: location
  }
  dependsOn: [
    containerApp
  ]
}

@description('FQDN of the Container App')
output fqdn string = containerApp.properties.configuration.ingress.fqdn

@description('Container App URL')
output url string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
