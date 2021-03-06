#! /bin/bash
# exit script when any command ran here returns with non-zero exit code
set -e

COMMIT_SHA1=$GIT_BRANCH-$BUILD_NUMBER

# We must export it so it's available for envsubst
export COMMIT_SHA1=$COMMIT_SHA1

# since the only way for envsubst to work on files is using input/output redirection,
#  it's not possible to do in-place substitution, so we need to save the output to another file
#  and overwrite the original with that one.
envsubst <./kube/DisPro.Accounts-deployment.yml >./kube/DisPro.Accounts-deployment.yml.out
mv ./kube/DisPro.Accounts-deployment.yml.out ./kube/DisPro.Accounts-deployment.yml

echo "$KUBERNETES_CLUSTER_CERTIFICATE" | base64 --decode > cert.crt

kubectl \
  --kubeconfig=/dev/null \
  --server=$KUBERNETES_SERVER \
  --certificate-authority=cert.crt \
  --token=$KUBERNETES_TOKEN \
  apply -f ./kube/