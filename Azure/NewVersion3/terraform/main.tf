data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

locals {
  location = var.location != "" ? var.location : data.azurerm_resource_group.rg.location

  base_name    = data.azurerm_resource_group.rg.name
  unique_seed  = substr(sha1(data.azurerm_resource_group.rg.id), 0, 13)
  acr_name     = lower("${local.unique_seed}acr")
  storage_name = lower("${local.unique_seed}strg")
}

resource "azurerm_container_registry" "acr" {
  name                = local.acr_name
  location            = local.location
  resource_group_name = data.azurerm_resource_group.rg.name
  sku                 = "Basic"
  admin_enabled       = true
}

resource "azurerm_storage_account" "storage" {
  name                     = local.storage_name
  resource_group_name      = data.azurerm_resource_group.rg.name
  location                 = local.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"
}

resource "azurerm_log_analytics_workspace" "logs" {
  name                = "${local.base_name}logs"
  location            = local.location
  resource_group_name = data.azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "appinsights" {
  name                = "${local.base_name}ai"
  location            = local.location
  resource_group_name = data.azurerm_resource_group.rg.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.logs.id
}

resource "azurerm_container_app_environment" "env" {
  name                       = "${local.base_name}env"
  location                   = local.location
  resource_group_name        = data.azurerm_resource_group.rg.name
  logs_destination           = "log-analytics"
  log_analytics_workspace_id = azurerm_log_analytics_workspace.logs.id

  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
    minimum_count         = 0
    maximum_count         = 0
  }

  workload_profile {
    name                  = "D4"
    workload_profile_type = "D4"
    minimum_count         = 0
    maximum_count         = 3
  }
}

locals {
  shared_env = {
    ASPNETCORE_ENVIRONMENT                  = "Development"
    StorageConnectionString                 = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.storage.name};AccountKey=${azurerm_storage_account.storage.primary_access_key};EndpointSuffix=core.windows.net"
    APPINSIGHTS_INSTRUMENTATIONKEY         = azurerm_application_insights.appinsights.instrumentation_key
    APPLICATIONINSIGHTS_CONNECTION_STRING  = azurerm_application_insights.appinsights.connection_string
  }
}

resource "azurerm_container_app" "scaler" {
  name                         = "scaler"
  resource_group_name          = data.azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.env.id
  revision_mode                = "Single"
  workload_profile_name        = var.workload_profile_name

  template {
    container {
      name   = "scaler"
      image  = var.repository_image
      cpu    = 0.25
      memory = "0.5Gi"

      dynamic "env" {
        for_each = local.shared_env
        content {
          name  = env.key
          value = env.value
        }
      }
    }

    min_replicas = 1
    max_replicas = 1
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "container-registry-password"
  }

  secret {
    name  = "container-registry-password"
    value = azurerm_container_registry.acr.admin_password
  }

  ingress {
    external_enabled           = false
    target_port                = 80
    allow_insecure_connections = true
    transport                  = "http2"
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }
}

resource "azurerm_container_app" "silo" {
  name                         = "silo"
  resource_group_name          = data.azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.env.id
  revision_mode                = "Multiple"
  workload_profile_name        = var.workload_profile_name

  template {
    container {
      name   = "silo"
      image  = var.repository_image
      cpu    = 0.25
      memory = "0.5Gi"

      dynamic "env" {
        for_each = local.shared_env
        content {
          name  = env.key
          value = env.value
        }
      }
    }

    min_replicas = 1
    max_replicas = 10

    custom_scale_rule {
      name             = "scaler"
      custom_rule_type = "external"
      metadata = {
        scalerAddress = "${azurerm_container_app.scaler.latest_revision_fqdn}:80"
        graintype     = "sensortwin"
        siloNameFilter = "silo"
        upperbound    = "300"
      }
    }
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "container-registry-password"
  }

  secret {
    name  = "container-registry-password"
    value = azurerm_container_registry.acr.admin_password
  }
}

resource "azurerm_container_app" "dashboard" {
  name                         = "dashboard"
  resource_group_name          = data.azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.env.id
  revision_mode                = "Single"
  workload_profile_name        = var.workload_profile_name

  template {
    container {
      name   = "dashboard"
      image  = var.repository_image
      cpu    = 0.25
      memory = "0.5Gi"

      dynamic "env" {
        for_each = local.shared_env
        content {
          name  = env.key
          value = env.value
        }
      }
    }

    min_replicas = 1
    max_replicas = 1
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "container-registry-password"
  }

  secret {
    name  = "container-registry-password"
    value = azurerm_container_registry.acr.admin_password
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }
}

resource "azurerm_container_app" "minimalapiclient" {
  name                         = "minimalapiclient"
  resource_group_name          = data.azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.env.id
  revision_mode                = "Single"
  workload_profile_name        = var.workload_profile_name

  template {
    container {
      name   = "minimalapiclient"
      image  = var.repository_image
      cpu    = 0.25
      memory = "0.5Gi"

      dynamic "env" {
        for_each = local.shared_env
        content {
          name  = env.key
          value = env.value
        }
      }
    }

    min_replicas = 1
    max_replicas = 1
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "container-registry-password"
  }

  secret {
    name  = "container-registry-password"
    value = azurerm_container_registry.acr.admin_password
  }

  ingress {
    external_enabled = true
    target_port      = 80
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }
}

resource "azurerm_container_app" "workerserviceclient" {
  name                         = "workerserviceclient"
  resource_group_name          = data.azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.env.id
  revision_mode                = "Multiple"
  workload_profile_name        = var.workload_profile_name

  template {
    container {
      name   = "workerserviceclient"
      image  = var.repository_image
      cpu    = 0.25
      memory = "0.5Gi"

      dynamic "env" {
        for_each = local.shared_env
        content {
          name  = env.key
          value = env.value
        }
      }
    }

    min_replicas = 1
    max_replicas = 1
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "container-registry-password"
  }

  secret {
    name  = "container-registry-password"
    value = azurerm_container_registry.acr.admin_password
  }
}
