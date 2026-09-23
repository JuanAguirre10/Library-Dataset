using System.Data;

namespace NeptunoApp.Data;

/// <summary>
/// Lecturas por nombre de columna sobre las filas del DataSet, para no repetir
/// el casteo y la comprobacion de DBNull en cada mapeo.
///
/// Sustituye a las extensiones sobre SqlDataReader del laboratorio anterior:
/// ahora las consultas se resuelven en modo desconectado y lo que se mapea es
/// un DataRow, no un lector abierto contra la base de datos.
/// </summary>
internal static class FilaExtensiones
{
    public static int Entero(this DataRow fila, string columna)
        => (int)fila[columna];

    public static int? EnteroNulo(this DataRow fila, string columna)
        => fila.IsNull(columna) ? null : (int)fila[columna];

    public static short Corto(this DataRow fila, string columna)
        => (short)fila[columna];

    public static decimal Decimal(this DataRow fila, string columna)
        => (decimal)fila[columna];

    public static bool Booleano(this DataRow fila, string columna)
        => (bool)fila[columna];

    public static string Texto(this DataRow fila, string columna)
        => (string)fila[columna];

    public static string? TextoNulo(this DataRow fila, string columna)
        => fila.IsNull(columna) ? null : (string)fila[columna];

    public static DateTime Fecha(this DataRow fila, string columna)
        => (DateTime)fila[columna];

    public static DateTime? FechaNula(this DataRow fila, string columna)
        => fila.IsNull(columna) ? null : (DateTime)fila[columna];
}
