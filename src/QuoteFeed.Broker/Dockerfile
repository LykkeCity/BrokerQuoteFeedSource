FROM microsoft/dotnet:latest
ARG SOURCEDIR
WORKDIR /brocker
COPY ${SOURCEDIR} .
ENTRYPOINT ["dotnet", "QuoteFeed.Broker.dll"]
