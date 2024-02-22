#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM artifactorycloud.ual.com/l-docker/datadog/dotnet/runtime:3.1-1.28.7 AS base

WORKDIR /app
EXPOSE 80

FROM artifactorycloud.ual.com/l-docker/ms/dotnet/core/sdk:3.1 AS build
WORKDIR /

COPY ["src/Services/DeviceInitialization/United.Mobile.DeviceInitialization.Api/United.Mobile.Services.DeviceInitialization.Api.csproj", "src/Services/DeviceInitialization/United.Mobile.DeviceInitialization.Api/"]
COPY ["src/Core/United.Utility.Middleware/United.Utility.Middleware.csproj", "src/Core/United.Utility.Middleware/"]
COPY ["src/Core/United.Utility/United.Utility.csproj", "src/Core/United.Utility/"]
COPY ["src/Core/United.Ebs.Logging/United.Ebs.Logging.csproj", "src/Core/United.Ebs.Logging/"]
COPY ["src/Model/United.Mobile.Model.Common/United.Mobile.Model.Common.csproj", "src/Model/United.Mobile.Model.Common/"]
COPY ["src/Model.Internal/United.Mobile.Model.Internal.Exception/United.Mobile.Model.Internal.Exception.csproj", "src/Model.Internal/United.Mobile.Model.Internal.Exception/"]
COPY ["src/Services/DeviceInitialization/United.Mobile.DeviceInitialization.Domain/United.Mobile.Services.DeviceInitialization.Domain.csproj", "src/Services/DeviceInitialization/United.Mobile.DeviceInitialization.Domain/"]
COPY ["src/DataAccess/United.Mobile.DataAccess.OnPremiseSQLSP/United.Mobile.DataAccess.OnPremiseSQLSP.csproj", "src/DataAccess/United.Mobile.DataAccess.OnPremiseSQLSP/"]
COPY ["src/Model.Internal/United.Definition/United.Mobile.Model.Internal.Common.csproj", "src/Model.Internal/United.Definition/"]
COPY ["src/DataAccess/United.Mobile.DataAccess.DynamoDB/United.Mobile.DataAccess.DynamoDB.csproj", "src/DataAccess/United.Mobile.DataAccess.DynamoDB/"]
COPY ["src/Model/United.Mobile.Model.MPSignIn/United.Mobile.Model.MPSignIn.csproj", "src/Model/United.Mobile.Model.MPSignIn/"]
COPY ["src/DataAccess/United.Mobile.DataAccess.Common/United.Mobile.DataAccess.Common.csproj", "src/DataAccess/United.Mobile.DataAccess.Common/"]

ARG NUGET_PASSWORD
ARG NUGET_USERNAME


RUN dotnet nuget add source https://artifactorycloud.ual.com/artifactory/api/nuget/v3/v-nuget \
            -n artifactory \
            -u ${NUGET_USERNAME}  \
            -p ${NUGET_PASSWORD} \
            --store-password-in-clear-text

COPY . .
WORKDIR "/src/Services/DeviceInitialization/United.Mobile.DeviceInitialization.Api"

FROM build AS publish
RUN dotnet publish "United.Mobile.Services.DeviceInitialization.Api.csproj" -c Release --self-contained -r linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "United.Mobile.Services.DeviceInitialization.Api.dll"]