using System.Data;
using Microsoft.Data.SqlClient;
using NeptunoApp.Models;

namespace NeptunoApp.Data;

public class PedidoRepository : RepositoryBase, IPedidoRepository
{
    public PedidoRepository(string cadenaConexion) : base(cadenaConexion)
    {
    }

    public Task<List<Pedido>> ListarAsync()
        => ListarAsync("dbo.usp_Pedido_Listar", Mapear);

    public Task<Pedido?> ObtenerPorIdAsync(int pedidoId)
        => ObtenerAsync("dbo.usp_Pedido_ObtenerPorId", Mapear, p =>
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
        => ListarAsync("dbo.usp_DetallePedido_ListarPorPedido", MapearDetalle, p =>
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
        => ListarAsync("dbo.usp_DetallePedido_ListarPorRangoFechas", MapearLinea, p =>
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

    private static Pedido Mapear(DataRow fila) => new()
    {
        PedidoID = fila.Entero("PedidoID"),
        ClienteID = fila.EnteroNulo("ClienteID"),
        EmpleadoID = fila.EnteroNulo("EmpleadoID"),
        FechaPedido = fila.Fecha("FechaPedido"),
        FechaRequerida = fila.FechaNula("FechaRequerida"),
        FechaEnvio = fila.FechaNula("FechaEnvio"),
        TransportistaID = fila.EnteroNulo("TransportistaID"),
        Destinatario = fila.TextoNulo("Destinatario"),
        CiudadDestino = fila.TextoNulo("CiudadDestino"),
        PaisDestino = fila.TextoNulo("PaisDestino"),
        Activo = fila.Booleano("Activo"),
        NombreCliente = fila.TextoNulo("NombreCliente"),
        NombreEmpleado = fila.TextoNulo("NombreEmpleado"),
        NombreTransportista = fila.TextoNulo("NombreTransportista"),
        Total = fila.Decimal("Total")
    };

    private static DetallePedido MapearDetalle(DataRow fila) => new()
    {
        PedidoID = fila.Entero("PedidoID"),
        ProductoID = fila.Entero("ProductoID"),
        NombreProducto = fila.TextoNulo("NombreProducto"),
        PrecioUnidad = fila.Decimal("PrecioUnidad"),
        Cantidad = fila.Corto("Cantidad"),
        Descuento = fila.Decimal("Descuento"),
        Subtotal = fila.Decimal("Subtotal")
    };

    private static LineaReporte MapearLinea(DataRow fila) => new()
    {
        PedidoID = fila.Entero("PedidoID"),
        FechaPedido = fila.Fecha("FechaPedido"),
        NombreCliente = fila.TextoNulo("NombreCliente"),
        ProductoID = fila.Entero("ProductoID"),
        NombreProducto = fila.Texto("NombreProducto"),
        PrecioUnidad = fila.Decimal("PrecioUnidad"),
        Cantidad = fila.Corto("Cantidad"),
        Descuento = fila.Decimal("Descuento"),
        Subtotal = fila.Decimal("Subtotal")
    };
}
