FROM mcr.microsoft.com/dotnet/sdk:8.0.406-bookworm-slim AS build

WORKDIR /src

COPY ./Example.csproj ./
RUN dotnet restore ./Example.csproj

COPY ./ ./
RUN dotnet publish ./Example.csproj --no-restore -c Release -o /out

WORKDIR /out
