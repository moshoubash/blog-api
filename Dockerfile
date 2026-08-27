# syntax=docker/dockerfile:1

# Stage 1: Build & Publish
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /source

# Copy project files first for better layer caching
COPY DotnetAPI.csproj ./
ARG TARGETARCH
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore -a ${TARGETARCH/amd64/x64}

# Copy the rest of the source code and publish
COPY . ./
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --no-restore --use-current-runtime --self-contained false -o /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app .

USER $APP_UID

ENTRYPOINT ["dotnet", "DotnetAPI.dll"]
