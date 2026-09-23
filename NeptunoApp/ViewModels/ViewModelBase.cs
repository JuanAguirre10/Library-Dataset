using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NeptunoApp.ViewModels;

/// <summary>
/// Comportamiento compartido por los ViewModels de mantenimiento: indicador de
/// ocupado, mensaje de error y el envoltorio que evita repetir el try/catch en
/// cada comando.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Ejecuta la operacion controlando el estado de ocupado y capturando los
    /// errores que devuelven los procedimientos almacenados.
    /// </summary>
    protected async Task EjecutarAsync(Func<Task> operacion, string mensajeError)
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await operacion();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{mensajeError}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected static bool Confirmar(string mensaje)
        => MessageBox.Show(mensaje, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question)
           == MessageBoxResult.Yes;
}
