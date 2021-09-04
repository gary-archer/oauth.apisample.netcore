#!/bin/bash

#
# Download SSL certificates from a central repo if needed
#
if [ ! -d '.certs' ]; then
    git clone https://github.com/gary-archer/oauth.developmentcertificates ./.certs
fi

#
# Build the app
#
dotnet build

#
# Then start listening
#
dotnet run