@description('Name of the Container App')
param containerAppName string

@description('Location for resources')
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

@description('Minimum replica count for autoscaling')
param minReplicas int = 0

@description('Maximum replica count for autoscaling')
param maxReplicas int = 10

@description('Concurrent requests per replica for HTTP scaling')
param concurrentRequests int = 10

module containerApp 'containerapp.bicep' = {
  name: 'containerAppDeployment'
  params: {
    containerAppName: containerAppName
    location: location
    containerAppsEnvironmentName: containerAppsEnvironmentName
    containerAppsEnvironmentResourceGroup: containerAppsEnvironmentResourceGroup
    containerImage: containerImage
    containerRegistryServer: containerRegistryServer
    containerRegistryUsername: containerRegistryUsername
    containerRegistryPassword: containerRegistryPassword
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    concurrentRequests: concurrentRequests
  }
}

@description('FQDN of the deployed Container App')
output fqdn string = containerApp.outputs.fqdn

@description('URL of the deployed Container App')
output url string = containerApp.outputs.url
