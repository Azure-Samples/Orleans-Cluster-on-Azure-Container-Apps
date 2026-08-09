variable "resource_group_name" {
  description = "Existing resource group name where resources will be deployed."
  type        = string
}

variable "location" {
  description = "Azure location for all resources. If empty, resource group location is used."
  type        = string
  default     = ""
}

variable "repository_image" {
  description = "Container image used by all container apps."
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "workload_profile_name" {
  description = "Default workload profile name for container apps."
  type        = string
  default     = "D4"
}
