#! /bin/bash
# exit script when any command ran here returns with non-zero exit code
set -e
echo "$DOCKER_CREDS_PSW" | docker login -u "$DOCKER_CREDS_USR" --password-stdin