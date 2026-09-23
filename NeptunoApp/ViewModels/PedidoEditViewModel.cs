using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

/// <summary>Edicion de la cabecera del pedido; las lineas se administran en la vista principal.</summary>
public partial class PedidoEditViewModel : ObservableObject
{
    public int PedidoID { get; }

    public ObservableCollection<Cliente> Clientes { get; }
    public ObservableCollection<Empleado> Empleados { get; }
    public ObservableCollection<Transportista> Transportistas { get; }

    [ObservableProperty]
    private Cliente? clienteSeleccionado;

    [ObservableProperty]
    private Empleado? empleadoSeleccionado;

    [ObservableProperty]
    private Transportista? transportistaSeleccionado;

    [ObservableProperty]
    private DateTime fechaPedido;

    [ObservableProperty]
    private DateTime? fechaRequerida;

    [ObservableProperty]
    private DateTime? fechaEnvio;

    [ObservableProperty]
    private string? destinatario;

    [ObservableProperty]
    private string? ciudadDestino;

    [ObservableProperty]
    private string? paisDestino;

    public bool EsNuevo => PedidoID == 0;
    public string TituloVentana => EsNuevo ? "Nuevo pedido" : $"Editar pedido #{PedidoID}";

    public PedidoEditViewModel(
        IEnumerable<Cliente> clientes,
        IEnumerable<Empleado> empleados,
        IEnumerable<Transportista> transportistas,
        Pedido? pedido = null)
    {
        Clientes = new ObservableCollection<Cliente>(clientes);
        Empleados = new ObservableCollection<Empleado>(empleados);
        Transportistas = new ObservableCollection<Transportista>(transportistas);

        PedidoID = pedido?.PedidoID ?? 0;
        fechaPedido = pedido?.FechaPedido ?? DateTime.Today;
        fechaRequerida = pedido?.FechaRequerida;
        fechaEnvio = pedido?.FechaEnvio;
        destinatario = pedido?.Destinatario;
        ciudadDestino = pedido?.CiudadDestino;
        paisDestino = pedido?.PaisDestino;

        clienteSeleccionado = Clientes.FirstOrDefault(c => c.ClienteID == pedido?.ClienteID);
        empleadoSeleccionado = Empleados.FirstOrDefault(e => e.EmpleadoID == pedido?.EmpleadoID);
        transportistaSeleccionado = Transportistas.FirstOrDefault(t => t.TransportistaID == pedido?.TransportistaID);
    }

    public string? Validar()
    {
        if (FechaRequerida.HasValue && FechaRequerida.Value.Date < FechaPedido.Date)
            return "La fecha requerida no puede ser anterior a la fecha del pedido.";

        if (FechaEnvio.HasValue && FechaEnvio.Value.Date < FechaPedido.Date)
            return "La fecha de envio no puede ser anterior a la fecha del pedido.";

        return null;
    }

    public Pedido AModelo() => new()
    {
        PedidoID = PedidoID,
        ClienteID = ClienteSeleccionado?.ClienteID,
        EmpleadoID = EmpleadoSeleccionado?.EmpleadoID,
        FechaPedido = FechaPedido,
        FechaRequerida = FechaRequerida,
        FechaEnvio = FechaEnvio,
        TransportistaID = TransportistaSeleccionado?.TransportistaID,
        Destinatario = Limpiar(Destinatario),
        CiudadDestino = Limpiar(CiudadDestino),
        PaisDestino = Limpiar(PaisDestino)
    };

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
