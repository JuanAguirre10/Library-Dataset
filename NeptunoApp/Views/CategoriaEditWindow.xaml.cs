using System.Windows;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Views;

public partial class CategoriaEditWindow : Window
{
    public CategoriaEditWindow(CategoriaEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => NombreBox.Focus();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = (CategoriaEditViewModel)DataContext;
        var error = viewModel.Validar();
        if (error is not null)
        {
            MessageBox.Show(this, error, "Datos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
