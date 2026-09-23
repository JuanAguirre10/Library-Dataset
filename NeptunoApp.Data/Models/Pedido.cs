using CommunityToolkit.Mvvm.ComponentModel;

namespace NeptunoApp.Models;

public partial class Pedido : ObservableObject
{
    [ObservableProperty]
    private int pedidoID;

    [ObservableProperty]
    private int? clienteID;

    [ObservableProperty]
    private int? empleadoID;

    [ObservableProperty]
    private DateTime fechaPedido = DateTime.Today;

    [ObservableProperty]
    private DateTime? fechaRequerida;

    [ObservableProperty]
    private DateTime? fechaEnvio;

    [ObservableProperty]
    private int? transportistaID;

    [ObservableProperty]
    private string? destinatario;

    [ObservableProperty]
    private string? ciudadDestino;

    [ObservableProperty]
    private string? paisDestino;

    /// <summary>Estado para la eliminacion logica: false cuando el registro fue dado de baja.</summary>
    [ObservableProperty]
    private bool activo = true;

    // Columnas calculadas por el procedimiento para la grilla.
    [ObservableProperty]
    private string? nombreCliente;

    [ObservableProperty]
    private string? nombreEmpleado;

    [ObservableProperty]
    private string? nombreTransportista;

    [ObservableProperty]
    private decimal total;

    public bool Enviado => FechaEnvio.HasValue;
}
