namespace NeptunoApp.Tests;

public class CategoriaPruebas : PruebaConBaseDatos
{
    [Fact]
    public Task Crear_con_ExecuteNonQuery_devuelve_el_id_del_parametro_OUTPUT() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();

        Assert.True(categoria.CategoriaID > 0);
        var leida = await Categorias.ObtenerPorIdAsync(categoria.CategoriaID);
        Assert.NotNull(leida);
        Assert.Equal(categoria.NombreCategoria, leida.NombreCategoria);
        Assert.True(leida.Activo);
    });

    [Fact]
    public Task Registro_nuevo_nace_activo_por_el_DEFAULT() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();

        Assert.True(await ActivoEnTablaAsync("Categorias", "CategoriaID", categoria.CategoriaID));
    });

    [Fact]
    public Task Actualizar_modifica_el_registro() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();
        categoria.NombreCategoria = "Renombrada";
        categoria.Descripcion = null;

        await Categorias.ActualizarAsync(categoria);

        var leida = await Categorias.ObtenerPorIdAsync(categoria.CategoriaID);
        Assert.Equal("Renombrada", leida!.NombreCategoria);
        Assert.Null(leida.Descripcion);
    });

    [Fact]
    public Task Eliminar_es_logico_la_fila_se_conserva_con_Activo_0() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();

        await Categorias.EliminarAsync(categoria.CategoriaID);

        Assert.False(await ActivoEnTablaAsync("Categorias", "CategoriaID", categoria.CategoriaID));
        Assert.Null(await Categorias.ObtenerPorIdAsync(categoria.CategoriaID));
        Assert.DoesNotContain(await Categorias.ListarAsync(), c => c.CategoriaID == categoria.CategoriaID);
    });

    [Fact]
    public Task Listar_solo_devuelve_activas() => EnTransaccionAsync(async () =>
    {
        await NuevaCategoriaAsync();
        var eliminada = await NuevaCategoriaAsync();
        await Categorias.EliminarAsync(eliminada.CategoriaID);

        var lista = await Categorias.ListarAsync();

        Assert.All(lista, c => Assert.True(c.Activo));
        var activasEnTabla = (int)(await EscalarAsync("SELECT COUNT(*) FROM dbo.Categorias WHERE Activo = 1"))!;
        Assert.Equal(activasEnTabla, lista.Count);
    });

    [Fact]
    public Task Eliminar_dos_veces_informa_que_ya_fue_eliminada() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();
        await Categorias.EliminarAsync(categoria.CategoriaID);

        var mensaje = await DebeLanzarAsync(50103, () => Categorias.EliminarAsync(categoria.CategoriaID));
        Assert.Contains("ya fue eliminada", mensaje);
    });

    [Fact]
    public Task Actualizar_una_categoria_eliminada_falla() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();
        await Categorias.EliminarAsync(categoria.CategoriaID);

        categoria.NombreCategoria = "No deberia";
        await DebeLanzarAsync(50101, () => Categorias.ActualizarAsync(categoria));
    });

    [Fact]
    public Task No_se_elimina_con_productos_activos_pero_si_cuando_estan_de_baja() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();
        var producto = await NuevoProductoAsync(categoriaId: categoria.CategoriaID);

        await DebeLanzarAsync(50102, () => Categorias.EliminarAsync(categoria.CategoriaID));
        Assert.True(await ActivoEnTablaAsync("Categorias", "CategoriaID", categoria.CategoriaID));

        await Productos.EliminarAsync(producto.ProductoID);
        await Categorias.EliminarAsync(categoria.CategoriaID);

        Assert.False(await ActivoEnTablaAsync("Categorias", "CategoriaID", categoria.CategoriaID));
    });

    [Fact]
    public async Task La_transaccion_de_prueba_no_deja_datos_en_la_base()
    {
        var id = 0;
        await EnTransaccionAsync(async () => id = (await NuevaCategoriaAsync()).CategoriaID);

        Assert.True(id > 0);
        Assert.Null(await ActivoEnTablaAsync("Categorias", "CategoriaID", id));
    }
}
