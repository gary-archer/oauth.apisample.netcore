#
# Build the docker image when invoked from the deployment script
#

# Use the .Net Core docker image for the lightweight Alpine Linux OS
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine3.12

# Set the API folder
WORKDIR /usr/sampleapi

# Copy libraries and other files into our docker image
COPY bin/Release/netcoreapp3.1/linux-musl-x64/publish/*.dll         /usr/sampleapi/
COPY bin/Release/netcoreapp3.1/linux-musl-x64/publish/*.so          /usr/sampleapi/
COPY bin/Release/netcoreapp3.1/linux-musl-x64/publish/*.a           /usr/sampleapi/
COPY deployment/api.config.json                                     /usr/sampleapi/
COPY certs/mycompany.ssl.pfx                                        /usr/sampleapi/certs/
COPY data/*                                                         /usr/sampleapi/data/

# When the container is run, this will execute the .Net Core program
CMD ["dotnet", "sampleapi.dll"]

# Expose the service over port 80 for demo purposes
EXPOSE 80/tcp

# This can be used to view the image's file structure before deploying it
# docker run -it netcoreapi sh