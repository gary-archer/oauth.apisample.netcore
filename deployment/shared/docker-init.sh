#!/bin/sh

############################################################
# A script to initialize infrastructure for the Docker image
############################################################

#
# Point to the trusted certificate bundle injected into the container
#
TRUSTED_CA_CERTS='/usr/local/share/certificates/trusted.ca.crt'

#
# Configure operating system trust
#
update-ca-certificates
if [ $? -ne 0 ]; then
  echo 'Problem encountered updating root certificates'
  exit 1
fi
