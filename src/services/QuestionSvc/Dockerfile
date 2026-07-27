# build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY QuestionSvc.csproj .
COPY --from=contracts . /shared/Contracts
RUN dotnet restore QuestionSvc.csproj
COPY . .
RUN dotnet publish QuestionSvc.csproj -c Release -o /app --no-restore

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "QuestionSvc.dll"]
