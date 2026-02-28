# build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY api/src/CRM.Backend.Api/ src/CRM.Backend.Api/
COPY api/src/CRM.Backend.Application/ src/CRM.Backend.Application/
COPY api/src/CRM.Backend.Domain/ src/CRM.Backend.Domain/
COPY api/src/CRM.Backend.Infra/ src/CRM.Backend.Infra/
RUN dotnet restore src/CRM.Backend.Api/CRM.Backend.Api.csproj
RUN dotnet publish src/CRM.Backend.Api/CRM.Backend.Api.csproj -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 7148
ENTRYPOINT ["dotnet", "CRM.Backend.Api.dll"]