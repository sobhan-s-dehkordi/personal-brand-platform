FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
RUN dotnet restore src/PersonalBrand.Admin/PersonalBrand.Admin.csproj
RUN dotnet publish src/PersonalBrand.Admin/PersonalBrand.Admin.csproj -c Release --no-restore -o /out /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN mkdir -p /data/uploads /data/keys && chown -R $APP_UID:$APP_UID /data
COPY --from=build /out .
USER $APP_UID
EXPOSE 8443
ENTRYPOINT ["dotnet", "PersonalBrand.Admin.dll"]

