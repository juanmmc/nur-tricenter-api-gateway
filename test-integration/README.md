# Integration tests (Postman / Newman)

Colección end-to-end del API Gateway: health, root status, Prometheus metrics, correlation id, CORS preflight, auth vía Keycloak y ruteo a downstream.

## Modos de ejecución

La variable `runSmokeOnly` controla el alcance:

- `true` (default en CI) — ejecuta sólo los requests que no dependen de Keycloak/downstream (gateway-only smoke tests).
- `false` — ejecuta la suite completa; requiere Keycloak accesible y al menos el microservicio `patients` corriendo.

## Local

```bash
# Smoke (no requiere downstream)
newman run test-integration/nur-gateway.postman_collection.json \
  -e test-integration/nur-gateway.postman_environment.json

# Suite completa (gateway + keycloak + patients)
newman run test-integration/nur-gateway.postman_collection.json \
  -e test-integration/nur-gateway.postman_environment.json \
  --env-var runSmokeOnly=false \
  --env-var keycloakUser=<user> --env-var keycloakPassword=<pass>
```

## CI

El workflow `deploy-ssh.yml` levanta el gateway y ejecuta la versión smoke. Para correr la suite completa se usa el job manual `workflow_dispatch` con inputs.
