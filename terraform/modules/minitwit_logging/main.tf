terraform {
  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.0"
    }
  }
}

resource "digitalocean_droplet" "logging" {
  image      = "docker-20-04"
  name       = var.name
  region     = "fra1"
  size       = "s-2vcpu-4gb"
  monitoring = true
  vpc_uuid   = var.vpc_uuid
  ssh_keys = [var.ssh_key_fingerprint]
}

resource "null_resource" "setup-logging" {
  depends_on = [digitalocean_droplet.logging]

  connection {
    user    = "root"
    host    = digitalocean_droplet.logging.ipv4_address
    type    = "ssh"
    private_key = file("~/.ssh/id_ed25519")
    timeout = "2m"
  }

  # Create minitwit folder at root
  provisioner "remote-exec" {
    inline = [
      "mkdir -p /logging"
    ]
  }

  # Copies remote_files folder to droplet
  provisioner "file" {
    source      = "../logging"
    destination = "/"
  }

  # Copies remote_files folder to droplet
  provisioner "file" {
    source      = "../logstash"
    destination = "/logging"
  }

  provisioner "remote-exec" {
    inline = [
      "echo 'export SERVER_NAME=\"${digitalocean_droplet.logging.ipv4_address}\"' >> ~/.bash_profile",
      "echo '${var.htpasswd}' > /logging/htpasswd",
      "cd /logging",
      "docker compose up --build -d"
    ]
  }
}