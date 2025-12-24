# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY RealEstate/RealEstate.Web/RealEstate.Web.csproj RealEstate/RealEstate.Web/
RUN dotnet restore RealEstate/RealEstate.Web/RealEstate.Web.csproj

# Copy source
COPY RealEstate/RealEstate.Web/ RealEstate/RealEstate.Web/

# Publish
RUN dotnet publish RealEstate/RealEstate.Web/RealEstate.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Configure port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Optionally set API base URL via environment
# ENV ApiBaseUrl=http://api:8080

ENTRYPOINT ["dotnet", "RealEstate.Web.dll"]
