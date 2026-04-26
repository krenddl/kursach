FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DigiClinicApi/DigiClinicApi/DigiClinicApi.csproj DigiClinicApi/DigiClinicApi/
RUN dotnet restore DigiClinicApi/DigiClinicApi/DigiClinicApi.csproj

COPY DigiClinicApi/ DigiClinicApi/
WORKDIR /src/DigiClinicApi/DigiClinicApi
RUN dotnet publish DigiClinicApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DigiClinicApi.dll"]
