source ~/.bash_profile

echo "Starting deployment..."
cd /minitwit

if [ -z "$ConnectionStrings__DefaultConnection" ]; then
  echo "Error: ConnectionStrings__DefaultConnection is not set"
  exit 1
fi

docker compose -f docker-compose.yml pull
docker compose -f docker-compose.yml up -d

echo "Deployment completed!"