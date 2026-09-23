using NeptunoApp.Models;
using NeptunoApp.ViewModels;

namespace NeptunoApp.Tests;

/// <summary>
/// Requisitos transversales del laboratorio verificados sobre la base de datos y
/// comportamiento de los ViewModels que no necesita ventanas.
/// </summary>
public class ReglasGeneralesPruebas : PruebaConBaseDatos
{
    [Theory]
    [InlineData("Categorias")]
    [InlineData("Proveedores")]
    [InlineData("Productos")]
    [InlineData("Pedidos")]
    public async Task Tabla_tiene_Activo_bit_no_nulo_con_default_1(string tabla)
    {
        var definicion = await EscalarAsync("""
            SELECT CONCAT(ty.name, '|', c.is_nullable, '|', dc.definition)
            FROM sys.columns c
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            LEFT JOIN sys.default_constraints dc ON dc.object_id = c.default_object_id
            WHERE c.object_id = OBJECT_ID(@Tabla) AND c.name = 'Activo'
            """, ("@Tabla", "dbo." + tabla));

        Assert.Equal("bit|0|((1))", definicion);
    }

    [Theory]
    [InlineData("usp_Categoria_Eliminar")]
    [InlineData("usp_Proveedor_Eliminar")]
    [InlineData("usp_Producto_Eliminar")]
    [InlineData("usp_Pedido_Eliminar")]
    public async Task Procedimiento_de_eliminar_no_hace_DELETE_fisico(string procedimiento)
    {
        var definicion = (string)(await EscalarAsync(
            "SELECT OBJECT_DEFINITION(OBJECT_ID(@Proc))", ("@Proc", "dbo." + procedimiento)))!;

        Assert.DoesNotContain("DELETE", definicion, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SET Activo = 0", definicion);
    }

    [Theory]
    [InlineData("usp_Categoria_Crear", "@CategoriaID")]
    [InlineData("usp_Proveedor_Crear", "@ProveedorID")]
    [InlineData("usp_Producto_Crear", "@ProductoID")]
    [InlineData("usp_Pedido_Crear", "@PedidoID")]
    public async Task Procedimiento_de_alta_devuelve_el_id_por_OUTPUT_y_no_con_SELECT(string procedimiento, string parametro)
    {
        var esSalida = await EscalarAsync(
            "SELECT is_output FROM sys.parameters WHERE object_id = OBJECT_ID(@Proc) AND name = @Param",
            ("@Proc", "dbo." + procedimiento), ("@Param", parametro));
        var definicion = (string)(await EscalarAsync(
            "SELECT OBJECT_DEFINITION(OBJECT_ID(@Proc))", ("@Proc", "dbo." + procedimiento)))!;

        Assert.True(esSalida is true);
        Assert.DoesNotContain("SELECT SCOPE_IDENTITY", definicion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public Task CategoriasViewModel_carga_y_guarda_sin_mostrar_inactivas() => EnTransaccionAsync(async () =>
    {
        var eliminada = await NuevaCategoriaAsync();
        await Categorias.EliminarAsync(eliminada.CategoriaID);
        var vm = new CategoriasViewModel(Categorias);

        await vm.GuardarCommand.ExecuteAsync(new CategoriaEditViewModel { NombreCategoria = "Desde el ViewModel" });

        Assert.Null(vm.ErrorMessage);
        Assert.Contains(vm.Categorias, c => c.NombreCategoria == "Desde el ViewModel");
        Assert.DoesNotContain(vm.Categorias, c => c.CategoriaID == eliminada.CategoriaID);
    });

    [Fact]
    public Task Error_del_procedimiento_llega_a_ErrorMessage_del_ViewModel() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();
        await Categorias.EliminarAsync(categoria.CategoriaID);
        var vm = new CategoriasViewModel(Categorias);

        // Editar una categoria dada de baja: el UPDATE no afecta filas y el procedimiento hace THROW.
        await vm.GuardarCommand.ExecuteAsync(new CategoriaEditViewModel(categoria) { NombreCategoria = "X" });

        Assert.NotNull(vm.ErrorMessage);
        Assert.Contains("no existe o fue eliminada", vm.ErrorMessage);
        Assert.False(vm.IsBusy);
    });

    [Fact]
    public Task ProveedoresViewModel_busca_con_los_filtros() => EnTransaccionAsync(async () =>
    {
        var proveedor = await NuevoProveedorAsync(contacto: "Filtro VM ZZTEST", ciudad: "Ica ZZTEST");
        var vm = new ProveedoresViewModel(Proveedores)
        {
            FiltroNombreContacto = "Filtro VM ZZTEST",
            FiltroCiudad = "Ica ZZTEST"
        };

        await vm.BuscarCommand.ExecuteAsync(null);

        Assert.Equal(proveedor.ProveedorID, Assert.Single(vm.Proveedores).ProveedorID);
        Assert.True(vm.FiltrosAplicados);
    });

    [Fact]
    public Task ReportesViewModel_suma_los_totales_del_intervalo() => EnTransaccionAsync(async () =>
    {
        var dia = new DateTime(2032, 2, 2);
        var producto = await NuevoProductoAsync();
        var pedido = await NuevoPedidoAsync(dia);
        await Pedidos.AgregarDetalleAsync(new DetallePedido
        {
            PedidoID = pedido.PedidoID, ProductoID = producto.ProductoID, PrecioUnidad = 12.5m, Cantidad = 4
        });
        var vm = new ReportesViewModel(Pedidos) { FechaInicio = dia, FechaFin = dia };

        await vm.GenerarCommand.ExecuteAsync(null);

        Assert.Null(vm.ErrorMessage);
        Assert.Single(vm.Lineas);
        Assert.Equal(50m, vm.TotalGeneral);
        Assert.Equal(4, vm.TotalUnidades);
    });

    [Fact]
    public void Editar_linea_con_producto_dado_de_baja_lo_muestra_y_permite_guardar()
    {
        var activos = new[] { new Producto { ProductoID = 1, NombreProducto = "Activo" } };
        var linea = new DetallePedido { PedidoID = 7, ProductoID = 99, NombreProducto = "Viejo", PrecioUnidad = 5m, Cantidad = 2 };

        var vm = new DetalleEditViewModel(7, activos, linea);

        Assert.NotNull(vm.ProductoSeleccionado);
        Assert.Equal(99, vm.ProductoSeleccionado.ProductoID);
        Assert.Contains("dado de baja", vm.ProductoSeleccionado.NombreProducto);
        Assert.Null(vm.Validar());
        Assert.Equal(99, vm.AModelo().ProductoID);
    }
}
