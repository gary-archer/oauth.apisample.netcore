#!/bin/bash

#
# A script to deploy our API docker image to the Kubernetes cluster
#

#
# Use the Minikube Docker Daemon rather than that of Docker Desktop for Mac
#
eval $(minikube docker-env)

#
# Clean up any resources for the previously deployed version of the API
#
echo "Preparing Kubernetes ..."
kubectl delete deploy/netcoreapi
kubectl delete service/netcoreapi-svc

#
# Deploy 2 instances of the local docker image to 2 Kubernetes pods
#
echo "Deploying Docker Image to Kubernetes ..."
kubectl apply -f Kubernetes.yaml
if [ $? -ne 0 ]
then
  echo "*** Kubernetes deployment error ***"
  exit 1
fi

#
# Expose the API to clients outside Kubernetes on port 443 with a custom host name
# We can then access the API at https://netcoreapi.mycompany.com/api/companies
#
kubectl apply -f Ingress.yaml
echo "Deployment completed successfully"
