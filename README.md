# ApiEcommerce

API de ejemplo para el curso .NET.

## Requisitos
- .NET SDK 8.x
- Docker (opcional, para SQL Server)

## Ejecutar en local
1) Restaurar y compilar:

```bash
dotnet restore

dotnet build
```

2) Ejecutar la API:

```bash
dotnet run --project ApiEcommerce/ApiEcommerce.csproj
```

## Base de datos con Docker
Este proyecto incluye un SQL Server en Docker usando docker-compose.

1) Levantar contenedores:

```bash
docker-compose -f ApiEcommerce/docker-compose.yaml up -d
```

2) Detener contenedores:

```bash
docker-compose -f ApiEcommerce/docker-compose.yaml down
```

Nota: revisa y ajusta la variable `MSSQL_SA_PASSWORD` en el archivo de docker-compose si es necesario.

## Configuracion
- Configuracion de la app: `ApiEcommerce/appsettings.json`
- Configuracion de desarrollo: `ApiEcommerce/appsettings.Development.json`

## Estructura
- `ApiEcommerce/` proyecto principal
- `ApiEcommerce/Models/` entidades
- `ApiEcommerce/Models/Dtos/` DTOs
- `ApiEcommerce/Repository/` capa de repositorio
- `ApiEcommerce/Mapping/` perfiles de mapeo
- `ApiEcommerce/Data/` acceso a datos
