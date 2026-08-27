# Terraform conversion for Azure/NewVersion3

This folder is a Terraform equivalent of the Bicep environment in `Azure/NewVersion3`.

## What is provisioned

- Azure Container Registry (Basic, admin enabled)
- Storage Account (StorageV2, Standard_LRS)
- Log Analytics Workspace
- Application Insights (workspace-based)
- Container Apps Environment with workload profiles:
  - Consumption
  - D4 (min: 0, max: 3)
- Container Apps:
  - scaler
  - silo
  - dashboard
  - minimalapiclient
  - workerserviceclient

## Usage

1. Initialize Terraform:

   ```powershell
   terraform init
   ```

2. Create variables file from example and adjust values:

   ```powershell
   copy terraform.tfvars.example terraform.tfvars
   ```

3. Plan and apply:

   ```powershell
   terraform plan
   terraform apply
   ```

## Notes

- `resource_group_name` must be an existing resource group.
- Resource naming uses deterministic hash from resource group ID to emulate Bicep `uniqueString(...)` behavior.
- Container app image defaults to `mcr.microsoft.com/azuredocs/containerapps-helloworld:latest`, matching the Bicep defaults.
