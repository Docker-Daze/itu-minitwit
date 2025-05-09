output "server_ip" {
  value = digitalocean_droplet.server.ipv4_address
}