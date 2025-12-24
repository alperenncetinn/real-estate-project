# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY RealEstate/RealEstate.Api/RealEstate.Api.csproj RealEstate/RealEstate.Api/
RUN dotnet restore RealEstate/RealEstate.Api/RealEstate.Api.csproj

# Copy source
COPY RealEstate/RealEstate.Api/ RealEstate/RealEstate.Api/

# Publish
RUN dotnet publish RealEstate/RealEstate.Api/RealEstate.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy uploads if any
COPY RealEstate/RealEstate.Api/uploads /app/uploads

# Configure port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Set environment variables via docker-compose or runtime
# ENV ConnectionStrings__DefaultConnection=...
# ENV JwtSettings__SecretKey=...

ENTRYPOINT ["dotnet", "RealEstate.Api.dll"]
