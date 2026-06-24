# DronParcial

Aplicacion de consola .NET 8 para resolver y persistir la trayectoria de un dron con movimiento 2x1, backtracking recursivo y ADO.NET sincronico sobre PostgreSQL.

## Requisitos

- .NET 8 o superior.
- PostgreSQL.
- Paquete `Npgsql`.
- Paquetes `Microsoft.Extensions.Configuration` y `Microsoft.Extensions.Configuration.Json`.
- Cadena de conexion configurada en `appsettings.json`.

## Base de datos

En esta maquina se dejo creado un cluster PostgreSQL local dentro de `.postgres-data`, escuchando en `127.0.0.1:55433`. La cadena de conexion de `appsettings.json` ya apunta a esa instancia.

Para crear o reparar la instancia local, levantar el servidor, crear la base y aplicar las tablas:

```powershell
.\scripts\setup-local-postgres.ps1
```

Si PowerShell bloquea la ejecucion de scripts, usar:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\setup-local-postgres.ps1
```

Si la PC se reinicia y la instancia ya existe, alcanza con:

```powershell
.\scripts\start-local-postgres.ps1
```

Para detenerlo:

```powershell
.\scripts\stop-local-postgres.ps1
```

Para preparar otra base desde cero:

1. Crear una base llamada `dron_parcial` o cambiar el nombre en `appsettings.json`.
2. Ejecutar el script:

```sql
sql/01_create_tables.sql
```

El diseno usa:

- `tb_master_control`: cabecera de cada simulacion exitosa.
- `tb_det_log`: detalle de movimientos, vinculado por FK a `tb_master_control`.

## Ejecucion

```powershell
dotnet restore
dotnet run
```

El programa pide:

- `N`: dimension del terreno, entero mayor o igual a 1.
- `X`: fila inicial.
- `Y`: columna inicial.

Si encuentra solucion, imprime la matriz ordinaria con pasos `0, 1, 2...`, guarda la simulacion en PostgreSQL y muestra los ultimos 5 movimientos reconstruidos desde los valores ofuscados.

## Casos rapidos

- `N = 1`, inicio `(0,0)`: exito con una sola parcela.
- `N = 2`, inicio `(0,0)`: exito, solo se pisa la inicial.
- `N = 3`, inicio `(0,0)`: exito, cubre 8 de 9 parcelas.
- `N = 4`, inicio `(0,0)`: sin solucion.
- `N = 6`, inicio `(0,0)`: exito, cubre todo el terreno.

## Puntos clave de cumplimiento

- Movimiento fijo 2x1 con los 8 destinos posibles.
- Calculo previo de parcelas alcanzables desde el despegue.
- Recursion con backtracking.
- Orden de candidatos por menor grado disponible.
- `appsettings.json` copiado al directorio de salida.
- ADO.NET sincronico con `NpgsqlConnection`, `NpgsqlCommand`, `NpgsqlTransaction`, `ExecuteScalar`, `ExecuteNonQuery` y `ExecuteReader`.
- Insercion del detalle con `while` e indice manual.
- Ofuscacion: paso par se guarda multiplicado por 2; paso impar se guarda negativo.
- Reconstruccion: negativo cambia de signo; no negativo se divide por 2.
