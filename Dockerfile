FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["DentalClinic.Appointments.Domain/DentalClinic.Appointments.Domain.csproj", "DentalClinic.Appointments.Domain/"]
COPY ["DentalClinic.Appointments.Application/DentalClinic.Appointments.Application.csproj", "DentalClinic.Appointments.Application/"]
COPY ["DentalClinic.Appointments.Infrastructure/DentalClinic.Appointments.Infrastructure.csproj", "DentalClinic.Appointments.Infrastructure/"]
COPY ["DentalClinic.Appointments.Infrastructure/DentalClinic.Appointments.Infrastructure.csproj", "DentalClinic.Appointments.Infrastructure/"]
COPY ["DentalClinic.Appointments.ReadModel/DentalClinic.Appointments.ReadModel.csproj", "DentalClinic.Appointments.ReadModel/"]
COPY ["DentalClinic.Appointments.Api/DentalClinic.Appointments.Api.csproj", "DentalClinic.Appointments.Api/"]
COPY ["DentalClinic.Appointments.Api/DentalClinic.Appointments.Api.csproj", "DentalClinic.Appointments.Api/"]

RUN dotnet restore "DentalClinic.Appointments.Api/DentalClinic.Appointments.Api.csproj"

COPY . .
WORKDIR "/src/DentalClinic.Appointments.Api"

RUN dotnet publish "DentalClinic.Appointments.Api.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DentalClinic.Appointments.Api.dll"]