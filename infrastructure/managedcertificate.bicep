@description('Name of the Container Apps Environment')
param containerAppsEnvironmentName string

@description('Custom domain name')
param customDomain string

@description('Location for resources')
param location string

// Reference to the existing Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppsEnvironmentName
}

// Managed certificate for custom domain
resource managedCertificate 'Microsoft.App/managedEnvironments/managedCertificates@2024-03-01' = {
  name: '${replace(customDomain, '.', '-')}-certificate'
  parent: containerAppsEnvironment
  location: location
  properties: {
    subjectName: customDomain
    domainControlValidation: 'CNAME'
  }
}

@description('ID of the managed certificate')
output certificateId string = managedCertificate.id
