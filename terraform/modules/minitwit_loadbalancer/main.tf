terraform {
  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.0"
    }
  }
}

resource "digitalocean_droplet" "loadbalancer" {
  image      = "docker-20-04"
  name       = var.name
  region     = "fra1"
  size       = "s-1vcpu-2gb"
  monitoring = true
  vpc_uuid   = var.vpc_uuid
  ssh_keys = [var.ssh_key_fingerprint]

  connection {
    user    = "root"
    host    = digitalocean_droplet.loadbalancer.ipv4_address
    type    = "ssh"
    private_key = file("~/.ssh/id_ed25519")
    timeout = "2m"
  }

  # Copies nginx_loadbalancer script to droplet
  provisioner "file" {
    content     = templatefile("./files/nginx_loadbalancer.sh.tmpl", {
      PRIMARY_SERVER_IP   = var.primary_server_ip
      SECONDARY_SERVER_IP = var.secondary_server_ip
    })
    destination = "/root/nginx_loadbalancer.sh"
  }


  provisioner "remote-exec" {
    inline = [
      "chmod +x nginx_loadbalancer.sh",
      "./nginx_loadbalancer.sh"
    ]
  }
}