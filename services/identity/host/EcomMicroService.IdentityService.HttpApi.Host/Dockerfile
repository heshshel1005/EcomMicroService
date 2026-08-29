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
COPY ["services/identity/NuGet.Config", "services/identity/"]
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/saas/NuGet.Config", "services/saas/"]
COPY ["services/identity/host/EcomMicroService.IdentityService.HttpApi.Host/EcomMicroService.IdentityService.HttpApi.Host.csproj", "services/identity/host/EcomMicroService.IdentityService.HttpApi.Host/"]
COPY ["shared/EcomMicroService.Microservice.Shared/EcomMicroService.Microservice.Shared.csproj", "shared/EcomMicroService.Microservice.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/EcomMicroService.Administration.EntityFrameworkCore.csproj", "services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/"]
COPY ["shared/EcomMicroService.Shared/EcomMicroService.Shared.csproj", "shared/EcomMicroService.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain/EcomMicroService.Administration.Domain.csproj", "services/administration/src/EcomMicroService.Administration.Domain/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain.Shared/EcomMicroService.Administration.Domain.Shared.csproj", "services/administration/src/EcomMicroService.Administration.Domain.Shared/"]
COPY ["shared/EcomMicroService.Hosting.Shared/EcomMicroService.Hosting.Shared.csproj", "shared/EcomMicroService.Hosting.Shared/"]
COPY ["shared/EcomMicroService.ServiceDefaults/EcomMicroService.ServiceDefaults.csproj", "shared/EcomMicroService.ServiceDefaults/"]
COPY ["services/saas/src/EcomMicroService.SaaS.EntityFrameworkCore/EcomMicroService.SaaS.EntityFrameworkCore.csproj", "services/saas/src/EcomMicroService.SaaS.EntityFrameworkCore/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Domain/EcomMicroService.SaaS.Domain.csproj", "services/saas/src/EcomMicroService.SaaS.Domain/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Domain.Shared/EcomMicroService.SaaS.Domain.Shared.csproj", "services/saas/src/EcomMicroService.SaaS.Domain.Shared/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.HttpApi/EcomMicroService.IdentityService.HttpApi.csproj", "services/identity/src/EcomMicroService.IdentityService.HttpApi/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.Application.Contracts/EcomMicroService.IdentityService.Application.Contracts.csproj", "services/identity/src/EcomMicroService.IdentityService.Application.Contracts/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.Domain.Shared/EcomMicroService.IdentityService.Domain.Shared.csproj", "services/identity/src/EcomMicroService.IdentityService.Domain.Shared/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.Application/EcomMicroService.IdentityService.Application.csproj", "services/identity/src/EcomMicroService.IdentityService.Application/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.Domain/EcomMicroService.IdentityService.Domain.csproj", "services/identity/src/EcomMicroService.IdentityService.Domain/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.EntityFrameworkCore/EcomMicroService.IdentityService.EntityFrameworkCore.csproj", "services/identity/src/EcomMicroService.IdentityService.EntityFrameworkCore/"]
RUN dotnet restore "./services/identity/host/EcomMicroService.IdentityService.HttpApi.Host/EcomMicroService.IdentityService.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/identity/host/EcomMicroService.IdentityService.HttpApi.Host"
RUN dotnet build "./EcomMicroService.IdentityService.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EcomMicroService.IdentityService.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcomMicroService.IdentityService.HttpApi.Host.dll"]