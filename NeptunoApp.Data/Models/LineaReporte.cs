namespace NeptunoApp.Models;

/// <summary>
/// Fila del reporte de detalles de pedidos filtrado por intervalo de fechas.
/// Combina datos del pedido y del producto, por eso no corresponde a una sola tabla.
/// </summary>
public class LineaReporte
{
    public int PedidoID { get; init; }
    public DateTime FechaPedido { get; init; }
    public string? NombreCliente { get; init; }
    public int ProductoID { get; init; }
    public string NombreProducto { get; init; } = string.Empty;
    public decimal PrecioUnidad { get; init; }
    public short Cantidad { get; init; }
    public decimal Descuento { get; init; }
    public decimal Subtotal { get; init; }
}
