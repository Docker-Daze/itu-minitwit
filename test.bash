#!/bin/bash

# === CONFIGURE THESE ===
SSH_KEY_PATH=~/.ssh/Devops            # Path to your private SSH key
SSH_USER=root                     # Replace with your SSH username
SSH_HOST=174.138.101.57        # Replace with your host (IP or FQDN)
LOCAL_FOLDER_PATH=remote_files  # Replace with the path to the folder you want to send
REMOTE_FOLDER_PATH=minitwit # Replace with the destination path on the remote server

# === DO NOT TOUCH BELOW THIS LINE ===
echo "Connecting to $SSH_USER@$SSH_HOST using key at $SSH_KEY_PATH"

# Transfer the folder to the remote server
echo "Sending folder $LOCAL_FOLDER_PATH to $SSH_USER@$SSH_HOST:$REMOTE_FOLDER_PATH"
scp -i "$SSH_KEY_PATH" -r "$LOCAL_FOLDER_PATH" "$SSH_USER@$SSH_HOST:$REMOTE_FOLDER_PATH"

# Connect to the remote server
ssh -i "$SSH_KEY_PATH" -o StrictHostKeyChecking=no "$SSH_USER@$SSH_HOST"