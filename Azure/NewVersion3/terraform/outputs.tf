output "acr_login_server" {
  description = "Azure Container Registry login server."
  value       = azurerm_container_registry.acr.login_server
}

output "container_app_environment_id" {
  description = "Container App Environment resource ID."
  value       = azurerm_container_app_environment.env.id
}

output "dashboard_fqdn" {
  description = "Dashboard public FQDN."
  value       = azurerm_container_app.dashboard.latest_revision_fqdn
}

output "minimalapiclient_fqdn" {
  description = "Minimal API client public FQDN."
  value       = azurerm_container_app.minimalapiclient.latest_revision_fqdn
}

output "scaler_fqdn" {
  description = "Scaler FQDN used by the silo external scaler rule."
  value       = azurerm_container_app.scaler.latest_revision_fqdn
}
