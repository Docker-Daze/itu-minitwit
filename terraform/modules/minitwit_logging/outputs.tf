output "logging_server_ip" {
  value = digitalocean_droplet.logging.ipv4_address
}