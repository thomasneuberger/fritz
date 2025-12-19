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

## Required GitHub Environment Secrets

For the CD workflow to deploy to Azure, configure these secrets in your GitHub repository under the `development` environment (Settings → Environments → development):

| Secret Name | Description |
|-------------|-------------|
| `AZURE_CLIENT_ID` | Azure service principal client ID for OIDC authentication |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_RESOURCE_GROUP` | Resource group where the Container App will be deployed |
| `AZURE_CONTAINER_APP_NAME` | Name for the Container App |
| `AZURE_CONTAINER_ENV_NAME` | Name of the existing Container Apps Environment |
| `AZURE_CONTAINER_ENV_RESOURCE_GROUP` | Resource group containing the Container Apps Environment |
| `CONTAINER_REGISTRY_USERNAME` | Username for GitHub Container Registry authentication (your GitHub username) |
| `CONTAINER_REGISTRY_PASSWORD` | Personal Access Token (PAT) for GitHub Container Registry authentication |
| `CUSTOM_DOMAIN` | (Optional) Custom domain name for the Container App (e.g., fritz.example.com) |

### Setting up Container Registry Authentication

The Container App needs to authenticate with GitHub Container Registry (GHCR) to pull the container image. This requires a Personal Access Token (PAT) with appropriate permissions.

**Why a PAT is required:**
- The default `GITHUB_TOKEN` used for publishing images does not have sufficient permissions for Azure Container Apps to pull images
- A PAT with `read:packages` scope provides the necessary authentication for image pulls

**To create a PAT:**
1. Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Give it a descriptive name (e.g., "Fritz Container Registry Access")
4. Select the `read:packages` scope
5. Set an appropriate expiration date
6. Generate the token and copy it immediately (you won't be able to see it again)
7. Add the token as the `CONTAINER_REGISTRY_PASSWORD` secret in the `development` environment (Repository Settings → Environments → development → Add secret)
8. Also add your GitHub username as the `CONTAINER_REGISTRY_USERNAME` secret in the same environment

## Custom Domain Configuration

The Container App supports custom domains with automatic HTTPS using Azure-managed certificates.

### Setting Up a Custom Domain

1. **Add the CUSTOM_DOMAIN secret**: In your GitHub repository, go to Settings → Environments → development → Add secret, and add a secret named `CUSTOM_DOMAIN` with your domain name (e.g., `fritz.example.com`).

2. **Configure DNS settings**: Before deploying, configure your DNS provider to point your custom domain to the Container App:
   - Create a CNAME record pointing your custom domain to the Container App's default FQDN
   - Example: `fritz.example.com` → `<your-container-app-name>.<region>.azurecontainerapps.io`
   - DNS propagation may take a few minutes to several hours

3. **Deploy the application**: Once DNS is configured and the `CUSTOM_DOMAIN` secret is set, deploy the application using the CD workflow or manual deployment.

4. **Certificate provisioning**: Azure will automatically provision a managed certificate for your custom domain:
   - The certificate uses CNAME-based domain validation
   - Certificate provisioning is automatic and may take a few minutes
   - The certificate is automatically renewed before expiration
   - HTTPS will be enabled automatically once the certificate is provisioned

### Managed Certificate Resource Placement

**Important**: Managed certificates for Azure Container Apps are created as child resources of the Container Apps Environment and **must be in the same resource group as the environment**, not the Container App itself. This is an Azure platform requirement.

- If your Container App and Container Apps Environment are in different resource groups, the managed certificate will be created in the environment's resource group
- The deployment will create the certificate resource at:
  ```
  /subscriptions/<subscription-id>/resourceGroups/<environment-resource-group>/
  providers/Microsoft.App/managedEnvironments/<environment-name>/
  managedCertificates/<certificate-name>
  ```
- This behavior is by design and cannot be changed when using Azure-managed certificates

**Required Permissions for Cross-Resource-Group Deployments:**

When the Container App is deployed to a different resource group than the Container Apps Environment, the service principal needs permissions to create managed certificates in the environment's resource group. You have two options:

1. **Built-in Contributor role** (simpler, but provides broader access to the resource group)
2. **Custom role** with specific permissions (recommended for production, follows least privilege principle):
   - `Microsoft.App/managedEnvironments/managedCertificates/write`
   - `Microsoft.App/managedEnvironments/managedCertificates/read`
   - `Microsoft.App/managedEnvironments/read`

See the "Azure Setup" section below for detailed permission configuration with both options.

### Important Notes

- **DNS must be configured first**: Ensure your DNS CNAME record is properly configured and propagated before deploying with a custom domain. The managed certificate provisioning will fail if DNS is not correctly configured.
- **Custom domain is optional**: If you don't set the `CUSTOM_DOMAIN` secret, the app will use the default Azure Container Apps domain.
- **HTTPS only**: The Container App is configured to use HTTPS only; HTTP requests are not allowed.

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

# Create a service principal and capture the output
az ad sp create-for-rbac \
  --name "fritz-github-actions" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/<your-resource-group>

# Note the appId (client ID) from the output above

# Configure federated credentials for GitHub
# Replace <app-id> with the appId value from the service principal creation output
az ad app federated-credential create \
  --id <app-id> \
  --parameters '{
    "name": "fritz-github-actions",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:<your-github-org>/<your-repo-name>:environment:development",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

**Required Permissions:**

The service principal needs the following permissions:
- **Contributor role** on the resource group where the Container App will be deployed (to create and manage the Container App resource)
- **Container Apps Contributor role** on the Container Apps Environment (to deploy to the environment)
- **Reader role** on the resource group containing the Azure Container Apps Environment (to reference the existing environment)

If the Container Apps Environment is in a different resource group than where you're deploying the Container App, grant additional permissions:

**Option 1: Using Built-in Contributor Role (Simpler)**

```bash
# Grant Contributor access to the Container Apps Environment resource group
# This is required when using custom domains with managed certificates
az role assignment create \
  --assignee <app-id> \
  --role Contributor \
  --scope /subscriptions/<subscription-id>/resourceGroups/<environment-resource-group>
