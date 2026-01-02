FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/BlogPlatform.Api/BlogPlatform.Api.csproj", "src/BlogPlatform.Api/"]
COPY ["src/BlogPlatform.Application/BlogPlatform.Application.csproj", "src/BlogPlatform.Application/"]
COPY ["src/BlogPlatform.Infrastructure/BlogPlatform.Infrastructure.csproj", "src/BlogPlatform.Infrastructure/"]
COPY ["src/BlogPlatform.Domain/BlogPlatform.Domain.csproj", "src/BlogPlatform.Domain/"]
RUN dotnet restore "src/BlogPlatform.Api/BlogPlatform.Api.csproj"
COPY . .
WORKDIR "/src/src/BlogPlatform.Api"
RUN dotnet build "BlogPlatform.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogPlatform.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogPlatform.Api.dll"]

