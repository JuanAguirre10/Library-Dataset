using NeptunoApp.Models;

namespace NeptunoApp.Tests;

public class PedidoPruebas : PruebaConBaseDatos
{
    // Fechas lejanas para que el reporte solo vea los pedidos creados por la prueba.
    private static readonly DateTime Dia = new(2031, 6, 15);

    private async Task<(Pedido Pedido, Producto Producto)> PedidoConLineaAsync(DateTime fecha)
    {
        var producto = await NuevoProductoAsync();
        var pedido = await NuevoPedidoAsync(fecha);
        await Pedidos.AgregarDetalleAsync(new DetallePedido
        {
            PedidoID = pedido.PedidoID,
            ProductoID = producto.ProductoID,
            PrecioUnidad = 20m,
            Cantidad = 3,
            Descuento = 0.10m
        });
        return (pedido, producto);
    }

    [Fact]
    public Task Crear_devuelve_id_y_calcula_el_total_con_sus_lineas() => EnTransaccionAsync(async () =>
    {
        var (pedido, _) = await PedidoConLineaAsync(Dia);

        Assert.True(pedido.PedidoID > 0);
        var leido = await Pedidos.ObtenerPorIdAsync(pedido.PedidoID);
        Assert.NotNull(leido);
        Assert.True(leido.Activo);
        Assert.Equal(54m, leido.Total); // 20 * 3 * (1 - 0.10)
    });

    [Fact]
    public Task Actualizar_y_validacion_de_fechas() => EnTransaccionAsync(async () =>
    {
        var pedido = await NuevoPedidoAsync(Dia);
        pedido.CiudadDestino = "Trujillo";
        await Pedidos.ActualizarAsync(pedido);
        Assert.Equal("Trujillo", (await Pedidos.ObtenerPorIdAsync(pedido.PedidoID))!.CiudadDestino);

        pedido.FechaRequerida = Dia.AddDays(-1);
        await DebeLanzarAsync(50402, () => Pedidos.ActualizarAsync(pedido));
    });

    [Fact]
    public Task Lineas_agregar_actualizar_y_quitar() => EnTransaccionAsync(async () =>
    {
        var (pedido, producto) = await PedidoConLineaAsync(Dia);

        await Pedidos.ActualizarDetalleAsync(new DetallePedido
        {
            PedidoID = pedido.PedidoID, ProductoID = producto.ProductoID, PrecioUnidad = 20m, Cantidad = 5, Descuento = 0m
        });
        Assert.Equal(100m, Assert.Single(await Pedidos.ListarDetalleAsync(pedido.PedidoID)).Subtotal);

        await DebeLanzarAsync(50503, () => Pedidos.AgregarDetalleAsync(new DetallePedido
        {
            PedidoID = pedido.PedidoID, ProductoID = producto.ProductoID, PrecioUnidad = 1m, Cantidad = 1
        }));

        await Pedidos.EliminarDetalleAsync(pedido.PedidoID, producto.ProductoID);
        Assert.Empty(await Pedidos.ListarDetalleAsync(pedido.PedidoID));
    });

    [Fact]
    public Task Eliminar_es_logico_y_conserva_cabecera_y_lineas() => EnTransaccionAsync(async () =>
    {
        var (pedido, _) = await PedidoConLineaAsync(Dia);

        await Pedidos.EliminarAsync(pedido.PedidoID);

        Assert.False(await ActivoEnTablaAsync("Pedidos", "PedidoID", pedido.PedidoID));
        var lineasEnTabla = (int)(await EscalarAsync(
            "SELECT COUNT(*) FROM dbo.DetallePedidos WHERE PedidoID = @Id", ("@Id", pedido.PedidoID)))!;
        Assert.Equal(1, lineasEnTabla);

        Assert.DoesNotContain(await Pedidos.ListarAsync(), p => p.PedidoID == pedido.PedidoID);
        Assert.Null(await Pedidos.ObtenerPorIdAsync(pedido.PedidoID));
        await DebeLanzarAsync(50404, () => Pedidos.EliminarAsync(pedido.PedidoID));
        await DebeLanzarAsync(50403, () => Pedidos.ActualizarAsync(pedido));
    });

    [Fact]
    public Task Pedido_dado_de_baja_no_admite_cambios_en_sus_lineas() => EnTransaccionAsync(async () =>
    {
        var (pedido, producto) = await PedidoConLineaAsync(Dia);
        var otro = await NuevoProductoAsync();
        await Pedidos.EliminarAsync(pedido.PedidoID);

        await DebeLanzarAsync(50508, () => Pedidos.AgregarDetalleAsync(new DetallePedido
        {
            PedidoID = pedido.PedidoID, ProductoID = otro.ProductoID, PrecioUnidad = 1m, Cantidad = 1
        }));
        await DebeLanzarAsync(50508, () => Pedidos.ActualizarDetalleAsync(new DetallePedido
        {
            PedidoID = pedido.PedidoID, ProductoID = producto.ProductoID, PrecioUnidad = 1m, Cantidad = 1
        }));
        await DebeLanzarAsync(50508, () => Pedidos.EliminarDetalleAsync(pedido.PedidoID, producto.ProductoID));
    });

    [Fact]
    public Task No_se_agrega_un_producto_dado_de_baja() => EnTransaccionAsync(async () =>
    {
        var pedido = await NuevoPedidoAsync(Dia);
        var producto = await NuevoProductoAsync();
        await Productos.EliminarAsync(producto.ProductoID);

        await DebeLanzarAsync(50509, () => Pedidos.AgregarDetalleAsync(new DetallePedido
        {
            PedidoID = pedido.PedidoID, ProductoID = producto.ProductoID, PrecioUnidad = 1m, Cantidad = 1
        }));
    });

    [Fact]
    public Task Reporte_filtra_por_fechas_y_excluye_pedidos_dados_de_baja() => EnTransaccionAsync(async () =>
    {
        var (dentro, _) = await PedidoConLineaAsync(Dia);
        var (eliminado, _) = await PedidoConLineaAsync(Dia.AddDays(1));
        var (fuera, _) = await PedidoConLineaAsync(Dia.AddDays(30));

        var antes = await Pedidos.ListarPorRangoFechasAsync(Dia, Dia.AddDays(2));
        Assert.Equal(
            new[] { dentro.PedidoID, eliminado.PedidoID },
            antes.Select(l => l.PedidoID).OrderBy(id => id));

        await Pedidos.EliminarAsync(eliminado.PedidoID);

        var despues = await Pedidos.ListarPorRangoFechasAsync(Dia, Dia.AddDays(2));
        var linea = Assert.Single(despues);
        Assert.Equal(dentro.PedidoID, linea.PedidoID);
        Assert.Equal(54m, linea.Subtotal);
        Assert.DoesNotContain(despues, l => l.PedidoID == fuera.PedidoID);
    });

    [Fact]
    public Task Reporte_rechaza_rango_invertido() => EnTransaccionAsync(async () =>
    {
        await DebeLanzarAsync(50601, () => Pedidos.ListarPorRangoFechasAsync(Dia, Dia.AddDays(-1)));
    });

    [Fact]
    public Task Catalogos_de_los_combos_cargan() => EnTransaccionAsync(async () =>
    {
        Assert.NotEmpty(await Catalogos.ListarClientesAsync());
        Assert.NotEmpty(await Catalogos.ListarEmpleadosAsync());
        Assert.NotEmpty(await Catalogos.ListarTransportistasAsync());
    });
}
