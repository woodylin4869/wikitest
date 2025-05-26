#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM royalgame/aspnet60-runtime-with-gcp-logging-library:1.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["H1_ThirdPartyWalletAPI/H1_ThirdPartyWalletAPI.csproj", "H1_ThirdPartyWalletAPI/"]
COPY ["ThirdPartyWallet.Common/ThirdPartyWallet.Common.csproj", "ThirdPartyWallet.Common/"]
COPY ["ThirdPartyWallet.GameAPI/ThirdPartyWallet.GameAPI.csproj", "ThirdPartyWallet.GameAPI/"]
COPY ["ThirdPartyWallet.Share/ThirdPartyWallet.Share.csproj", "ThirdPartyWallet.Share/"]
RUN dotnet restore "H1_ThirdPartyWalletAPI/H1_ThirdPartyWalletAPI.csproj" -s https://api.nuget.org/v3/index.json -s http://nuget.royal-test.com/api/v3/index.json

COPY . .
WORKDIR "/src/H1_ThirdPartyWalletAPI"
RUN dotnet build "H1_ThirdPartyWalletAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "H1_ThirdPartyWalletAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "H1_ThirdPartyWalletAPI.dll"]