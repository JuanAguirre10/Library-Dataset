namespace NeptunoApp.Tests;

/// <summary>
/// Cadena de conexion que usan las pruebas de integracion.
///
/// No se reutiliza NeptunoApp.Configuracion.DbConfig a proposito: esa clase lee
/// el App.config del proyecto de inicio, y en una corrida de pruebas el proceso
/// que arranca es el host de pruebas, no NeptunoApp.exe, asi que
/// ConfigurationManager no encontraria la entrada. El arnes de pruebas declara
/// entonces su propia cadena, que puede sobrescribirse con la variable de
/// entorno NEPTUNO_CONEXION para apuntar a otra instancia o base de datos.
/// </summary>
internal static class ConfiguracionPruebas
{
    private const string PorDefecto =
        @"Server=.\SQLEXPRESS;Database=NeptunoDB_Lab06;Trusted_Connection=True;TrustServerCertificate=True;";

    public static string Cadena =>
        Environment.GetEnvironmentVariable("NEPTUNO_CONEXION") is { Length: > 0 } cadena
            ? cadena
            : PorDefecto;
}
