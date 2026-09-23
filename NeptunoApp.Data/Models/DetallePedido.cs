using CommunityToolkit.Mvvm.ComponentModel;

namespace NeptunoApp.Models;

/// <summary>
/// Linea de un pedido. La clave primaria en la base de datos es compuesta
/// (PedidoID, ProductoID), por eso ambas se conservan en el modelo.
/// </summary>
public partial class DetallePedido : ObservableObject
{
    [ObservableProperty]
    private int pedidoID;

    [ObservableProperty]
    private int productoID;

    [ObservableProperty]
    private string? nombreProducto;

    [ObservableProperty]
    private decimal precioUnidad;

    [ObservableProperty]
    private short cantidad = 1;

    [ObservableProperty]
    private decimal descuento;

    [ObservableProperty]
    private decimal subtotal;
}
