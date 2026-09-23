using System.Configuration;

namespace NeptunoApp.Configuracion;

/// <summary>
/// Lee la cadena de conexion desde App.config.
///
/// Vive en el proyecto WPF y no en la biblioteca NeptunoApp.Data a proposito:
/// ConfigurationManager resuelve el archivo de configuracion del ensamblado que
/// inicia el proceso. Si esta clase (y la cadena) estuvieran en la biblioteca,
/// el App.config de la biblioteca no se copiaria a la salida y en tiempo de
/// ejecucion la busqueda devolveria null. La biblioteca recibe la cadena ya
/// resuelta por el constructor de cada repositorio.
/// </summary>
public static class DbConfig
{
    /// <summary>Nombre de la entrada en la seccion connectionStrings de App.config.</summary>
    public const string Nombre = "NeptunoDB";

    public static string ConnectionString
    {
        get
        {
            var entrada = ConfigurationManager.ConnectionStrings[Nombre];

            if (string.IsNullOrWhiteSpace(entrada?.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    $"No se encontro la cadena de conexion '{Nombre}'. " +
                    "Debe estar en la seccion <connectionStrings> del App.config " +
                    "del proyecto de inicio (NeptunoApp), no en la biblioteca de datos.");
            }

            return entrada.ConnectionString;
        }
    }
}
