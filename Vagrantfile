Vagrant.configure("2") do |config|
  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/do_ssh_key'
  config.vm.synced_folder "remote_files", "/minitwit", type: "rsync"
  config.vm.synced_folder './src', '/minitwit/src', type: "rsync"
  config.vm.synced_folder '.', '/vagrant', disabled: true
  
  config.vm.define "minitwit", primary: true do |server|
      server.vm.provider :digital_ocean do |provider|
        provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
        provider.image = 'ubuntu-22-04-x64'
        provider.region = 'fra1'
        provider.size = 's-1vcpu-1gb'
    end
  
    server.vm.hostname = "minitwit-server"
    
    server.vm.provision "shell", inline: 'echo "export DOCKER_USERNAME=' + "'" + ENV["DOCKER_USERNAME"] + "'" + '" >> ~/.bash_profile'
    server.vm.provision "shell", inline: 'echo "export DOCKER_PASSWORD=' + "'" + ENV["DOCKER_PASSWORD"] + "'" + '" >> ~/.bash_profile'
    
    server.vm.provision "shell", privileged: false, inline: <<-SHELL
      echo "Updating package list and installing dependencies..."
      sudo apt-get update
      sudo apt-get install -y wget apt-transport-https software-properties-common
      
      # Install docker and docker compose
      sudo apt-get install -y docker.io docker-compose-v2
      
      echo -e "\nVerifying that docker works ...\n"
      docker run --rm hello-world
      docker rmi hello-world
      
      # Add Microsoft's package repository for .NET SDK if it's missing
      echo "Adding Microsoft's package repository for .NET SDK..."
      wget https://packages.microsoft.com/config/ubuntu/22.04/prod.list -O /etc/apt/sources.list.d/microsoft-prod.list
      
      # Download and add the missing GPG key for Microsoft's repository
      echo "Adding the Microsoft GPG key..."
      sudo wget https://packages.microsoft.com/keys/microsoft.asc -O /etc/apt/trusted.gpg.d/microsoft.asc
      
      # Update package list again after adding the key
      echo "Updating package list again after adding GPG key..."
      sudo apt-get update
      
      # Install .NET SDK
      echo "Installing .NET SDK..."
      sudo apt-get install -y dotnet-sdk-9.0
      
      # Check if the .NET SDK is installed
      dotnet --version
      
      echo ". $HOME/.bashrc" >> $HOME/.bash_profile
      
      echo -e "\nConfiguring credentials as environment variables...\n"
      
      source $HOME/.bash_profile
      
      echo -e "\nSelecting Minitwit Folder as default folder when you ssh into the server...\n"
      echo "cd /minitwit" >> ~/.bash_profile
      
      echo "Executing deploy script"
      chmod +x /minitwit/deploy.sh
   
      # Print host link
      echo "Provisioning complete."
      THIS_IP=`hostname -I | cut -d" " -f1`
      echo "http://${THIS_IP}:5000"
    SHELL
  end
end