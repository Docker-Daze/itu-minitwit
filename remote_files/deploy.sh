source ~/.bash_profile

echo "Starting deployment..."
cd remote_files

docker compose -f docker-compose.yml pull
docker compose -f docker-compose.yml up -d

echo "Deployment completed! :D"