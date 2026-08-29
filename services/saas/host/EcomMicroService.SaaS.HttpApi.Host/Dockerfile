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
COPY ["services/saas/NuGet.Config", "services/saas/"]
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/saas/host/EcomMicroService.SaaS.HttpApi.Host/EcomMicroService.SaaS.HttpApi.Host.csproj", "services/saas/host/EcomMicroService.SaaS.HttpApi.Host/"]
COPY ["shared/EcomMicroService.Microservice.Shared/EcomMicroService.Microservice.Shared.csproj", "shared/EcomMicroService.Microservice.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/EcomMicroService.Administration.EntityFrameworkCore.csproj", "services/administration/src/EcomMicroService.Administration.EntityFrameworkCore/"]
COPY ["shared/EcomMicroService.Shared/EcomMicroService.Shared.csproj", "shared/EcomMicroService.Shared/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain/EcomMicroService.Administration.Domain.csproj", "services/administration/src/EcomMicroService.Administration.Domain/"]
COPY ["services/administration/src/EcomMicroService.Administration.Domain.Shared/EcomMicroService.Administration.Domain.Shared.csproj", "services/administration/src/EcomMicroService.Administration.Domain.Shared/"]
COPY ["shared/EcomMicroService.Hosting.Shared/EcomMicroService.Hosting.Shared.csproj", "shared/EcomMicroService.Hosting.Shared/"]
COPY ["shared/EcomMicroService.ServiceDefaults/EcomMicroService.ServiceDefaults.csproj", "shared/EcomMicroService.ServiceDefaults/"]
COPY ["services/saas/src/EcomMicroService.SaaS.HttpApi/EcomMicroService.SaaS.HttpApi.csproj", "services/saas/src/EcomMicroService.SaaS.HttpApi/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Application.Contracts/EcomMicroService.SaaS.Application.Contracts.csproj", "services/saas/src/EcomMicroService.SaaS.Application.Contracts/"]
COPY ["services/administration/src/EcomMicroService.Administration.Application.Contracts/EcomMicroService.Administration.Application.Contracts.csproj", "services/administration/src/EcomMicroService.Administration.Application.Contracts/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Domain.Shared/EcomMicroService.SaaS.Domain.Shared.csproj", "services/saas/src/EcomMicroService.SaaS.Domain.Shared/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Application/EcomMicroService.SaaS.Application.csproj", "services/saas/src/EcomMicroService.SaaS.Application/"]
COPY ["services/administration/src/EcomMicroService.Administration.Application/EcomMicroService.Administration.Application.csproj", "services/administration/src/EcomMicroService.Administration.Application/"]
COPY ["services/saas/src/EcomMicroService.SaaS.Domain/EcomMicroService.SaaS.Domain.csproj", "services/saas/src/EcomMicroService.SaaS.Domain/"]
COPY ["services/saas/src/EcomMicroService.SaaS.EntityFrameworkCore/EcomMicroService.SaaS.EntityFrameworkCore.csproj", "services/saas/src/EcomMicroService.SaaS.EntityFrameworkCore/"]
RUN dotnet restore "./services/saas/host/EcomMicroService.SaaS.HttpApi.Host/EcomMicroService.SaaS.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/saas/host/EcomMicroService.SaaS.HttpApi.Host"
RUN dotnet build "./EcomMicroService.SaaS.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EcomMicroService.SaaS.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcomMicroService.SaaS.HttpApi.Host.dll"]