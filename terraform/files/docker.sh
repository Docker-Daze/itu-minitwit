curl -fsSL https://get.docker.com -o get-docker.sh
sudo service packagekit restart
sudo sh get-docker.sh
docker --version
docker run hello-world