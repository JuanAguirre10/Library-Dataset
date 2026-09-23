using System.Data;

namespace NeptunoApp.Data;

/// <summary>
/// Lecturas por nombre de columna para no repetir GetOrdinal / IsDBNull en cada mapeo.
///
/// Trabajan sobre IDataRecord para que un mismo Mapear sirva a los dos modos:
/// en modo conectado recibe el SqlDataReader abierto contra la base de datos, y
/// en modo desconectado el DataTableReader que recorre la tabla del DataSet ya
/// cargada en memoria (sin conexion).
/// </summary>
internal static class LectorExtensiones
{
    public static int Entero(this IDataRecord lector, string columna)
        => lector.GetInt32(lector.GetOrdinal(columna));

    public static int? EnteroNulo(this IDataRecord lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetInt32(indice);
    }

    public static short Corto(this IDataRecord lector, string columna)
        => lector.GetInt16(lector.GetOrdinal(columna));

    public static decimal Decimal(this IDataRecord lector, string columna)
        => lector.GetDecimal(lector.GetOrdinal(columna));

    public static bool Booleano(this IDataRecord lector, string columna)
        => lector.GetBoolean(lector.GetOrdinal(columna));

    public static string Texto(this IDataRecord lector, string columna)
        => lector.GetString(lector.GetOrdinal(columna));

    public static string? TextoNulo(this IDataRecord lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetString(indice);
    }

    public static DateTime Fecha(this IDataRecord lector, string columna)
        => lector.GetDateTime(lector.GetOrdinal(columna));

    public static DateTime? FechaNula(this IDataRecord lector, string columna)
    {
        var indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetDateTime(indice);
    }
}
