# Optional Linux container for EImece.Web (Phase 9).
# Build context: the EImece/ directory (parent of EImece.Web).
#   docker build -f EImece.Web/Dockerfile -t eimece-web .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY EImece.Web/EImece.Web.csproj EImece.Web/
COPY EImece.Domain.Core/EImece.Domain.Core.csproj EImece.Domain.Core/
COPY Resources/Resources.csproj Resources/
RUN dotnet restore EImece.Web/EImece.Web.csproj
COPY EImece.Web/ EImece.Web/
COPY EImece.Domain.Core/ EImece.Domain.Core/
COPY Resources/ Resources/
RUN dotnet publish EImece.Web/EImece.Web.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EImece.Web.dll"]
