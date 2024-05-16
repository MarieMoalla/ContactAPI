# Utiliser une image de base qui contient le SDK .NET 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
EXPOSE 4040
ENV ASPNETCORE_URLS=http://*:4040

# Copy only the project file and restore dependencies
COPY ["ContactsApp.csproj", "ContactsApp/"]
WORKDIR "/src/ContactsApp"
RUN dotnet restore "./ContactsApp.csproj"

# Copy the entire source code
COPY . .

# Build the application
RUN dotnet build "./ContactsApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish Stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ContactsApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Utiliser une image de runtime pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy the published output from the publish stage
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ContactsApp.dll"]