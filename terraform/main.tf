terraform {
  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.0"
    }
  }
}

# Configure the DigitalOcean Provider
provider "digitalocean" {
}

resource "digitalocean_ssh_key" "main" {
  name       = "minitwit-server-tf-ssh-key"
  public_key = file("~/.ssh/tf_do_ssh_key.pub")
}


module "minitwit_logging" {
  source              = "./modules/minitwit_logging"
  name                = "minitwit-logging-tf"
  vpc_uuid            = var.vpc_uuid
  htpasswd            = var.htpasswd
  ssh_key_fingerprint = digitalocean_ssh_key.main.fingerprint
}

module "minitwit_primary" {
  source               = "./modules/minitwit_server"
  name                 = "minitwit-primary-tf"
  vpc_uuid             = var.vpc_uuid
  db_connection_string = var.db_connection_string
  docker_username      = var.docker_username
  db_cluster_uuid      = var.db_cluster_uuid
  ssh_key_fingerprint  = digitalocean_ssh_key.main.fingerprint
  logging_server_ip    = module.minitwit_logging.logging_server_ip

  depends_on = [module.minitwit_logging]
}

module "minitwit_secondary" {
  source               = "./modules/minitwit_server"
  name                 = "minitwit-secondary-tf"
  vpc_uuid             = var.vpc_uuid
  db_connection_string = var.db_connection_string
  docker_username      = var.docker_username
  db_cluster_uuid      = var.db_cluster_uuid
  ssh_key_fingerprint  = digitalocean_ssh_key.main.fingerprint
  logging_server_ip    = module.minitwit_logging.logging_server_ip

  depends_on = [module.minitwit_logging]
}

module "minitwit_loadbalancer" {
  source              = "./modules/minitwit_loadbalancer"
  name                = "minitwit-loadbalancer-tf"
  vpc_uuid            = var.vpc_uuid
  ssh_key_fingerprint = digitalocean_ssh_key.main.fingerprint
  primary_server_ip   = module.minitwit_primary.server_ip
  secondary_server_ip = module.minitwit_secondary.server_ip

  depends_on = [module.minitwit_primary, module.minitwit_secondary]
}
