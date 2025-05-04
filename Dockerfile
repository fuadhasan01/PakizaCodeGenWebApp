# Use the official image for ASP.NET Core runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PakizaCodeGenWebApp/PakizaCodeGenWebApp.csproj", "PakizaCodeGenWebApp/"]
RUN dotnet restore "PakizaCodeGenWebApp/PakizaCodeGenWebApp.csproj"
COPY . .
WORKDIR "/src/PakizaCodeGenWebApp"
RUN dotnet build "PakizaCodeGenWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PakizaCodeGenWebApp.csproj" -c Release -o /app/publish

# Final stage to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PakizaCodeGenWebApp.dll"]
