#! /bin/bash
# exit script when any command ran here returns with non-zero exit code
set -e
echo "$DOCKERHUB_PASS" | docker login -u "$DOCKERHUB_USERNAME" --password-stdin