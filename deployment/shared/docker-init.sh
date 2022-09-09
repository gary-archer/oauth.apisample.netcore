#!/bin/sh

############################################################
# A script to initialize infrastructure for the Docker image
############################################################

#
# See if the extra trusted certificates file is non-empty
#
TRUSTED_CA_CERTS='/usr/local/share/certificates/trusted.ca.crt'
if [ -s "$TRUSTED_CA_CERTS" ]; then

  #
  # If so then configure operating system trust
  #
  update-ca-certificates
  if [ $? -ne 0 ]; then
    echo 'Problem encountered updating root certificates'
    exit 1
  fi
fi
