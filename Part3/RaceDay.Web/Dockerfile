# Build stage: restore and publish the app with the .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file first so the restore step is cached
COPY RaceDay.Web.csproj .
RUN dotnet restore

# Copy the rest of the code and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Run stage: only the small ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The app listens on port 8080 inside the container
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# The API address is a setting, the default is the host machine
ENV Api__BaseUrl=http://host.docker.internal:5100

# Do not run as root
USER app

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
