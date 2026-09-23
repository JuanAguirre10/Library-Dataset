using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeptunoApp.Data;
using NeptunoApp.Models;

namespace NeptunoApp.ViewModels;

/// <summary>
/// Reporte de detalles de pedidos filtrado por intervalo de fechas
/// (procedimiento usp_DetallePedido_ListarPorRangoFechas).
/// </summary>
public partial class ReportesViewModel : ViewModelBase
{
    private readonly IPedidoRepository _repositorio;

    public ObservableCollection<LineaReporte> Lineas { get; } = new();

    [ObservableProperty]
    private DateTime fechaInicio = new DateTime(DateTime.Today.Year, 1, 1);

    [ObservableProperty]
    private DateTime fechaFin = DateTime.Today;

    [ObservableProperty]
    private bool hayRegistros;

    [ObservableProperty]
    private bool consultaRealizada;

    [ObservableProperty]
    private decimal totalGeneral;

    [ObservableProperty]
    private int totalUnidades;

    public ReportesViewModel(IPedidoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    [RelayCommand]
    private Task GenerarAsync() => EjecutarAsync(async () =>
    {
        if (FechaInicio.Date > FechaFin.Date)
        {
            ErrorMessage = "La fecha inicial no puede ser mayor que la fecha final.";
            return;
        }

        var resultado = await _repositorio.ListarPorRangoFechasAsync(FechaInicio, FechaFin);

        Lineas.Clear();
        foreach (var linea in resultado)
        {
            Lineas.Add(linea);
        }

        HayRegistros = Lineas.Count > 0;
        ConsultaRealizada = true;
        TotalGeneral = Lineas.Sum(l => l.Subtotal);
        TotalUnidades = Lineas.Sum(l => (int)l.Cantidad);
    }, "No se pudo generar el reporte");

    [RelayCommand]
    private void EstablecerMesActual()
    {
        var hoy = DateTime.Today;
        FechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
        FechaFin = FechaInicio.AddMonths(1).AddDays(-1);
    }

    [RelayCommand]
    private void EstablecerAnioActual()
    {
        var hoy = DateTime.Today;
        FechaInicio = new DateTime(hoy.Year, 1, 1);
        FechaFin = new DateTime(hoy.Year, 12, 31);
    }
}
