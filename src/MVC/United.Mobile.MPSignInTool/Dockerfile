#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM artifactorycloud.ual.com/l-docker/datadog/dotnet/runtime:3.1-1.28.7 AS base

WORKDIR /app
EXPOSE 80

FROM artifactorycloud.ual.com/l-docker/ms/dotnet/core/sdk:3.1 AS build
WORKDIR /

COPY ["src/MVC/United.Mobile.MPSignInTool/United.Mobile.MPSignInTool.csproj", "src/MVC/United.Mobile.MPSignInTool/"]

ARG NUGET_PASSWORD
ARG NUGET_USERNAME

RUN dotnet nuget add source https://artifactorycloud.ual.com/artifactory/api/nuget/v3/v-nuget \
            -n artifactory \
            -u ${NUGET_USERNAME}  \
            -p ${NUGET_PASSWORD} \
            --store-password-in-clear-text

COPY . .
WORKDIR "/src/MVC/United.Mobile.MPSignInTool"

FROM build AS publish
RUN dotnet publish "United.Mobile.MPSignInTool.csproj" -c Release --self-contained -r linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "United.Mobile.MPSignInTool.dll"]