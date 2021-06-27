#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 3050
VOLUME /dbs

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Server/EngineController/EngineController.csproj", "Server/EngineController/"]
COPY ["Server/TcpWorkerService/TcpWorkerService.csproj", "Server/TcpWorkerService/"]
COPY ["Server/EngineControllerDataContext/EngineControllerData.csproj", "Server/EngineControllerDataContext/"]
RUN dotnet restore "Server/EngineController/EngineController.csproj"
COPY . .
WORKDIR "/src/Server/EngineController"
RUN dotnet build "EngineController.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EngineController.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EngineController.dll"]