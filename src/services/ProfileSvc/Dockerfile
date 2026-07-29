# build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ProfileSvc.csproj .
RUN dotnet restore ProfileSvc.csproj
COPY . .
RUN dotnet publish ProfileSvc.csproj -c Release -o /app --no-restore

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ProfileSvc.dll"]
