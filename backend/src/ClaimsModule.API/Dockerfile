FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/ClaimsModule.API/ClaimsModule.API.csproj", "src/ClaimsModule.API/"]
COPY ["src/ClaimsModule.Application/ClaimsModule.Application.csproj", "src/ClaimsModule.Application/"]
COPY ["src/ClaimsModule.Domain/ClaimsModule.Domain.csproj", "src/ClaimsModule.Domain/"]
COPY ["src/ClaimsModule.Infrastructure/ClaimsModule.Infrastructure.csproj", "src/ClaimsModule.Infrastructure/"]
COPY ["src/ClaimsModule.Persistence/ClaimsModule.Persistence.csproj", "src/ClaimsModule.Persistence/"]

RUN dotnet restore "src/ClaimsModule.API/ClaimsModule.API.csproj"

COPY . .
WORKDIR "/src/src/ClaimsModule.API"
RUN dotnet build "ClaimsModule.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClaimsModule.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN mkdir -p /app/uploads

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "ClaimsModule.API.dll"]
