terraform {
  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.0"
    }
  }
}

resource "digitalocean_droplet" "server" {
  image      = "docker-20-04"
  name       = var.name
  region     = "fra1"
  size       = "s-1vcpu-2gb"
  monitoring = true
  vpc_uuid   = var.vpc_uuid
  ssh_keys   = [var.ssh_key_fingerprint]
  
  connection {
    user        = "root"
    host        = digitalocean_droplet.server.ipv4_address
    type        = "ssh"
    private_key = file("~/.ssh/tf_do_ssh_key")
    timeout     = "2m"
  }

  # Create minitwit folder at root
  provisioner "remote-exec" {
    inline = [
      "mkdir -p /minitwit"
    ]
  }

  # Copies remote_files folder to droplet
  provisioner "file" {
    source      = "../remote_files"
    destination = "/minitwit"
  }

  # Copies logstash folder to droplet
  provisioner "file" {
    source      = "../logstash"
    destination = "/minitwit"
  }
}

resource "null_resource" "db_firewall_setup" {
  depends_on = [digitalocean_droplet.server]
  
  provisioner "local-exec" {
    command = "doctl databases firewalls append ${var.db_cluster_uuid} --rule ip_addr:${digitalocean_droplet.server.ipv4_address}"
  }

  connection {
    user        = "root"
    host        = digitalocean_droplet.server.ipv4_address
    type        = "ssh"
    private_key = file("~/.ssh/tf_do_ssh_key")
    timeout     = "2m"
  }

  provisioner "remote-exec" {
    inline = [
      "echo 'export LOGGING_SERVER_IP=\"${var.logging_server_ip}\"' >> ~/.bash_profile",
      "echo 'export DB_CONNECTION_STRING=\"${var.db_connection_string}\"' >> ~/.bash_profile",
      "echo 'export DOCKER_USERNAME=\"${var.docker_username}\"' >> ~/.bash_profile",
      "bash -c 'source ~/.bash_profile && cd /minitwit/remote_files && chmod +x deploy.sh && ./deploy.sh'"
    ]
  }
}
