#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Server/TcpWorkerService/TcpWorkerService.csproj", "Server/TcpWorkerService/"]
COPY ["Server/EngineControllerDataContext/EngineControllerData.csproj", "Server/EngineControllerDataContext/"]
RUN dotnet restore "Server/TcpWorkerService/TcpWorkerService.csproj"
COPY . .
WORKDIR "/src/Server/TcpWorkerService"
RUN dotnet build "TcpWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TcpWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TcpWorkerService.dll"]