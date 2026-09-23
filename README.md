# Laboratorio 06 - Class Library & DataSet sobre NeptunoDB

Aplicación de escritorio WPF (.NET 10, C#, patrón MVVM) que consume SQL Server
exclusivamente a través de procedimientos almacenados con ADO .NET. Parte del
Laboratorio 05 (eliminación lógica con `Activo` y `ExecuteNonQuery` en toda
escritura) y agrega:

- los modelos y el acceso a datos separados en una **biblioteca de clases**
  (`NeptunoApp.Data`), referenciada por la aplicación WPF;
- **modo desconectado** (`SqlDataAdapter` + `DataSet`) en las consultas;
- la cadena de conexión movida al **`App.config` del proyecto de inicio**, leída
  con `ConfigurationManager`;
- la auditoría de cargas bloqueantes (`.Result` / `.Wait()`) para que todo el
  camino de datos sea `async`/`await`.

Usa su **propia base de datos, `NeptunoDB_Lab06`**, separada de las de los
laboratorios anteriores, para que estos cambios no alteren lo ya entregado.

**Curso:** Desarrollo de Aplicaciones Empresariales Avanzado — 6 · C24 · Sección C - D
**Docente:** Arévalo Sermeño, Edwin William
**Integrante:** Aguirre Saavedra, Juan Alexis

## Requisitos

- Windows con .NET SDK 10 (la aplicación es WPF, `net10.0-windows`).
- SQL Server (probado con SQL Server 2025 Express, instancia `.\SQLEXPRESS`)
  con autenticación de Windows.
- `sqlcmd` para ejecutar los scripts desde la línea de comandos, o SQL Server
  Management Studio.

## Cómo ejecutarlo

1. Ejecutar los scripts en este orden, desde `Scripts/`:

   | Orden | Script | Qué hace |
   |-------|--------|----------|
   | 1 | `00_NeptunoDB.sql` | Crea la base de datos, las 8 tablas y los datos de prueba |
   | 2 | `01_NeptunoDB_EliminacionLogica.sql` | Agrega `Activo BIT NOT NULL DEFAULT 1` a Productos, Categorías, Proveedores y Pedidos |
   | 3 | `02_NeptunoDB_Procedimientos.sql` | Procedimientos de categorías, proveedores y productos |
   | 4 | `03_NeptunoDB_Pedidos_Reportes.sql` | Pedidos, detalle de pedidos, catálogos y el reporte por fechas |
   | 5 | `04_NeptunoDB_CorregirAcentos.sql` | Opcional: repara las tildes de los datos de ejemplo |

   Desde la línea de comandos:

   ```
   sqlcmd -S .\SQLEXPRESS -E -C -i 00_NeptunoDB.sql
   sqlcmd -S .\SQLEXPRESS -E -C -i 01_NeptunoDB_EliminacionLogica.sql
   sqlcmd -S .\SQLEXPRESS -E -C -i 02_NeptunoDB_Procedimientos.sql
   sqlcmd -S .\SQLEXPRESS -E -C -i 03_NeptunoDB_Pedidos_Reportes.sql
   sqlcmd -S .\SQLEXPRESS -E -C -i 04_NeptunoDB_CorregirAcentos.sql
   ```

   El script 00 crea `NeptunoDB_Lab06`; los demás hacen `USE NeptunoDB_Lab06`,
   así que nunca tocan la `NeptunoDB` del Laboratorio 04. Los scripts 01 a 04 se
   pueden repetir sin error (los `ALTER TABLE` están protegidos con `COL_LENGTH`
   y los procedimientos son `CREATE OR ALTER`). El script 00 no es repetible,
   porque sus `CREATE TABLE` no tienen guarda.

2. Abrir `Neptuno.slnx` en Visual Studio y ejecutar (F5), o bien
   `dotnet run --project NeptunoApp/NeptunoApp.csproj`.

La cadena de conexión está en `NeptunoApp/App.config` (sección
`connectionStrings`, entrada `NeptunoDB`) y apunta a `NeptunoDB_Lab06` en
`.\SQLEXPRESS`. Si la instancia local tiene otro nombre, se cambia ahí: es un
archivo de configuración, no hace falta recompilar la biblioteca de datos.

## Interfaz

La ventana principal tiene una barra lateral con los cinco módulos; cada uno
se carga al entrar:

| Módulo | Qué permite |
|--------|-------------|
| Productos | Listar, crear, editar y dar de baja productos. La columna *Estado* marca cada producto como **Disponible**, **Reponer** (stock en o bajo el nivel de reorden) o **Descontinuado**. |
| Categorías | Listar, crear, editar y dar de baja categorías. |
| Proveedores | Mantenimiento de proveedores y búsqueda por nombre de contacto y ciudad. |
| Pedidos | Mantenimiento de la cabecera del pedido y, debajo, de sus líneas de detalle (agregar, editar y quitar). |
| Reportes | Detalle de pedidos entre dos fechas, con atajos *Mes actual* / *Año actual* y totales de líneas, unidades e importe. |

Las altas y ediciones se hacen en ventanas de diálogo. Antes de dar de baja un
registro se pide confirmación, indicando que es una eliminación lógica. Los
errores que devuelven los procedimientos (por ejemplo, "la categoría tiene
productos activos asociados") se muestran en una barra roja sobre el listado.

Los estilos están centralizados en `Themes/Theme.xaml` (paleta, botones,
campos, grillas, insignias de estado y barra lateral), así que las vistas no
definen colores propios.

## Pruebas

`NeptunoApp.Tests/` contiene 49 pruebas de integración (xUnit) que usan los
repositorios y ViewModels reales de la aplicación contra `NeptunoDB_Lab06`:

```
dotnet test Neptuno.slnx
dotnet test Neptuno.slnx --filter "FullyQualifiedName~PedidoPruebas"
```

Cada prueba corre dentro de un `TransactionScope` que nunca se confirma, así
que al terminar se revierte y **la base de datos queda igual** (solo se
consumen valores de identidad). Requieren que los scripts 00 a 03 ya estén
aplicados. Cubren:

- Alta con `ExecuteNonQuery` + parámetro `OUTPUT` en las cuatro entidades.
- Baja lógica: la fila sigue en la tabla con `Activo = 0`, desaparece de
  listados, búsquedas y reporte, y no se puede editar ni eliminar otra vez.
- Búsqueda de proveedores por nombre de contacto y ciudad.
- Reporte por intervalo de fechas que excluye pedidos dados de baja.
- Reglas de integridad (categoría/proveedor con productos activos, productos o
  pedidos dados de baja en las líneas de pedido).
- Estructura en la base de datos: columna `Activo BIT NOT NULL DEFAULT 1`,
  procedimientos de eliminación sin `DELETE`, altas sin `SELECT SCOPE_IDENTITY`.
- ViewModels de categorías, proveedores y reportes, y el paso de los errores
  de `THROW` a `ErrorMessage`.

## Estructura

Tres proyectos, `Neptuno.slnx` los agrupa:

```
NeptunoApp.Data/         BIBLIOTECA DE CLASES (net10.0)
  Models/                  Entidades del dominio
  RepositoryBase.cs        Conexión, modo desconectado y ExecuteNonQuery
  I*Repository.cs          Contratos que consumen los ViewModels
  *Repository.cs           Un procedimiento almacenado por operación
  FilaExtensiones.cs       Lectura tipada de las filas del DataSet

NeptunoApp/              APLICACIÓN WPF, proyecto de inicio (net10.0-windows)
  App.config               Cadena de conexión (sección connectionStrings)
  Configuracion/DbConfig   Lee App.config con ConfigurationManager
  App.xaml.cs              Punto de composición: inyecta la cadena y los repositorios
  Models/Seccion.cs        Enum de navegación (es de interfaz, no del dominio)
  ViewModels/              Lógica de cada módulo y de cada formulario
  Views/                   Pantallas de mantenimiento y ventanas de edición
  Themes/                  Estilos y paleta de colores
  Converters/              Convertidores de enlace de datos

NeptunoApp.Tests/        Pruebas de integración contra NeptunoDB_Lab06
Scripts/                 Scripts T-SQL, numerados en orden de ejecución
```

Las referencias van en un solo sentido: `NeptunoApp.Tests` → `NeptunoApp` →
`NeptunoApp.Data`. La biblioteca tiene como destino `net10.0` (sin `-windows`)
justamente para que no pueda depender de WPF: si alguien intentara usar un tipo
de interfaz ahí, el compilador lo rechaza.

`RepositoryBase` centraliza la conexión y la ejecución de procedimientos;
cada repositorio solo declara el nombre del procedimiento, sus parámetros y
cómo mapear el resultado. `ViewModelBase` concentra el indicador de ocupado,
el mensaje de error y el try/catch que atrapa los errores que lanzan los
procedimientos con `THROW`.

## Puntos del laboratorio

| Requisito | Dónde está |
|-----------|-----------|
| Campo de estado `Activo` en las 4 tablas | `01_NeptunoDB_EliminacionLogica.sql` |
| CRUD de productos con baja lógica | `usp_Producto_*` · módulo Productos |
| CRUD de categorías con baja lógica | `usp_Categoria_*` · módulo Categorías |
| CRUD de proveedores con baja lógica | `usp_Proveedor_*` · módulo Proveedores |
| CRUD de pedidos con baja lógica | `usp_Pedido_*` y `usp_DetallePedido_*` · módulo Pedidos |
| Proveedores por nombreContacto y ciudad, solo `Activo = 1` | `usp_Proveedor_Buscar` · filtros del módulo Proveedores |
| Detalle de pedidos con inner join a pedidos por fechas, excluyendo pedidos inactivos | `usp_DetallePedido_ListarPorRangoFechas` · módulo Reportes |
| Alta, edición y baja lógica con `ExecuteNonQuery` | `RepositoryBase.InsertarAsync` y `RepositoryBase.EjecutarAsync` |
| Modelos y acceso a datos en una biblioteca de clases | Proyecto `NeptunoApp.Data` |
| Referencias de proyectos correctas | `NeptunoApp.csproj` y `NeptunoApp.Tests.csproj` |
| Modo desconectado donde corresponde | `RepositoryBase.LlenarAsync` (`SqlDataAdapter` + `DataSet`) |
| Cadena de conexión en el proyecto de inicio | `NeptunoApp/App.config` + `NeptunoApp/Configuracion/DbConfig.cs` |
| Sin cargas bloqueantes (`.Result` / `.Wait()`) | Auditoría al final de este documento |

## Explicación: ExecuteNonQuery

Todas las operaciones de escritura pasan por uno de dos métodos de
`Data/RepositoryBase.cs`, y los dos ejecutan el comando con
`ExecuteNonQueryAsync()`:

| Operación | Método del repositorio | Procedimiento | Qué hace el procedimiento |
|-----------|------------------------|---------------|---------------------------|
| Insertar | `InsertarAsync` | `usp_*_Crear` | `INSERT` y devuelve el id en un parámetro `OUTPUT` |
| Actualizar | `EjecutarAsync` | `usp_*_Actualizar` | `UPDATE ... WHERE Id = @Id AND Activo = 1` |
| Eliminar | `EjecutarAsync` | `usp_*_Eliminar` | `UPDATE ... SET Activo = 0` (baja lógica) |
| Líneas del pedido | `EjecutarAsync` | `usp_DetallePedido_Agregar/Actualizar/Eliminar` | Modifica las líneas de un pedido activo |

**Insertar.** En el Laboratorio 04 los procedimientos de alta terminaban con
`SELECT SCOPE_IDENTITY()` y la aplicación leía ese valor con `ExecuteScalar`.
Como `ExecuteNonQuery` no devuelve filas, ahora cada `usp_*_Crear` declara un
parámetro de salida (`@ProductoID INT OUTPUT`) y hace
`SET @ProductoID = CAST(SCOPE_IDENTITY() AS INT)`. En C#, `InsertarAsync`
agrega ese parámetro con `Direction = ParameterDirection.Output`, ejecuta
`ExecuteNonQueryAsync()` y luego lee `parametro.Value`:

```csharp
var idGenerado = comando.Parameters.Add(parametroId, SqlDbType.Int);
idGenerado.Direction = ParameterDirection.Output;

await conexion.OpenAsync();
await comando.ExecuteNonQueryAsync();
return (int)idGenerado.Value;
```

**Actualizar y eliminar.** `EjecutarAsync` arma el `SqlCommand` con
`CommandType.StoredProcedure`, agrega los parámetros tipados y llama a
`ExecuteNonQueryAsync()`. El botón "Eliminar" de cada pantalla invoca
`EliminarAsync` del repositorio, que ejecuta el procedimiento de baja lógica;
no existe ningún `DELETE` sobre Productos, Categorías, Proveedores ni Pedidos.

**Cómo se detecta que no se afectó ninguna fila.** Los procedimientos usan
`SET NOCOUNT ON`, por lo que `ExecuteNonQuery` devuelve `-1` en lugar del
número de filas. Por eso la verificación se hace dentro del procedimiento:
después del `UPDATE` se revisa `@@ROWCOUNT` y, si es 0, se lanza
`THROW 50xxx` con un mensaje. Esa excepción llega a la aplicación como
`SqlException`, la atrapa `ViewModelBase.EjecutarAsync` y se muestra en la
barra de error de la pantalla.

## Explicación: eliminación lógica

**Campo.** `01_NeptunoDB_EliminacionLogica.sql` agrega
`Activo BIT NOT NULL CONSTRAINT DF_<Tabla>_Activo DEFAULT (1) WITH VALUES` a
Productos, Categorías, Proveedores y Pedidos. `WITH VALUES` deja en 1 las
filas que ya existían y el `DEFAULT` hace que todo registro nuevo nazca activo
sin que la aplicación tenga que enviarlo.

**Baja.** Cada `usp_*_Eliminar` ejecuta
`UPDATE <Tabla> SET Activo = 0 WHERE <Id> = @Id AND Activo = 1`. La condición
`Activo = 1` hace que eliminar dos veces el mismo registro devuelva
"ya fue eliminado" en vez de pasar en silencio.

**Verificación en listados y consultas.** Todos los procedimientos que leen
esas tablas filtran por `Activo = 1`:

| Procedimiento | Filtro |
|---------------|--------|
| `usp_Categoria_Listar`, `usp_Categoria_ObtenerPorId` | `WHERE Activo = 1` |
| `usp_Proveedor_Listar`, `usp_Proveedor_ObtenerPorId`, `usp_Proveedor_Buscar` | `WHERE Activo = 1` |
| `usp_Producto_Listar`, `usp_Producto_ObtenerPorId` | `WHERE p.Activo = 1` |
| `usp_Pedido_Listar`, `usp_Pedido_ObtenerPorId` | `WHERE ped.Activo = 1` |
| `usp_DetallePedido_ListarPorPedido` | `ped.Activo = 1` |
| `usp_DetallePedido_ListarPorRangoFechas` (reporte) | `ped.Activo = 1` |

Además, las escrituras también respetan el estado:

- `usp_*_Actualizar` solo modifica registros activos.
- `usp_Producto_Crear/Actualizar` rechazan una categoría o un proveedor dados de
  baja (`THROW 50306` / `50307`).
- `usp_Categoria_Eliminar` y `usp_Proveedor_Eliminar` se bloquean mientras
  tengan **productos activos** (`THROW 50102` / `50202`), para que ningún
  producto visible quede apuntando a un registro dado de baja.
- `usp_DetallePedido_Agregar` rechaza productos dados de baja (`50509`) y las
  tres operaciones de detalle rechazan pedidos dados de baja (`50508`).

Como las combos de la interfaz se llenan con los mismos procedimientos
`_Listar`, un registro dado de baja desaparece también de las listas
desplegables de productos y pedidos.

## Explicación: biblioteca de clases y referencias

Los modelos y todo el acceso a datos salieron de la aplicación WPF y viven en
el proyecto **`NeptunoApp.Data`**. La aplicación ya no sabe cómo se habla con
SQL Server: solo conoce las interfaces `I*Repository`.

| Antes (Lab05) | Ahora (Lab06) |
|---------------|---------------|
| `NeptunoApp/Models/*.cs` | `NeptunoApp.Data/Models/*.cs` |
| `NeptunoApp/Data/*.cs` | `NeptunoApp.Data/*.cs` |
| `NeptunoApp/Data/DbConfig.cs` (cadena en una constante) | `NeptunoApp/App.config` + `NeptunoApp/Configuracion/DbConfig.cs` |
| Un solo proyecto | `NeptunoApp.Data` ← `NeptunoApp` ← `NeptunoApp.Tests` |

Dos decisiones que vale la pena justificar:

- **`Seccion.cs` se quedó en la aplicación.** Es el enum de los módulos de la
  barra lateral: describe la navegación, no el negocio. Mover a la biblioteca
  todo lo que estaba en `Models/` habría metido una preocupación de interfaz en
  la capa de datos.
- **La biblioteca no lee configuración.** Cada repositorio recibe la cadena de
  conexión por constructor. Quién la obtiene y de dónde es problema del
  proyecto de inicio (ver la auditoría de `App.config` más abajo).

## Explicación: escenario desconectado (criterio)

El criterio es **por tipo de operación, no por pantalla**:

> **Toda consulta** (listados, búsqueda de proveedores, obtener por id y el
> reporte por fechas) se resuelve en **modo desconectado** con
> `SqlDataAdapter` llenando un `DataSet`.
> **Toda escritura** (alta, actualización y baja lógica) se mantiene en
> **modo conectado** con `ExecuteNonQuery` sobre el procedimiento almacenado.

Ambos modos están implementados en `NeptunoApp.Data/RepositoryBase.cs`:

```csharp
// Lectura: modo desconectado
using var adaptador = new SqlDataAdapter(comando);
var conjunto = new DataSet("Neptuno");
adaptador.Fill(conjunto, "Resultado");   // abre y cierra la conexión
```

**Por qué las consultas van desconectadas.** Los listados alimentan grillas y
combos que el usuario mira, ordena y recorre durante minutos; no tiene sentido
sostener una conexión abierta mientras tanto. `Fill` abre la conexión, trae las
filas y la cierra de inmediato, y la aplicación sigue trabajando sobre la copia
en memoria. También simplifica el código: no hay un `SqlDataReader` vivo al que
haya que respetarle el ciclo de vida.

**Por qué las escrituras siguen conectadas.** Las reglas de negocio están en los
procedimientos almacenados (baja lógica, `@@ROWCOUNT`, validaciones con `THROW`
50xxx, bloqueo de categorías con productos activos). Un `SqlDataAdapter` con
comandos generados escribiría los cambios del `DataSet` directamente sobre las
tablas y se saltaría esas reglas, además de resolver la concurrencia en el
cliente en vez de en la base de datos. Cada escritura es de una sola fila y
tiene que confirmarse o fallar en el momento, así que el modo conectado con
`ExecuteNonQuery` es el que corresponde.

**Consecuencia en el mapeo.** Como lo que se lee ya no es un `SqlDataReader`
sino filas de un `DataSet`, los mapeos de todos los repositorios pasaron de
`Mapear(SqlDataReader lector)` a `Mapear(DataRow fila)`, y las extensiones de
lectura por nombre de columna son ahora `FilaExtensiones` (antes
`LectorExtensiones`).

**Sobre `Fill` y el hilo de la interfaz.** `SqlDataAdapter.Fill` no tiene
versión asincrónica. Para no bloquear la ventana, `LlenarAsync` lo ejecuta en
un hilo del pool con `Task.Run` y devuelve un `Task<DataTable>`: quien llama
sigue usando `await` y el hilo de interfaz queda libre.

## Auditoría: la cadena de conexión y App.config

**Lo que se encontró.** El proyecto no tenía `App.config`. La cadena vivía en
una constante de `NeptunoApp/Data/DbConfig.cs`, es decir, dentro del código que
en este laboratorio se muda a la biblioteca de datos. De haberla movido tal
cual, habría quedado justo en el lugar equivocado.

**Lo que se hizo.**

1. Se creó `NeptunoApp/App.config` en el **proyecto de inicio** con la entrada
   `NeptunoDB` en `<connectionStrings>`.
2. `DbConfig` se quedó en la aplicación WPF (`NeptunoApp/Configuracion/`) y lee
   esa entrada con `ConfigurationManager.ConnectionStrings["NeptunoDB"]`.
3. La biblioteca `NeptunoApp.Data` no referencia `ConfigurationManager` ni
   conoce el nombre de la entrada: recibe la cadena por constructor desde
   `App.xaml.cs`.

**Por qué el `App.config` tiene que estar en el proyecto de inicio.**
`ConfigurationManager` busca el archivo de configuración del ensamblado que
arranca el proceso: al compilar, `NeptunoApp/App.config` se copia a la salida
como `NeptunoApp.dll.config` y es el que se lee en tiempo de ejecución. Un
`App.config` colocado dentro de `NeptunoApp.Data` no se copia ni se consulta
nunca, y la búsqueda devolvería `null`. Por eso `DbConfig` lanza un
`ConfigurationErrorsException` con un mensaje explícito si la entrada falta, en
lugar de fallar más tarde con una cadena vacía.

Las pruebas no usan ese `DbConfig`: el proceso que arranca en una corrida de
pruebas es el host de pruebas, no `NeptunoApp.exe`, así que
`ConfigurationManager` no encontraría la entrada. `NeptunoApp.Tests` declara su
propia cadena en `ConfiguracionPruebas.cs`, que además se puede sobrescribir
con la variable de entorno `NEPTUNO_CONEXION`.

## Auditoría: cargas bloqueantes y async/await

**Lo que se encontró.** No había ningún `.Result`, `.Wait()` ni
`.GetAwaiter().GetResult()` en el código heredado del Laboratorio 05: los
repositorios ya eran asincrónicos de punta a punta y los manejadores de evento
de las vistas ya estaban declarados como `private async void`, que es la forma
correcta para un manejador (es el único caso en el que `async void` es válido,
porque no hay un `Task` que el framework pueda esperar).

**Lo que se cuidó en este laboratorio.** El riesgo real aparecía al introducir
el modo desconectado, porque `SqlDataAdapter.Fill` es sincrónico y lo más
directo habría sido llamarlo dentro de un método `async` (bloqueando el hilo de
interfaz) o exponerlo como sincrónico y consumirlo con `.Result`. En su lugar,
`LlenarAsync` lo ejecuta con `Task.Run` y el resto de la cadena
—repositorio → ViewModel (`[RelayCommand] private async Task ...`) → manejador
`async void`— sigue con `await` sin bloquear.

**Comprobación:**

```
grep -rEn "\.Result\b|\.Wait\(\)|GetAwaiter\(\)" --include=*.cs .
```

no devuelve resultados en los tres proyectos.

## Decisiones de diseño

- **Productos que figuran en pedidos.** En el Laboratorio 04 no se podían
  borrar. Con la baja lógica sí se pueden dar de baja: la fila se conserva, así
  que las líneas de pedidos anteriores y el reporte siguen mostrando su nombre.
  Si se edita una línea antigua cuyo producto está inactivo, el formulario lo
  muestra como "(dado de baja)" para poder cambiar cantidad, precio o descuento.
- **Pedidos.** `usp_Pedido_Eliminar` ya no borra las líneas y la cabecera en una
  transacción: solo marca la cabecera con `Activo = 0` y el detalle queda
  intacto. El pedido sale de los listados y del reporte.
- **Líneas del pedido.** `DetallePedidos` no tiene campo `Activo` (el enunciado
  lo pide para las otras cuatro tablas) y quitar una línea es editar el
  contenido de un pedido, no darlo de baja. Por eso
  `usp_DetallePedido_Eliminar` es el único procedimiento con `DELETE`, y solo
  actúa sobre pedidos activos.
- **Clientes, empleados y transportistas** quedan fuera de la baja lógica y sus
  listados no filtran por estado.

## Notas de implementación

- `DetallePedidos` tiene clave primaria compuesta `(PedidoID, ProductoID)` y no
  tiene columna identidad, por eso su alta no usa parámetro `OUTPUT` y su
  edición no permite cambiar el producto de una línea existente.
- Las columnas de fecha son `DATE`, se enlazan como `SqlDbType.Date`.
- El descuento se guarda como fracción (0.05 = 5 %) y se captura en porcentaje.
- Códigos de error por entidad: 501xx categorías, 502xx proveedores,
  503xx productos, 504xx pedidos, 505xx detalle, 506xx reporte.
- Los mapeos leen `DataRow` y no `SqlDataReader`. Las conversiones nulas usan
  `fila.IsNull("Columna")` en vez de `IsDBNull(ordinal)`; el resto del mapeo es
  idéntico porque el `DataSet` conserva los tipos CLR de las columnas.
- La biblioteca referencia `CommunityToolkit.Mvvm` porque los modelos notifican
  cambios con `[ObservableProperty]`. El paquete no depende de WPF, así que no
  ata la capa de datos a la interfaz.
- En `net10.0-windows`, `ConfigurationManager` ya lo aporta el framework de
  escritorio: no hace falta el paquete `System.Configuration.ConfigurationManager`.
