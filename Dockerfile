FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY back_end/light_brary/light_brary.csproj back_end/light_brary/
RUN dotnet restore back_end/light_brary/light_brary.csproj

COPY back_end/light_brary/ back_end/light_brary/
RUN dotnet publish back_end/light_brary/light_brary.csproj \
    --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "light_brary.dll"]
