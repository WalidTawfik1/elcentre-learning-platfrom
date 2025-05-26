FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/ElCentreAPI/ElCentreAPI.csproj", "src/ElCentreAPI/"]
COPY ["src/ElCentre.Core/ElCentre.Core.csproj", "src/ElCentre.Core/"]
COPY ["src/ElCentre.Infrastructure/ElCentre.Infrastructure.csproj", "src/ElCentre.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/ElCentreAPI/ElCentreAPI.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/ElCentreAPI"
RUN dotnet build "ElCentreAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ElCentreAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElCentreAPI.dll"]