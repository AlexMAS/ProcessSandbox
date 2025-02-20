FROM mcr.microsoft.com/dotnet/sdk:8.0.406-bookworm-slim AS build

WORKDIR /src

COPY Sandbox/sandbox-exec Sandbox/sandbox-exec

# Restore packages
COPY ProcessSandbox/ProcessSandbox.csproj ProcessSandbox/
COPY ProcessSandbox.App/ProcessSandbox.App.csproj ProcessSandbox.App/
RUN dotnet restore ProcessSandbox/ProcessSandbox.csproj
RUN dotnet restore ProcessSandbox.App/ProcessSandbox.App.csproj

# Copy source files
COPY ProcessSandbox/. ProcessSandbox/
COPY ProcessSandbox.App/. ProcessSandbox.App/

# Build projects
RUN dotnet pack ProcessSandbox/ProcessSandbox.csproj --no-restore -c Release -o /bin/ProcessSandbox/
RUN dotnet publish ProcessSandbox.App/ProcessSandbox.App.csproj --no-restore -c Release -o /bin/ProcessSandbox.App/ -p:GenerateDocumentationFile=false
