FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /usr/api
COPY bin/Release/net8.0/linux-x64/publish/*  /usr/api/
COPY data/*                                  /usr/api/data/

RUN groupadd --gid 10000 apiuser \
  && useradd --uid 10001 --gid apiuser --shell /bin/bash --create-home apiuser
USER 10001
CMD ["dotnet", "finalapi.dll"]
