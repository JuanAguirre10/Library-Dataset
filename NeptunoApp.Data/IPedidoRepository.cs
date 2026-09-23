using NeptunoApp.Models;

namespace NeptunoApp.Data;

public interface IPedidoRepository
{
    Task<List<Pedido>> ListarAsync();
    Task<Pedido?> ObtenerPorIdAsync(int pedidoId);
    Task<int> CrearAsync(Pedido pedido);
    Task ActualizarAsync(Pedido pedido);
    /// <summary>Eliminacion logica: ejecuta con ExecuteNonQuery un procedimiento que pone Activo = 0.</summary>
    Task EliminarAsync(int pedidoId);

    Task<List<DetallePedido>> ListarDetalleAsync(int pedidoId);
    Task AgregarDetalleAsync(DetallePedido detalle);
    Task ActualizarDetalleAsync(DetallePedido detalle);
    Task EliminarDetalleAsync(int pedidoId, int productoId);

    /// <summary>Detalles de pedidos unidos a la cabecera, filtrados por intervalo de fechas.</summary>
    Task<List<LineaReporte>> ListarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
}
