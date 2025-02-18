Vagrant.configure("2") do |config|
  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/id_rsa'
  config.vm.synced_folder ".", "/vagrant", type: "rsync"
  
  config.vm.define "webserver", primary: true do |server|
      server.vm.provider :digital_ocean do |provider|
        provider.ssh_key_name = ENV["DIGITAL_OCEAN_SSH_KEY"]
        provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
        provider.image = 'ubuntu-22-04-x64'
        provider.region = 'fra1'
        provider.size = 's-1vcpu-1gb'
    end
  
    server.vm.hostname = "webserver"
    server.vm.provision "shell", privileged: false, inline: <<-SHELL
      echo "Updating package list and installing dependencies..."
      sudo apt-get update
      sudo apt-get install -y wget apt-transport-https software-properties-common
      
      # Add Microsoft's package repository for .NET SDK if it's missing
      echo "Adding Microsoft's package repository for .NET SDK..."
      wget https://packages.microsoft.com/config/ubuntu/22.04/prod.list -O /etc/apt/sources.list.d/microsoft-prod.list
      
      # Download and add the missing GPG key for Microsoft's repository
      echo "Adding the Microsoft GPG key..."
      sudo wget https://packages.microsoft.com/keys/microsoft.asc -O /etc/apt/trusted.gpg.d/microsoft.asc
      
      # Update package list again after adding the key
      echo "Updating package list again after adding GPG key..."
      sudo apt-get update
      
      # Install .NET SDK (optional step, just in case it's not installed)
      echo "Installing .NET SDK..."
      sudo apt-get install -y dotnet-sdk-9.0
      
      # Check if the .NET SDK is installed
      dotnet --version
      
      # Assuming you have already placed your code inside /vagrant
      echo "Copying project files to the server..."
      cp -r /vagrant/* $HOME
      
      # Navigate to the project directory HERE
      cd /root/src/minitwit.web
      
      # Restore and build the project (if needed)
      echo "Restoring and building the .NET project..."
      dotnet restore
      dotnet build
      
      # Publish the .NET application
      echo "Publishing the .NET application..."
      dotnet publish -c Release -o /home/vagrant/published
      
      # Start the application using a suitable command (assuming it's a web app)
      echo "Starting the application..."
      cd /home/vagrant/published/
      dotnet minitwit.web.dll &
      
      echo "Provisioning complete. Navigate to http://<your_vagrant_ip>:5000 in your browser."
    SHELL
  end
end