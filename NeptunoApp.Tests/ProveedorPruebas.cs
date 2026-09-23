namespace NeptunoApp.Tests;

public class ProveedorPruebas : PruebaConBaseDatos
{
    [Fact]
    public Task Crear_devuelve_id_y_queda_activo() => EnTransaccionAsync(async () =>
    {
        var proveedor = await NuevoProveedorAsync();

        Assert.True(proveedor.ProveedorID > 0);
        Assert.True(await ActivoEnTablaAsync("Proveedores", "ProveedorID", proveedor.ProveedorID));
    });

    [Fact]
    public Task Actualizar_modifica_el_registro() => EnTransaccionAsync(async () =>
    {
        var proveedor = await NuevoProveedorAsync();
        proveedor.Telefono = "999-111-222";

        await Proveedores.ActualizarAsync(proveedor);

        Assert.Equal("999-111-222", (await Proveedores.ObtenerPorIdAsync(proveedor.ProveedorID))!.Telefono);
    });

    [Fact]
    public Task Buscar_filtra_por_nombreContacto_y_ciudad() => EnTransaccionAsync(async () =>
    {
        var buscado = await NuevoProveedorAsync(contacto: "Rosa Quispe ZZTEST", ciudad: "Huaraz ZZTEST");
        await NuevoProveedorAsync(contacto: "Rosa Quispe ZZTEST", ciudad: "Tacna ZZTEST");
        await NuevoProveedorAsync(contacto: "Otro ZZTEST", ciudad: "Huaraz ZZTEST");

        var porAmbos = await Proveedores.BuscarAsync("Quispe ZZTEST", "Huaraz ZZTEST");
        var soloContacto = await Proveedores.BuscarAsync("Quispe ZZTEST", null);
        var soloCiudad = await Proveedores.BuscarAsync("", "Huaraz ZZTEST");

        Assert.Equal(buscado.ProveedorID, Assert.Single(porAmbos).ProveedorID);
        Assert.Equal(2, soloContacto.Count);
        Assert.Equal(2, soloCiudad.Count);
    });

    [Fact]
    public Task Buscar_sin_filtros_equivale_al_listado_de_activos() => EnTransaccionAsync(async () =>
    {
        var eliminado = await NuevoProveedorAsync();
        await Proveedores.EliminarAsync(eliminado.ProveedorID);

        var busqueda = await Proveedores.BuscarAsync(null, null);
        var listado = await Proveedores.ListarAsync();

        Assert.Equal(listado.Select(p => p.ProveedorID), busqueda.Select(p => p.ProveedorID));
        Assert.DoesNotContain(busqueda, p => p.ProveedorID == eliminado.ProveedorID);
    });

    [Fact]
    public Task Buscar_no_devuelve_proveedores_dados_de_baja() => EnTransaccionAsync(async () =>
    {
        var proveedor = await NuevoProveedorAsync(contacto: "Baja ZZTEST", ciudad: "Puno ZZTEST");
        Assert.Single(await Proveedores.BuscarAsync("Baja ZZTEST", "Puno ZZTEST"));

        await Proveedores.EliminarAsync(proveedor.ProveedorID);

        Assert.Empty(await Proveedores.BuscarAsync("Baja ZZTEST", "Puno ZZTEST"));
        Assert.False(await ActivoEnTablaAsync("Proveedores", "ProveedorID", proveedor.ProveedorID));
    });

    [Fact]
    public Task No_se_elimina_con_productos_activos() => EnTransaccionAsync(async () =>
    {
        var proveedor = await NuevoProveedorAsync();
        await NuevoProductoAsync(proveedorId: proveedor.ProveedorID);

        await DebeLanzarAsync(50202, () => Proveedores.EliminarAsync(proveedor.ProveedorID));
    });

    [Fact]
    public Task Eliminar_dos_veces_y_actualizar_eliminado_fallan() => EnTransaccionAsync(async () =>
    {
        var proveedor = await NuevoProveedorAsync();
        await Proveedores.EliminarAsync(proveedor.ProveedorID);

        await DebeLanzarAsync(50203, () => Proveedores.EliminarAsync(proveedor.ProveedorID));
        await DebeLanzarAsync(50201, () => Proveedores.ActualizarAsync(proveedor));
    });
}
