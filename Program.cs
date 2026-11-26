using System.Globalization;
using System.Security.Permissions;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Net.Http.Headers;

public abstract class Material
{
    public int Id { get; internal set; }
    public string Titulo { get; internal set;}
    public string Codigo { get; }

    public Material(string titulo, string codigo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new ArgumentException("El titulo no puede estar vacio");
        }
        Titulo = titulo;
        Codigo = codigo;
    }

    public virtual void MostrarInfo()
    {
        Console.WriteLine($"Título: {Titulo}");
        Console.WriteLine($"Código: {Codigo}");
        Console.WriteLine($"Id: {Id}");
    }
}
public interface IPrestable
{
    int ObtenerDiasPrestamo();
}

public interface IRenovable
{
    int Renovar();
}

public class Libro : Material, IPrestable, IRenovable
{
    public string Autor { get; private set; }

    public Libro(string titulo, string codigo, string autor) : base(titulo, codigo)
    {
        Autor = autor;
    }
    public override void MostrarInfo()
    {
        base.MostrarInfo();
        Console.WriteLine($"Autor: {Autor}");
    }
    public int ObtenerDiasPrestamo()
    {
        return 14;
    }
    public int Renovar()
    {
        return 7;
    }
}

public class Revista : Material, IPrestable
{
    public int NumeroEdicion { get; private set; }

    public Revista(string titulo, string codigo, int numeroEdicion) : base(titulo, codigo)
    {
        NumeroEdicion = numeroEdicion;
    }

    public override void MostrarInfo()
    {
        base.MostrarInfo();
        Console.WriteLine($"Edicion: {NumeroEdicion}");
    }

    public int ObtenerDiasPrestamo()
    {
        return 7;
    }

}

public class Enciclopedia : Material
{
    public int Volumen { get; private set; }

    public Enciclopedia(string titulo, string codigo, int volumen) : base(titulo, codigo)
    {
        Volumen = volumen;
    }

    public override void MostrarInfo()
    {
        base.MostrarInfo();
        Console.WriteLine($"Volumen: {Volumen}");
    }
}

public class Periodico : Material, IPrestable
{
    public string Fecha { get; private set; }

    public Periodico(string titulo, string codigo, string fecha) : base(titulo, codigo)
    {
        Fecha = fecha;
    }
    public override void MostrarInfo()
    {
        base.MostrarInfo();
        Console.WriteLine($"Fecha: {Fecha}");
    }
    public int ObtenerDiasPrestamo()
    {
        return 3;
    }
}

public class LibroColeccion : Material
{
    public string Autor { get; private set; }


    public LibroColeccion(string titulo, string codigo, string autor) : base(titulo, codigo)
    {
        Autor = autor;
    }

    public override void MostrarInfo()
    {
        base.MostrarInfo();
        Console.WriteLine($"Autor: {Autor}");
    }
}
public class Usuario
{
    public string Nombre { get; }

    private List<IPrestable> _prestamos;

    public int CantidadPrestamos
    {
        get { return _prestamos.Count; }
    }

    public Usuario(string nombre)
    {
        Nombre = nombre;
        _prestamos = new List<IPrestable>();
    }

    public void TomarPrestado(IPrestable material)
    {
        if (material == null)
        {
            throw new ArgumentNullException(nameof(material));
        }
        if (_prestamos.Count >= 3)
        {
            throw new InvalidOperationException("Se alcanzó el máximo de préstamos");
        }
        else
        {
            _prestamos.Add(material);

        }
    }
}

public class Biblioteca
{
    private INotificador _notificador;
    private List<Material> _materiales;
    private List<Usuario> _usuarios;
    public Biblioteca(INotificador notificador)
    {
        _notificador = notificador;
        _materiales = new List<Material>();
        _usuarios = new List<Usuario>();
    }

    public void RegistrarMaterial(Material m)
    {
        _materiales.Add(m);
        _notificador.Notificar($"Material registrado: {m.Titulo}");
    }
    public void RegistrarUsuario(Usuario u)
    {
        _usuarios.Add(u);
    }

    public void MostrarCatalogo()
    {
        foreach (Material m in _materiales)
        {
            m.MostrarInfo();
        }
    }
    public List<string> ObtenerTitulosOrdenados()
    {
        return _materiales
            .Select(m => m.Titulo)    // de Material a string: ahora tienes títulos
            .OrderBy(t => t)          // los ordena alfabéticamente
            .ToList();
    }
    public List<Material> BuscarPorTitulo(string texto)
    {
        return _materiales
        //.Where(m =>m.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase))
        .ToList();
    }
}

public interface INotificador
{
    // HUECO 1
    void Notificar(string mensaje);
}

public class NotificadorMayusculas : INotificador
{
    public void Notificar(string mensaje)
    {
        Console.WriteLine(mensaje.ToUpper());
    }
}

public class NotificadorConsola : INotificador
{
    // HUECO 2
    public void Notificar(string mensaje)
    {
        Console.WriteLine(mensaje);
    }
}

public interface IRepositorioMateriales
{
    List<Material> ObtenerTodos();

}

public class RepositorioMaterialesSql : IRepositorioMateriales
{
    private string _cadenaConexion;

    public RepositorioMaterialesSql(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public List<Material> ObtenerTodos()
    {
        List<Material> materiales = new List<Material>();

        using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
        {
            conexion.Open();
            SqlCommand comando = new SqlCommand(
                "SELECT Tipo, Codigo, Titulo, Autor, NumeroEdicion, Volumen, Fecha FROM Materiales ORDER BY Id",
                conexion);

            using (SqlDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    string tipo = lector["Tipo"].ToString()?? "";
                    string codigo = lector["Codigo"].ToString()?? "";
                    string titulo = lector["Titulo"].ToString()?? "";

                    if (tipo == "Libro")
                    {
                        string autor = lector["Autor"].ToString()?? "";
                        materiales.Add(new Libro(titulo, codigo, autor));
                    }
                    else if (tipo == "Revista")
                    {
                        int numeroEdicion = Convert.ToInt32(lector["NumeroEdicion"]);
                        materiales.Add(new Revista(titulo, codigo, numeroEdicion));
                    }
                    else if (tipo == "LibroColeccion")
                    {
                        string autor = lector["Autor"].ToString()?? "";
                        materiales.Add(new LibroColeccion(titulo, codigo, autor));
                    }
                    else if (tipo == "Enciclopedia")
                    {
                        int volumen = Convert.ToInt32(lector["Volumen"]);
                        materiales.Add(new Enciclopedia(titulo, codigo, volumen));
                    }
                    else if (tipo == "Periodico")
                    {
                        DateTime fecha = Convert.ToDateTime(lector["Fecha"]);
                        materiales.Add(new Periodico(titulo, codigo, fecha.ToString("yyyy-MM-dd")));
                    }
                    else
                    {
                        throw new InvalidOperationException($"Tipo de material desconocido: {tipo}");
                    }
                }
            }
        }

        return materiales;
    }
}

public class Program
{
    public static void Main()
    {

        string cadena = @"Server=localhost\SQLEXPRESS;Database=Biblioteca;Trusted_Connection=True;TrustServerCertificate=True;";
        MaterialRepositorioSql repositorio = new MaterialRepositorioSql(cadena);

        try
        {
            List<Material> lista1 = repositorio.ObtenerTodos();
            foreach(Material m in lista1)
            {
                 Console.WriteLine($"Id: {m.Id} | Titulo: {m.Titulo}");
            }

        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error de base de datos ({ex.Number}): {ex.Message}");
        }

    }
}