```

**Option 2: Using Custom Role (More Secure - Recommended)**

For better security with least privilege access, create a custom role with only the necessary permissions:

```bash
# Create a custom role definition file (managed-cert-role.json)
cat > managed-cert-role.json <<EOF
{
  "Name": "Container Apps Managed Certificate Manager",
  "IsCustom": true,
  "Description": "Allows management of managed certificates in Container Apps Environments",
  "Actions": [
    "Microsoft.App/managedEnvironments/managedCertificates/write",
    "Microsoft.App/managedEnvironments/managedCertificates/read",
    "Microsoft.App/managedEnvironments/read"
  ],
  "AssignableScopes": [
    "/subscriptions/<subscription-id>"
  ]
}
EOF

# Create the custom role at subscription level for reusability
az role definition create --role-definition managed-cert-role.json

# Assign the custom role to the service principal
az role assignment create \
  --assignee <app-id> \
  --role "Container Apps Managed Certificate Manager" \
  --scope /subscriptions/<subscription-id>/resourceGroups/<environment-resource-group>
```

**Note**: When using custom domains with managed certificates, the certificates must be created in the environment's resource group (not the app's resource group). This is an Azure platform requirement. The built-in "Container Apps Contributor" role mentioned earlier does not include permissions for managing certificates. This is why you need either the broader "Contributor" role or a custom role with specific permissions.

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
    containerRegistryPassword="<your-github-pat>" \
    minReplicas=0 \
    customDomain="<your-custom-domain>"
```

**Note:** Replace `<your-github-pat>` with a GitHub Personal Access Token that has `read:packages` scope to authenticate with GitHub Container Registry.

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
| `customDomain` | Custom domain name for the Container App | (empty - uses default domain) |

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
