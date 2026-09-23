using System.Transactions;
using Microsoft.Data.SqlClient;
using NeptunoApp.Data;
using NeptunoApp.Models;

// Las pruebas comparten filas de NeptunoDB_Lab06; en paralelo sus transacciones se bloquearian entre si.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NeptunoApp.Tests;

/// <summary>
/// Base de las pruebas de integracion. Usan los repositorios reales de la aplicacion
/// contra NeptunoDB_Lab06 (ConfiguracionPruebas.Cadena), pero cada prueba corre dentro de un
/// TransactionScope que nunca se completa: al terminar se hace rollback y la base de
/// datos queda como estaba. Los repositorios abren sus conexiones dentro del ambito,
/// asi que se enlistan en la misma transaccion.
/// </summary>
public abstract class PruebaConBaseDatos
{
    // static readonly y no const: la cadena se resuelve en tiempo de ejecucion.
    protected static readonly string Cadena = ConfiguracionPruebas.Cadena;

    protected readonly CategoriaRepository Categorias = new(Cadena);
    protected readonly ProveedorRepository Proveedores = new(Cadena);
    protected readonly ProductoRepository Productos = new(Cadena);
    protected readonly PedidoRepository Pedidos = new(Cadena);
    protected readonly CatalogoRepository Catalogos = new(Cadena);

    /// <summary>Ejecuta la prueba dentro de una transaccion que siempre se revierte.</summary>
    protected static async Task EnTransaccionAsync(Func<Task> prueba)
    {
        using var ambito = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await prueba();
        // Sin ambito.Complete(): el Dispose revierte todo lo que hizo la prueba.
    }

    /// <summary>Consulta directa a la tabla, sin pasar por los procedimientos (ve tambien las filas inactivas).</summary>
    protected static async Task<object?> EscalarAsync(string sql, params (string Nombre, object Valor)[] parametros)
    {
        await using var conexion = new SqlConnection(Cadena);
        await using var comando = new SqlCommand(sql, conexion);
        foreach (var (nombre, valor) in parametros)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }
        await conexion.OpenAsync();
        var resultado = await comando.ExecuteScalarAsync();
        return resultado is DBNull ? null : resultado;
    }

    /// <summary>Estado Activo guardado en la tabla, o null si la fila no existe fisicamente.</summary>
    protected static async Task<bool?> ActivoEnTablaAsync(string tabla, string columnaId, int id)
    {
        var valor = await EscalarAsync($"SELECT Activo FROM dbo.{tabla} WHERE {columnaId} = @Id", ("@Id", id));
        return valor is null ? null : (bool)valor;
    }

    /// <summary>Verifica que la accion falle con el THROW indicado y devuelve el mensaje.</summary>
    protected static async Task<string> DebeLanzarAsync(int numeroError, Func<Task> accion)
    {
        var ex = await Assert.ThrowsAsync<SqlException>(accion);
        Assert.Equal(numeroError, ex.Number);
        return ex.Message;
    }

    protected static string Unico(string prefijo) => $"{prefijo} {Guid.NewGuid():N}"[..Math.Min(30, prefijo.Length + 9)];

    protected async Task<Categoria> NuevaCategoriaAsync()
    {
        var categoria = new Categoria { NombreCategoria = Unico("Cat"), Descripcion = "Prueba" };
        categoria.CategoriaID = await Categorias.CrearAsync(categoria);
        return categoria;
    }

    protected async Task<Proveedor> NuevoProveedorAsync(string? contacto = null, string? ciudad = null)
    {
        var proveedor = new Proveedor
        {
            CompaniaNombre = Unico("Prov"),
            NombreContacto = contacto ?? Unico("Contacto"),
            Ciudad = ciudad ?? Unico("Ciudad")
        };
        proveedor.ProveedorID = await Proveedores.CrearAsync(proveedor);
        return proveedor;
    }

    protected async Task<Producto> NuevoProductoAsync(int? categoriaId = null, int? proveedorId = null)
    {
        var producto = new Producto
        {
            NombreProducto = Unico("Prod"),
            CategoriaID = categoriaId,
            ProveedorID = proveedorId,
            PrecioUnidad = 10.50m,
            UnidadesEnExistencia = 20
        };
        producto.ProductoID = await Productos.CrearAsync(producto);
        return producto;
    }

    protected async Task<Pedido> NuevoPedidoAsync(DateTime fecha)
    {
        var pedido = new Pedido { FechaPedido = fecha, Destinatario = "Prueba" };
        pedido.PedidoID = await Pedidos.CrearAsync(pedido);
        return pedido;
    }
}
