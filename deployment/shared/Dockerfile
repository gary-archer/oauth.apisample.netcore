# Use the .NET Debian linux docker image
# https://hub.docker.com/_/microsoft-dotnet-aspnet
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy libraries and other files into our docker image
WORKDIR /usr/api
COPY bin/Release/net8.0/linux-x64/publish/*  /usr/api/
COPY data/*                                  /usr/api/data/

# Run the app as a low privilege user
RUN groupadd --gid 10000 apiuser \
  && useradd --uid 10001 --gid apiuser --shell /bin/bash --create-home apiuser
USER 10001
CMD ["dotnet", "sampleapi.dll"]
