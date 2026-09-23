using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

/// <summary>
/// Edicion de una linea del pedido. Al ser la clave primaria compuesta
/// (PedidoID, ProductoID), el producto solo se puede elegir al agregar:
/// en una linea existente cambiarlo equivaldria a cambiar la clave.
/// </summary>
public partial class DetalleEditViewModel : ObservableObject
{
    public int PedidoID { get; }
    public bool EsNueva { get; }

    public ObservableCollection<Producto> Productos { get; }

    [ObservableProperty]
    private Producto? productoSeleccionado;

    [ObservableProperty]
    private decimal precioUnidad;

    [ObservableProperty]
    private short cantidad = 1;

    /// <summary>Descuento expresado en porcentaje (0 a 100) para que sea comodo de escribir.</summary>
    [ObservableProperty]
    private decimal descuentoPorcentaje;

    public string TituloVentana => EsNueva ? "Agregar línea" : "Editar línea";

    public DetalleEditViewModel(int pedidoId, IEnumerable<Producto> productos, DetallePedido? detalle = null)
    {
        PedidoID = pedidoId;
        Productos = new ObservableCollection<Producto>(productos);
        EsNueva = detalle is null;

        if (detalle is not null)
        {
            productoSeleccionado = Productos.FirstOrDefault(p => p.ProductoID == detalle.ProductoID);

            // El listado de productos solo trae los activos. Si el producto de esta
            // linea fue dado de baja logicamente, se agrega solo para mostrarlo: la
            // linea historica se puede seguir editando (precio, cantidad, descuento).
            if (productoSeleccionado is null)
            {
                productoSeleccionado = new Producto
                {
                    ProductoID = detalle.ProductoID,
                    NombreProducto = $"{detalle.NombreProducto} (dado de baja)",
                    Activo = false
                };
                Productos.Add(productoSeleccionado);
            }
            precioUnidad = detalle.PrecioUnidad;
            cantidad = detalle.Cantidad;
            descuentoPorcentaje = detalle.Descuento * 100m;
        }
    }

    /// <summary>Al elegir un producto nuevo se propone su precio de lista.</summary>
    partial void OnProductoSeleccionadoChanged(Producto? value)
    {
        if (EsNueva && value is not null)
        {
            PrecioUnidad = value.PrecioUnidad;
        }
    }

    public string? Validar()
    {
        if (ProductoSeleccionado is null)
            return "Debe seleccionar un producto.";

        if (Cantidad <= 0)
            return "La cantidad debe ser mayor que cero.";

        if (PrecioUnidad < 0)
            return "El precio unitario no puede ser negativo.";

        if (DescuentoPorcentaje < 0 || DescuentoPorcentaje > 100)
            return "El descuento debe estar entre 0 y 100 por ciento.";

        return null;
    }

    public DetallePedido AModelo() => new()
    {
        PedidoID = PedidoID,
        ProductoID = ProductoSeleccionado!.ProductoID,
        PrecioUnidad = PrecioUnidad,
        Cantidad = Cantidad,
        Descuento = DescuentoPorcentaje / 100m
    };
}
