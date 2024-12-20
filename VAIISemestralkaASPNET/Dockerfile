# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5041


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VAIISemestralkaASPNET/VAIISemestralkaASPNET.csproj", "VAIISemestralkaASPNET/"]
RUN dotnet restore "./VAIISemestralkaASPNET/VAIISemestralkaASPNET.csproj"
COPY . .
WORKDIR "/src/VAIISemestralkaASPNET"
RUN dotnet build "./VAIISemestralkaASPNET.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VAIISemestralkaASPNET.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
USER root
COPY --from=publish /app/publish . 
COPY ["VAIISemestralkaASPNET/app.db", "./"]
RUN chmod 666 /app/app.db && chmod -R 777 /app
USER app
ENTRYPOINT ["dotnet", "VAIISemestralkaASPNET.dll"]