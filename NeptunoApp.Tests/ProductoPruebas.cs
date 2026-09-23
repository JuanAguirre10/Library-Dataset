namespace NeptunoApp.Tests;

public class ProductoPruebas : PruebaConBaseDatos
{
    [Fact]
    public Task Crear_devuelve_id_y_trae_nombres_de_categoria_y_proveedor() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();
        var proveedor = await NuevoProveedorAsync();

        var producto = await NuevoProductoAsync(categoria.CategoriaID, proveedor.ProveedorID);

        var leido = await Productos.ObtenerPorIdAsync(producto.ProductoID);
        Assert.NotNull(leido);
        Assert.True(leido.Activo);
        Assert.Equal(categoria.NombreCategoria, leido.NombreCategoria);
        Assert.Equal(proveedor.CompaniaNombre, leido.NombreProveedor);
        Assert.Equal(10.50m, leido.PrecioUnidad);
    });

    [Fact]
    public Task Crear_sin_categoria_ni_proveedor_acepta_nulos() => EnTransaccionAsync(async () =>
    {
        var producto = await NuevoProductoAsync();

        var leido = await Productos.ObtenerPorIdAsync(producto.ProductoID);
        Assert.Null(leido!.CategoriaID);
        Assert.Null(leido.ProveedorID);
    });

    [Fact]
    public Task Actualizar_modifica_el_registro() => EnTransaccionAsync(async () =>
    {
        var producto = await NuevoProductoAsync();
        producto.PrecioUnidad = 99.99m;
        producto.Descontinuado = true;

        await Productos.ActualizarAsync(producto);

        var leido = await Productos.ObtenerPorIdAsync(producto.ProductoID);
        Assert.Equal(99.99m, leido!.PrecioUnidad);
        Assert.True(leido.Descontinuado);
    });

    [Fact]
    public Task Precio_negativo_se_rechaza() => EnTransaccionAsync(async () =>
    {
        var producto = await NuevoProductoAsync();
        producto.PrecioUnidad = -1;

        await DebeLanzarAsync(50302, () => Productos.ActualizarAsync(producto));
    });

    [Fact]
    public Task Eliminar_es_logico_y_sale_del_listado() => EnTransaccionAsync(async () =>
    {
        var producto = await NuevoProductoAsync();
        Assert.Contains(await Productos.ListarAsync(), p => p.ProductoID == producto.ProductoID);

        await Productos.EliminarAsync(producto.ProductoID);

        Assert.False(await ActivoEnTablaAsync("Productos", "ProductoID", producto.ProductoID));
        Assert.DoesNotContain(await Productos.ListarAsync(), p => p.ProductoID == producto.ProductoID);
        Assert.Null(await Productos.ObtenerPorIdAsync(producto.ProductoID));
        await DebeLanzarAsync(50305, () => Productos.EliminarAsync(producto.ProductoID));
        await DebeLanzarAsync(50303, () => Productos.ActualizarAsync(producto));
    });

    [Fact]
    public Task No_acepta_categoria_o_proveedor_dados_de_baja() => EnTransaccionAsync(async () =>
    {
        var categoria = await NuevaCategoriaAsync();
        var proveedor = await NuevoProveedorAsync();
        await Categorias.EliminarAsync(categoria.CategoriaID);
        await Proveedores.EliminarAsync(proveedor.ProveedorID);

        await DebeLanzarAsync(50306, () => NuevoProductoAsync(categoriaId: categoria.CategoriaID));
        await DebeLanzarAsync(50307, () => NuevoProductoAsync(proveedorId: proveedor.ProveedorID));

        var existente = await NuevoProductoAsync();
        existente.CategoriaID = categoria.CategoriaID;
        await DebeLanzarAsync(50306, () => Productos.ActualizarAsync(existente));
    });

    [Fact]
    public Task Producto_que_figura_en_pedidos_se_puede_dar_de_baja_y_la_linea_se_conserva() => EnTransaccionAsync(async () =>
    {
        var producto = await NuevoProductoAsync();
        var pedido = await NuevoPedidoAsync(new DateTime(2031, 3, 10));
        await Pedidos.AgregarDetalleAsync(new() { PedidoID = pedido.PedidoID, ProductoID = producto.ProductoID, PrecioUnidad = 10.50m, Cantidad = 2 });

        await Productos.EliminarAsync(producto.ProductoID);

        var linea = Assert.Single(await Pedidos.ListarDetalleAsync(pedido.PedidoID));
        Assert.Equal(producto.NombreProducto, linea.NombreProducto);
        var reporte = await Pedidos.ListarPorRangoFechasAsync(new DateTime(2031, 3, 10), new DateTime(2031, 3, 10));
        Assert.Contains(reporte, l => l.ProductoID == producto.ProductoID);
    });
}
