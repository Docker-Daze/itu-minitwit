source ~/.bash_profile
cd remote_files

echo "Starting deployment..."

docker compose -f docker-compose.yml pull
docker compose -f docker-compose.yml up -d

echo "Deployment completed! :D"