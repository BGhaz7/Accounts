FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /accounts
EXPOSE 5031

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Accounts.API/Accounts.API.csproj", "Accounts.API/"]
COPY ["Accounts.Models/Accounts.Models.csproj", "Accounts.Models/"]
COPY ["Accounts.Repository/Accounts.Repository.csproj", "Accounts.Repository/"]
COPY ["Accounts.Service/Accounts.Service.csproj", "Accounts.Service/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Accounts.API/Accounts.API.csproj"
COPY . .
WORKDIR "/src/Accounts.API"
RUN dotnet build "Accounts.API.csproj" -c $BUILD_CONFIGURATION -o /accounts/build

RUN dotnet tool install --global dotnet-ef

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Accounts.API.csproj" -c $BUILD_CONFIGURATION -o /accounts/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /accounts
COPY --from=publish /accounts/publish .
ENTRYPOINT ["dotnet", "Accounts.API.dll"]