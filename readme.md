## .env File Configuration

```hocon
POSTGRES_USER=<>
POSTGRES_PASSWORD=<>
POSTGRES_DB=<>
POSTGRES_PORT=<>

FLYWAY_URL=jdbc:postgresql://data:5432/<DB_NAME>

```

## Running the DB and Migrating

```bash
docker compose up -d data
docker compose run --rm flyway migrate
```