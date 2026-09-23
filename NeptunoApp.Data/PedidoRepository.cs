using System.Data;
using Microsoft.Data.SqlClient;
using NeptunoApp.Models;

namespace NeptunoApp.Data;

/// <summary>
/// Pedidos y su detalle se leen en modo conectado: la cabecera muestra el total
/// del pedido y el detalle se relee despues de cada linea agregada, editada o
/// quitada, asi que una copia en memoria quedaria desactualizada enseguida. Solo
/// el reporte por fechas, que es una consulta de solo lectura, va desconectado.
/// </summary>
public class PedidoRepository : RepositoryBase, IPedidoRepository
{
    public PedidoRepository(string cadenaConexion) : base(cadenaConexion)
    {
    }

    public Task<List<Pedido>> ListarAsync()
        => ListarConectadoAsync("dbo.usp_Pedido_Listar", Mapear);

    public Task<Pedido?> ObtenerPorIdAsync(int pedidoId)
        => ObtenerConectadoAsync("dbo.usp_Pedido_ObtenerPorId", Mapear, p =>
            p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId);

    public Task<int> CrearAsync(Pedido pedido)
        => InsertarAsync("dbo.usp_Pedido_Crear", "@PedidoID", p => AgregarDatos(p, pedido));

    public Task ActualizarAsync(Pedido pedido)
        => EjecutarAsync("dbo.usp_Pedido_Actualizar", p =>
        {
            p.Add("@PedidoID", SqlDbType.Int).Value = pedido.PedidoID;
            AgregarDatos(p, pedido);
        });

    public Task EliminarAsync(int pedidoId)
        => EjecutarAsync("dbo.usp_Pedido_Eliminar", p =>
            p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId);

    public Task<List<DetallePedido>> ListarDetalleAsync(int pedidoId)
        => ListarConectadoAsync("dbo.usp_DetallePedido_ListarPorPedido", MapearDetalle, p =>
            p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId);

    public Task AgregarDetalleAsync(DetallePedido detalle)
        => EjecutarAsync("dbo.usp_DetallePedido_Agregar", p => AgregarDatosDetalle(p, detalle));

    public Task ActualizarDetalleAsync(DetallePedido detalle)
        => EjecutarAsync("dbo.usp_DetallePedido_Actualizar", p => AgregarDatosDetalle(p, detalle));

    public Task EliminarDetalleAsync(int pedidoId, int productoId)
        => EjecutarAsync("dbo.usp_DetallePedido_Eliminar", p =>
        {
            p.Add("@PedidoID", SqlDbType.Int).Value = pedidoId;
            p.Add("@ProductoID", SqlDbType.Int).Value = productoId;
        });

    public Task<List<LineaReporte>> ListarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        => ListarDesconectadoAsync("dbo.usp_DetallePedido_ListarPorRangoFechas", MapearLinea, p =>
        {
            p.Add("@FechaInicio", SqlDbType.Date).Value = fechaInicio.Date;
            p.Add("@FechaFin", SqlDbType.Date).Value = fechaFin.Date;
        });

    private static void AgregarDatos(SqlParameterCollection p, Pedido pedido)
    {
        p.Add("@ClienteID", SqlDbType.Int).Value = Valor(pedido.ClienteID);
        p.Add("@EmpleadoID", SqlDbType.Int).Value = Valor(pedido.EmpleadoID);
        p.Add("@FechaPedido", SqlDbType.Date).Value = pedido.FechaPedido.Date;
        p.Add("@FechaRequerida", SqlDbType.Date).Value = Valor(pedido.FechaRequerida?.Date);
        p.Add("@FechaEnvio", SqlDbType.Date).Value = Valor(pedido.FechaEnvio?.Date);
        p.Add("@TransportistaID", SqlDbType.Int).Value = Valor(pedido.TransportistaID);
        p.Add("@Destinatario", SqlDbType.NVarChar, 60).Value = Valor(pedido.Destinatario);
        p.Add("@CiudadDestino", SqlDbType.NVarChar, 30).Value = Valor(pedido.CiudadDestino);
        p.Add("@PaisDestino", SqlDbType.NVarChar, 30).Value = Valor(pedido.PaisDestino);
    }

    private static void AgregarDatosDetalle(SqlParameterCollection p, DetallePedido detalle)
    {
        p.Add("@PedidoID", SqlDbType.Int).Value = detalle.PedidoID;
        p.Add("@ProductoID", SqlDbType.Int).Value = detalle.ProductoID;
        p.Add("@PrecioUnidad", SqlDbType.Decimal).Value = detalle.PrecioUnidad;
        p.Add("@Cantidad", SqlDbType.SmallInt).Value = detalle.Cantidad;
        p.Add("@Descuento", SqlDbType.Decimal).Value = detalle.Descuento;
    }

    private static Pedido Mapear(IDataRecord registro) => new()
    {
        PedidoID = registro.Entero("PedidoID"),
        ClienteID = registro.EnteroNulo("ClienteID"),
        EmpleadoID = registro.EnteroNulo("EmpleadoID"),
        FechaPedido = registro.Fecha("FechaPedido"),
        FechaRequerida = registro.FechaNula("FechaRequerida"),
        FechaEnvio = registro.FechaNula("FechaEnvio"),
        TransportistaID = registro.EnteroNulo("TransportistaID"),
        Destinatario = registro.TextoNulo("Destinatario"),
        CiudadDestino = registro.TextoNulo("CiudadDestino"),
        PaisDestino = registro.TextoNulo("PaisDestino"),
        Activo = registro.Booleano("Activo"),
        NombreCliente = registro.TextoNulo("NombreCliente"),
        NombreEmpleado = registro.TextoNulo("NombreEmpleado"),
        NombreTransportista = registro.TextoNulo("NombreTransportista"),
        Total = registro.Decimal("Total")
    };

    private static DetallePedido MapearDetalle(IDataRecord registro) => new()
    {
        PedidoID = registro.Entero("PedidoID"),
        ProductoID = registro.Entero("ProductoID"),
        NombreProducto = registro.TextoNulo("NombreProducto"),
        PrecioUnidad = registro.Decimal("PrecioUnidad"),
        Cantidad = registro.Corto("Cantidad"),
        Descuento = registro.Decimal("Descuento"),
        Subtotal = registro.Decimal("Subtotal")
    };

    private static LineaReporte MapearLinea(IDataRecord registro) => new()
    {
        PedidoID = registro.Entero("PedidoID"),
        FechaPedido = registro.Fecha("FechaPedido"),
        NombreCliente = registro.TextoNulo("NombreCliente"),
        ProductoID = registro.Entero("ProductoID"),
        NombreProducto = registro.Texto("NombreProducto"),
        PrecioUnidad = registro.Decimal("PrecioUnidad"),
        Cantidad = registro.Corto("Cantidad"),
        Descuento = registro.Decimal("Descuento"),
        Subtotal = registro.Decimal("Subtotal")
    };
}
