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

@description('CPU threshold percentage for scaling')
param cpuThreshold int = 80

@description('Memory threshold percentage for scaling')
param memoryThreshold int = 80

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
    cpuThreshold: cpuThreshold
    memoryThreshold: memoryThreshold
  }
}

@description('FQDN of the deployed Container App')
output fqdn string = containerApp.outputs.fqdn

@description('URL of the deployed Container App')
output url string = containerApp.outputs.url
