using System.Data;
using Microsoft.Data.SqlClient;

namespace NeptunoApp.Data;

/// <summary>
/// Clase base de los repositorios. Centraliza el acceso a SQL Server; las clases
/// derivadas solo declaran el nombre del procedimiento, sus parametros y como
/// mapear el resultado.
///
/// Las dos formas de trabajo de ADO .NET estan disponibles y cada repositorio
/// elige, por operacion, la que corresponde:
///
/// - Modo DESCONECTADO (<see cref="ListarDesconectadoAsync"/>): SqlDataAdapter
///   llena un DataSet, abre y cierra la conexion el mismo, y la aplicacion
///   trabaja sobre la copia en memoria. Se usa para datos de consulta que el
///   usuario solo mira y recorre: listados de mantenimiento, combos, la busqueda
///   de proveedores y el reporte por fechas.
///
/// - Modo CONECTADO de lectura (<see cref="ListarConectadoAsync"/> y
///   <see cref="ObtenerConectadoAsync"/>): SqlDataReader sobre la conexion
///   abierta, recorriendo las filas a medida que llegan. Se usa donde el dato
///   tiene que estar al dia en el momento de usarlo: los pedidos y su detalle,
///   que cambian con cada linea que se agrega, y la relectura por id justo antes
///   de editar un registro.
///
/// - ESCRITURAS (alta, actualizacion y baja logica): siempre modo CONECTADO con
///   ExecuteNonQuery sobre procedimientos almacenados, porque las reglas de
///   negocio y la validacion viven en el procedimiento, que informa los errores
///   con THROW; un DataAdapter con comandos generados perderia esas reglas.
/// </summary>
public abstract class RepositoryBase
{
    private readonly string _cadenaConexion;

    protected RepositoryBase(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    /// <summary>Convierte un valor nulo de .NET al DBNull que espera ADO .NET.</summary>
    protected static object Valor(object? valor) => valor ?? DBNull.Value;

    /// <summary>
    /// Lectura desconectada: llena un DataSet con SqlDataAdapter y devuelve la
    /// lista ya mapeada. Al volver de <see cref="LlenarAsync"/> la conexion ya
    /// esta cerrada; el mapeo recorre la tabla en memoria con un DataTableReader.
    /// </summary>
    protected async Task<List<T>> ListarDesconectadoAsync<T>(
        string procedimiento,
        Func<IDataRecord, T> mapear,
        Action<SqlParameterCollection>? parametros = null)
    {
        var tabla = await LlenarAsync(procedimiento, parametros);

        var resultado = new List<T>(tabla.Rows.Count);
        using var filas = tabla.CreateDataReader();
        while (filas.Read())
        {
            resultado.Add(mapear(filas));
        }
        return resultado;
    }

    /// <summary>
    /// Lectura conectada: abre la conexion, ejecuta el procedimiento con
    /// ExecuteReader y mapea cada fila mientras el SqlDataReader esta abierto.
    /// </summary>
    protected async Task<List<T>> ListarConectadoAsync<T>(
        string procedimiento,
        Func<IDataRecord, T> mapear,
        Action<SqlParameterCollection>? parametros = null)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = CrearComando(conexion, procedimiento, parametros);

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync();

        var resultado = new List<T>();
        while (await lector.ReadAsync())
        {
            resultado.Add(mapear(lector));
        }
        return resultado;
    }

    /// <summary>Igual que <see cref="ListarConectadoAsync"/> pero para una sola fila.</summary>
    protected async Task<T?> ObtenerConectadoAsync<T>(
        string procedimiento,
        Func<IDataRecord, T> mapear,
        Action<SqlParameterCollection> parametros) where T : class
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = CrearComando(conexion, procedimiento, parametros);

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync(CommandBehavior.SingleRow);
        return await lector.ReadAsync() ? mapear(lector) : null;
    }

    /// <summary>
    /// Ejecuta un procedimiento de consulta en modo desconectado y devuelve la
    /// tabla del DataSet.
    ///
    /// SqlDataAdapter.Fill no tiene version asincronica, asi que la llamada
    /// bloqueante se hace en un hilo del pool con Task.Run: quien llama sigue
    /// usando await y el hilo de la interfaz nunca se queda esperando.
    /// Fill se encarga de abrir y cerrar la conexion.
    /// </summary>
    protected Task<DataTable> LlenarAsync(
        string procedimiento,
        Action<SqlParameterCollection>? parametros = null)
        => Task.Run(() =>
        {
            using var conexion = new SqlConnection(_cadenaConexion);
            using var comando = CrearComando(conexion, procedimiento, parametros);
            using var adaptador = new SqlDataAdapter(comando);

            var conjunto = new DataSet("Neptuno");
            adaptador.Fill(conjunto, "Resultado");

            // Un procedimiento que lanza THROW antes del SELECT no devuelve
            // ninguna tabla; se trata como resultado vacio.
            return conjunto.Tables.Count == 0
                ? new DataTable("Resultado")
                : conjunto.Tables[0];
        });

    /// <summary>
    /// Ejecuta un procedimiento de alta con ExecuteNonQuery. El procedimiento no
    /// devuelve filas: entrega el identificador generado en el parametro OUTPUT
    /// <paramref name="parametroId"/>, que se lee despues de ejecutar el comando.
    /// </summary>
    protected async Task<int> InsertarAsync(
        string procedimiento,
        string parametroId,
        Action<SqlParameterCollection> parametros)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = CrearComando(conexion, procedimiento, parametros);

        var idGenerado = comando.Parameters.Add(parametroId, SqlDbType.Int);
        idGenerado.Direction = ParameterDirection.Output;

        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
        return (int)idGenerado.Value;
    }

    /// <summary>
    /// Ejecuta con ExecuteNonQuery un procedimiento de escritura que no devuelve
    /// filas: actualizar o eliminar logicamente (Activo = 0). Los procedimientos
    /// usan SET NOCOUNT ON, asi que el valor de retorno es -1 y no sirve para saber
    /// si se afecto la fila; ese caso lo detecta el procedimiento con @@ROWCOUNT y
    /// lo informa con THROW, que llega aqui como SqlException.
    /// </summary>
    protected async Task EjecutarAsync(string procedimiento, Action<SqlParameterCollection> parametros)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = CrearComando(conexion, procedimiento, parametros);

        await conexion.OpenAsync();
        await comando.ExecuteNonQueryAsync();
    }

    private static SqlCommand CrearComando(
        SqlConnection conexion,
        string procedimiento,
        Action<SqlParameterCollection>? parametros)
    {
        var comando = new SqlCommand(procedimiento, conexion)
        {
            CommandType = CommandType.StoredProcedure
        };
        parametros?.Invoke(comando.Parameters);
        return comando;
    }
}
