# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/projects/NuGet.Config", "services/projects/"]
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/projects/host/EcomMicroService.Projects.HttpApi.Host/EcomMicroService.Projects.HttpApi.Host.csproj", "services/projects/host/EcomMicroService.Projects.HttpApi.Host/"]
COPY ["shared/EcomMicroService.Microservice.Shared/EcomMicroService.Microservice.Shared.csproj", "shared/EcomMicroService.Microservice.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/EcomMicroService.Administration.EntityFrameworkCore.csproj", "services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/"]
COPY ["shared/EcomMicroService.Shared/EcomMicroService.Shared.csproj", "shared/EcomMicroService.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain/EcomMicroService.Administration.Domain.csproj", "services/administration/src/EcomMicroService.Administration.Domain/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain.Shared/EcomMicroService.Administration.Domain.Shared.csproj", "services/administration/src/EcomMicroService.Administration.Domain.Shared/"]
COPY ["shared/EcomMicroService.Hosting.Shared/EcomMicroService.Hosting.Shared.csproj", "shared/EcomMicroService.Hosting.Shared/"]
COPY ["shared/EcomMicroService.ServiceDefaults/EcomMicroService.ServiceDefaults.csproj", "shared/EcomMicroService.ServiceDefaults/"]
COPY ["services/projects/src/EcomMicroService.Projects.HttpApi/EcomMicroService.Projects.HttpApi.csproj", "services/projects/src/EcomMicroService.Projects.HttpApi/"]
COPY ["services/projects/src/EcomMicroService.Projects.Application.Contracts/EcomMicroService.Projects.Application.Contracts.csproj", "services/projects/src/EcomMicroService.Projects.Application.Contracts/"]
COPY ["services/projects/src/EcomMicroService.Projects.Domain.Shared/EcomMicroService.Projects.Domain.Shared.csproj", "services/projects/src/EcomMicroService.Projects.Domain.Shared/"]
COPY ["services/projects/src/EcomMicroService.Projects.Application/EcomMicroService.Projects.Application.csproj", "services/projects/src/EcomMicroService.Projects.Application/"]
COPY ["services/projects/src/EcomMicroService.Projects.Domain/EcomMicroService.Projects.Domain.csproj", "services/projects/src/EcomMicroService.Projects.Domain/"]
COPY ["services/projects/src/EcomMicroService.Projects.EntityFrameworkCore/EcomMicroService.Projects.EntityFrameworkCore.csproj", "services/projects/src/EcomMicroService.Projects.EntityFrameworkCore/"]
RUN dotnet restore "./services/projects/host/EcomMicroService.Projects.HttpApi.Host/EcomMicroService.Projects.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/projects/host/EcomMicroService.Projects.HttpApi.Host"
RUN dotnet build "./EcomMicroService.Projects.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EcomMicroService.Projects.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcomMicroService.Projects.HttpApi.Host.dll"]