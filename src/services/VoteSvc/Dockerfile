# build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY VoteSvc.csproj .
COPY --from=contracts . /shared/Contracts
RUN dotnet restore VoteSvc.csproj
COPY . .
RUN dotnet publish VoteSvc.csproj -c Release -o /app --no-restore

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "VoteSvc.dll"]
