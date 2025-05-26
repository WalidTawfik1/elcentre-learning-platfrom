FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the necessary project files
COPY ["src/ElCentreAPI/ElCentreAPI.csproj", "src/ElCentreAPI/"]
COPY ["src/ElCentre.Core/ElCentre.Core.csproj", "src/ElCentre.Core/"]
COPY ["src/ElCentre.Infrastructure/ElCentre.Infrastructure.csproj", "src/ElCentre.Infrastructure/"]

# Set the working directory to ElCentreAPI for restore
WORKDIR /src/src/ElCentreAPI
RUN dotnet restore "ElCentreAPI.csproj"

# Copy the rest of the source code
WORKDIR /src
COPY . .

# Build
WORKDIR /src/src/ElCentreAPI
RUN dotnet build "ElCentreAPI.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR /src/src/ElCentreAPI
RUN dotnet publish "ElCentreAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElCentreAPI.dll"]
