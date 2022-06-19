#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
RUN apt-get update && apt-get install curl ffmpeg libsodium23 libsodium-dev libopus0 libopus-dev python -y
RUN curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl
RUN chmod a+rx /usr/local/bin/youtube-dl
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ConstantBotApplication/ConstantBotApplication.csproj", "ConstantBotApplication/"]
RUN dotnet restore "ConstantBotApplication/ConstantBotApplication.csproj"
COPY . .
WORKDIR "/src/ConstantBotApplication"
RUN dotnet build "ConstantBotApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConstantBotApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConstantBotApplication.dll"]