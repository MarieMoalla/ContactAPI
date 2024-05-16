# Utiliser une image de base qui contient le SDK .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file and restore dependencies
COPY ["ContactsApp/ContactsApp.csproj", "ContactsApp/"]
RUN dotnet restore "./ContactsApp/ContactsApp.csproj"
COPY . .
WORKDIR "/src/ContactsApp"

# Build the application
RUN dotnet build "./ContactsApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish Stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ContactsApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContactsApp.dll"]