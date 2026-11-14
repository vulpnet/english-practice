# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY EnglishPractice.sln .
COPY src/EnglishPractice.Domain/EnglishPractice.Domain.csproj src/EnglishPractice.Domain/
COPY src/EnglishPractice.Application/EnglishPractice.Application.csproj src/EnglishPractice.Application/
COPY src/EnglishPractice.Infrastructure/EnglishPractice.Infrastructure.csproj src/EnglishPractice.Infrastructure/
COPY src/EnglishPractice.API/EnglishPractice.API.csproj src/EnglishPractice.API/

RUN dotnet restore src/EnglishPractice.API/EnglishPractice.API.csproj

COPY src/ src/
RUN dotnet publish src/EnglishPractice.API/EnglishPractice.API.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "EnglishPractice.API.dll"]
