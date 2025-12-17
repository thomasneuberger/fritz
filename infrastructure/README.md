# Azure Infrastructure

This directory contains Bicep templates for deploying the Fritz application to Azure Container Apps.

## Overview

The infrastructure consists of:
- **main.bicep**: Main orchestration template
- **containerapp.bicep**: Azure Container App configuration with autoscaling

## Prerequisites

- Azure subscription
- Azure Container Apps Environment (must be created beforehand)
- Azure CLI installed locally for manual deployments

## Azure Resources

### Container App
- **Type**: Azure Container App
- **Hosting**: Runs the Fritz Blazor WebAssembly app with nginx
- **Scaling**: Configured with autoscaling
  - Minimum replicas: 0 (can scale to zero for cost optimization)
  - Maximum replicas: 10
  - Scaling rules: HTTP-based (concurrent requests per replica)

## Required GitHub Secrets

For the CD workflow to deploy to Azure, configure these secrets in your GitHub repository:

| Secret Name | Description |
|-------------|-------------|
| `AZURE_CLIENT_ID` | Azure service principal client ID for OIDC authentication |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_RESOURCE_GROUP` | Resource group where the Container App will be deployed |
| `AZURE_CONTAINER_APP_NAME` | Name for the Container App |
| `AZURE_CONTAINER_ENV_NAME` | Name of the existing Container Apps Environment |
| `AZURE_CONTAINER_ENV_RESOURCE_GROUP` | Resource group containing the Container Apps Environment |

## Azure Setup

### 1. Create Container Apps Environment (if not already exists)

```bash
az containerapp env create \
  --name <your-environment-name> \
  --resource-group <your-resource-group> \
  --location westeurope
```

### 2. Configure Azure for GitHub Actions (OIDC)

Create a service principal with federated credentials for GitHub Actions:

```bash
# Create a resource group (if needed)
az group create --name <your-resource-group> --location westeurope

# Create a service principal
az ad sp create-for-rbac \
  --name "fritz-github-actions" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/<your-resource-group> \
  --sdk-auth

# Configure federated credentials for GitHub
az ad app federated-credential create \
  --id <app-id> \
  --parameters '{
    "name": "fritz-github-actions",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:<your-github-org>/<your-repo-name>:ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

## Manual Deployment

To deploy manually using Azure CLI:

```bash
# Login to Azure
az login

# Set your subscription
az account set --subscription <subscription-id>

# Deploy the infrastructure
az deployment group create \
  --resource-group <your-resource-group> \
  --template-file infrastructure/main.bicep \
  --parameters \
    containerAppName="fritz-app" \
    containerAppsEnvironmentName="<your-environment-name>" \
    containerAppsEnvironmentResourceGroup="<environment-resource-group>" \
    containerImage="ghcr.io/thomasneuberger/fritz:latest" \
    containerRegistryUsername="<your-github-username>" \
    containerRegistryPassword="<your-github-token>" \
    minReplicas=0
```

## Template Parameters

### Required Parameters

| Parameter | Description |
|-----------|-------------|
| `containerAppName` | Name of the Container App to create |
| `containerAppsEnvironmentName` | Name of the existing Container Apps Environment |
| `containerAppsEnvironmentResourceGroup` | Resource group of the Container Apps Environment |
| `containerImage` | Full container image reference (e.g., ghcr.io/thomasneuberger/fritz:latest) |
| `containerRegistryUsername` | Username for container registry authentication |
| `containerRegistryPassword` | Password/token for container registry authentication |

### Optional Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `location` | Azure region for resources | Resource group location |
| `containerRegistryServer` | Container registry server | ghcr.io |
| `minReplicas` | Minimum number of replicas (autoscaling) | 0 |
| `maxReplicas` | Maximum number of replicas (autoscaling) | 10 |
| `concurrentRequests` | Concurrent requests per replica for HTTP scaling | 10 |

## Autoscaling Configuration

The Container App is configured with HTTP-based autoscaling:
- **Minimum instances**: 0 (can scale to zero to save costs when no traffic)
- **Maximum instances**: 10
- **Scaling trigger**: HTTP concurrent requests
  - Default: 10 concurrent requests per replica
  - When traffic exceeds this threshold, new replicas are automatically created
  - When traffic decreases, replicas scale down (including to zero)

This ensures the app scales automatically based on actual HTTP traffic while being cost-effective when idle.

## Outputs

After deployment, the following outputs are available:
- `fqdn`: Fully qualified domain name of the Container App
- `url`: HTTPS URL to access the application

## Notes

- The Container App uses HTTPS by default (HTTP is disabled)
- The app is exposed externally and accessible from the internet
- Container registry credentials are stored as secrets in the Container App
- The deployment uses the image built in the CD workflow
