source ~/.bash_profile

echo "Starting deployment..."

docker compose -f docker-compose.yml pull
docker compose -f docker-compose.yml up -d

echo "Deployment completed! :D"