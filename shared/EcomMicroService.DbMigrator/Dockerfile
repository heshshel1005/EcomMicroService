# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/identity/NuGet.Config", "services/identity/"]
COPY ["services/projects/NuGet.Config", "services/projects/"]
COPY ["services/saas/NuGet.Config", "services/saas/"]
COPY ["shared/EcomMicroService.DbMigrator/EcomMicroService.DbMigrator.csproj", "shared/EcomMicroService.DbMigrator/"]
COPY ["services/administration/src/EcomMicroService.Administration.Application.Contracts/EcomMicroService.Administration.Application.Contracts.csproj", "services/administration/src/EcomMicroService.Administration.Application.Contracts/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain.Shared/EcomMicroService.Administration.Domain.Shared.csproj", "services/administration/src/EcomMicroService.Administration.Domain.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/EcomMicroService.Administration.EntityFrameworkCore.csproj", "services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/"]
COPY ["shared/EcomMicroService.Shared/EcomMicroService.Shared.csproj", "shared/EcomMicroService.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain/EcomMicroService.Administration.Domain.csproj", "services/administration/src/EcomMicroService.Administration.Domain/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.Application.Contracts/EcomMicroService.IdentityService.Application.Contracts.csproj", "services/identity/src/EcomMicroService.IdentityService.Application.Contracts/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.Domain.Shared/EcomMicroService.IdentityService.Domain.Shared.csproj", "services/identity/src/EcomMicroService.IdentityService.Domain.Shared/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.EntityFrameworkCore/EcomMicroService.IdentityService.EntityFrameworkCore.csproj", "services/identity/src/EcomMicroService.IdentityService.EntityFrameworkCore/"]
COPY ["services/identity/src/EcomMicroService.IdentityService.Domain/EcomMicroService.IdentityService.Domain.csproj", "services/identity/src/EcomMicroService.IdentityService.Domain/"]
COPY ["services/projects/src/EcomMicroService.Projects.Application.Contracts/EcomMicroService.Projects.Application.Contracts.csproj", "services/projects/src/EcomMicroService.Projects.Application.Contracts/"]
COPY ["services/projects/src/EcomMicroService.Projects.Domain.Shared/EcomMicroService.Projects.Domain.Shared.csproj", "services/projects/src/EcomMicroService.Projects.Domain.Shared/"]
COPY ["services/projects/src/EcomMicroService.Projects.EntityFrameworkCore/EcomMicroService.Projects.EntityFrameworkCore.csproj", "services/projects/src/EcomMicroService.Projects.EntityFrameworkCore/"]
COPY ["services/projects/src/EcomMicroService.Projects.Domain/EcomMicroService.Projects.Domain.csproj", "services/projects/src/EcomMicroService.Projects.Domain/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Application.Contracts/EcomMicroService.SaaS.Application.Contracts.csproj", "services/saas/src/EcomMicroService.SaaS.Application.Contracts/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Domain.Shared/EcomMicroService.SaaS.Domain.Shared.csproj", "services/saas/src/EcomMicroService.SaaS.Domain.Shared/"]
COPY ["services/saas/src/EcomMicroService.SaaS.EntityFrameworkCore/EcomMicroService.SaaS.EntityFrameworkCore.csproj", "services/saas/src/EcomMicroService.SaaS.EntityFrameworkCore/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Domain/EcomMicroService.SaaS.Domain.csproj", "services/saas/src/EcomMicroService.SaaS.Domain/"]
COPY ["shared/EcomMicroService.Microservice.Shared/EcomMicroService.Microservice.Shared.csproj", "shared/EcomMicroService.Microservice.Shared/"]
COPY ["shared/EcomMicroService.Hosting.Shared/EcomMicroService.Hosting.Shared.csproj", "shared/EcomMicroService.Hosting.Shared/"]
COPY ["shared/EcomMicroService.ServiceDefaults/EcomMicroService.ServiceDefaults.csproj", "shared/EcomMicroService.ServiceDefaults/"]
RUN dotnet restore "./shared/EcomMicroService.DbMigrator/EcomMicroService.DbMigrator.csproj"
COPY . .
WORKDIR "/src/shared/EcomMicroService.DbMigrator"
RUN dotnet build "./EcomMicroService.DbMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EcomMicroService.DbMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcomMicroService.DbMigrator.dll"]