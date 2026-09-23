using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeptunoApp.Data;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

public partial class PedidosViewModel : ViewModelBase
{
    private readonly IPedidoRepository _repositorio;
    private readonly ICatalogoRepository _catalogoRepositorio;
    private readonly IProductoRepository _productoRepositorio;

    public ObservableCollection<Pedido> Pedidos { get; } = new();
    public ObservableCollection<DetallePedido> Detalle { get; } = new();

    [ObservableProperty]
    private Pedido? seleccionado;

    [ObservableProperty]
    private DetallePedido? detalleSeleccionado;

    [ObservableProperty]
    private bool hayRegistros;

    public event Action<PedidoEditViewModel>? EdicionSolicitada;
    public event Action<DetalleEditViewModel>? EdicionDetalleSolicitada;

    public PedidosViewModel(
        IPedidoRepository repositorio,
        ICatalogoRepository catalogoRepositorio,
        IProductoRepository productoRepositorio)
    {
        _repositorio = repositorio;
        _catalogoRepositorio = catalogoRepositorio;
        _productoRepositorio = productoRepositorio;
    }

    // Identifica cada carga de detalle para descartar las que quedaron obsoletas
    // cuando la seleccion cambia mientras la consulta anterior sigue en curso.
    private int _generacionDetalle;

    /// <summary>Al cambiar el pedido seleccionado se recarga su detalle.</summary>
    partial void OnSeleccionadoChanged(Pedido? value) => _ = CargarDetalleAsync();

    [RelayCommand]
    private Task CargarAsync() => EjecutarAsync(async () =>
    {
        var pedidoActivo = Seleccionado?.PedidoID;
        var resultado = await _repositorio.ListarAsync();

        Pedidos.Clear();
        foreach (var pedido in resultado)
        {
            Pedidos.Add(pedido);
        }
        HayRegistros = Pedidos.Count > 0;

        // Se conserva la seleccion previa para que el detalle no se pierda al refrescar.
        Seleccionado = Pedidos.FirstOrDefault(p => p.PedidoID == pedidoActivo) ?? Pedidos.FirstOrDefault();
    }, "No se pudo cargar el listado de pedidos");

    private Task CargarDetalleAsync() => EjecutarAsync(async () =>
    {
        var generacion = ++_generacionDetalle;
        var pedidoId = Seleccionado?.PedidoID;

        var lineas = pedidoId is null
            ? new List<DetallePedido>()
            : await _repositorio.ListarDetalleAsync(pedidoId.Value);

        // Si mientras se consultaba se pidio otra carga, esta ya no vale.
        if (generacion != _generacionDetalle) return;

        Detalle.Clear();
        foreach (var linea in lineas)
        {
            Detalle.Add(linea);
        }
    }, "No se pudo cargar el detalle del pedido");

    [RelayCommand]
    private Task NuevoAsync() => EjecutarAsync(async () =>
    {
        var (clientes, empleados, transportistas) = await CargarCatalogosAsync();
        EdicionSolicitada?.Invoke(new PedidoEditViewModel(clientes, empleados, transportistas));
    }, "No se pudo abrir el formulario");

    [RelayCommand]
    private Task EditarAsync(Pedido? pedido) => EjecutarAsync(async () =>
    {
        if (pedido is null) return;

        var actual = await _repositorio.ObtenerPorIdAsync(pedido.PedidoID);
        if (actual is null)
        {
            await CargarAsync();
            ErrorMessage = "El pedido ya no existe.";
            return;
        }

        var (clientes, empleados, transportistas) = await CargarCatalogosAsync();
        EdicionSolicitada?.Invoke(new PedidoEditViewModel(clientes, empleados, transportistas, actual));
    }, "No se pudo abrir el pedido");

    [RelayCommand]
    private Task GuardarAsync(PedidoEditViewModel? edicion) => EjecutarAsync(async () =>
    {
        if (edicion is null) return;

        var pedido = edicion.AModelo();
        if (edicion.EsNuevo)
        {
            var nuevoId = await _repositorio.CrearAsync(pedido);
            await CargarAsync();
            Seleccionado = Pedidos.FirstOrDefault(p => p.PedidoID == nuevoId);
        }
        else
        {
            await _repositorio.ActualizarAsync(pedido);
            await CargarAsync();
        }
    }, "No se pudo guardar el pedido");

    [RelayCommand]
    private Task EliminarAsync(Pedido? pedido) => EjecutarAsync(async () =>
    {
        if (pedido is null) return;
        if (!Confirmar($"¿Eliminar el pedido #{pedido.PedidoID}?\n\nEs una eliminación lógica: el pedido se marca como inactivo (Activo = 0), sale de los listados y reportes, y tanto la cabecera como sus líneas se conservan en la base de datos.")) return;

        await _repositorio.EliminarAsync(pedido.PedidoID);
        await CargarAsync();
    }, "No se pudo eliminar el pedido");

    [RelayCommand]
    private Task AgregarDetalleAsync() => EjecutarAsync(async () =>
    {
        if (Seleccionado is null)
        {
            ErrorMessage = "Seleccione primero un pedido.";
            return;
        }

        var productos = await _productoRepositorio.ListarAsync();
        EdicionDetalleSolicitada?.Invoke(new DetalleEditViewModel(Seleccionado.PedidoID, productos));
    }, "No se pudo abrir el formulario de la línea");

    [RelayCommand]
    private Task EditarDetalleAsync(DetallePedido? detalle) => EjecutarAsync(async () =>
    {
        if (detalle is null) return;

        var productos = await _productoRepositorio.ListarAsync();
        EdicionDetalleSolicitada?.Invoke(new DetalleEditViewModel(detalle.PedidoID, productos, detalle));
    }, "No se pudo abrir la línea");

    [RelayCommand]
    private Task GuardarDetalleAsync(DetalleEditViewModel? edicion) => EjecutarAsync(async () =>
    {
        if (edicion is null) return;

        var detalle = edicion.AModelo();
        if (edicion.EsNueva)
        {
            await _repositorio.AgregarDetalleAsync(detalle);
        }
        else
        {
            await _repositorio.ActualizarDetalleAsync(detalle);
        }

        await RefrescarPedidoYDetalleAsync();
    }, "No se pudo guardar la línea del pedido");

    [RelayCommand]
    private Task EliminarDetalleAsync(DetallePedido? detalle) => EjecutarAsync(async () =>
    {
        if (detalle is null) return;
        if (!Confirmar($"¿Quitar \"{detalle.NombreProducto}\" del pedido?")) return;

        await _repositorio.EliminarDetalleAsync(detalle.PedidoID, detalle.ProductoID);
        await RefrescarPedidoYDetalleAsync();
    }, "No se pudo quitar la línea del pedido");

    /// <summary>
    /// Refresca la cabecera para recalcular el total. Al reasignarse la seleccion,
    /// OnSeleccionadoChanged vuelve a cargar las lineas.
    /// </summary>
    private Task RefrescarPedidoYDetalleAsync() => CargarAsync();

    private async Task<(List<Cliente> Clientes, List<Empleado> Empleados, List<Transportista> Transportistas)>
        CargarCatalogosAsync()
    {
        var clientes = await _catalogoRepositorio.ListarClientesAsync();
        var empleados = await _catalogoRepositorio.ListarEmpleadosAsync();
        var transportistas = await _catalogoRepositorio.ListarTransportistasAsync();
        return (clientes, empleados, transportistas);
    }
}
