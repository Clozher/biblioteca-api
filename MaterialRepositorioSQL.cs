using System.ComponentModel.Design;
using System.Net.Mail;
using Microsoft.Data.SqlClient;
using System.Globalization;

public class MaterialRepositorioSql : IMaterialRepositorio
{
    private readonly string _cadenaConexion;
    private const string TipoLibro = "Libro";
    private const string TipoRevista = "Revista";
    private const string TipoLibroColeccion = "LibroColeccion";
    private const string TipoEnciclopedia = "Enciclopedia";
    private const string TipoPeriodico = "Periodico";

    public MaterialRepositorioSql(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public static Material ConvertirFilaEnMaterial(SqlDataReader lector)
    {
        string tipo = lector["Tipo"].ToString() ?? "";
        string codigo = lector["Codigo"].ToString() ?? "";
        string titulo = lector["Titulo"].ToString() ?? "";
        int id = Convert.ToInt32(lector["Id"]);
        Material material;
        if (tipo == TipoLibro)
        {
            string autor = lector["Autor"].ToString() ?? "";
            material = new Libro(titulo, codigo, autor);
        }
        else if (tipo == TipoRevista)
        {
            int numeroEdicion = Convert.ToInt32(lector["NumeroEdicion"]);
            material = new Revista(titulo, codigo, numeroEdicion);
        }
        else if (tipo == TipoLibroColeccion)
        {
            string autor = lector["Autor"].ToString() ?? "";
            material = new LibroColeccion(titulo, codigo, autor);
        }
        else if (tipo == TipoEnciclopedia)
        {
            int volumen = Convert.ToInt32(lector["Volumen"]);
            material = new Enciclopedia(titulo, codigo, volumen);
        }
        else if (tipo == TipoPeriodico)
        {
            DateTime fecha = Convert.ToDateTime(lector["Fecha"]);
            material = new Periodico(titulo, codigo, fecha.ToString("yyyy-MM-dd"));
        }
        else
        {
            throw new InvalidOperationException($"Tipo de material desconocido: {tipo}");
        }
        material.Id = id;
        return material;


    }

    public List<Material> ObtenerTodos()
    {
        var materiales = new List<Material>();

        using var conexion = new SqlConnection(_cadenaConexion);
        conexion.Open();
        SqlCommand comando = new SqlCommand(
              "SELECT Id,Tipo, Codigo, Titulo, Autor, NumeroEdicion, Volumen, Fecha FROM Materiales ORDER BY Id",
              conexion);

        using (SqlDataReader lector = comando.ExecuteReader())
        {
            while (lector.Read())
            {
                materiales.Add(ConvertirFilaEnMaterial(lector));
            }
        }

        return materiales;
    }

    public void Agregar(Material material)
    {
        string tipo;
        object autor = DBNull.Value;
        object numeroEdicion = DBNull.Value;
        object volumen = DBNull.Value;
        object fecha = DBNull.Value;

        if (material is Libro libro)
        {
            tipo = TipoLibro;
            autor = libro.Autor;
        }
        else if (material is Revista revista)
        {
            tipo = TipoRevista;
            numeroEdicion = revista.NumeroEdicion;
        }
        else if (material is LibroColeccion libroColeccion)
        {
            tipo = TipoLibroColeccion;
            autor = libroColeccion.Autor;
        }
        else if (material is Enciclopedia enciclopedia)
        {
            tipo = TipoEnciclopedia;
            volumen = enciclopedia.Volumen;
        }
        else if (material is Periodico periodico)
        {
            tipo = TipoPeriodico;
            fecha = DateTime.ParseExact(periodico.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        else
        {
            throw new NotImplementedException("Tipo aún no soportado");
        }

        using var conexion = new SqlConnection(_cadenaConexion);
        conexion.Open();

        var comando = new SqlCommand(
            @"INSERT INTO Materiales (Tipo, Codigo, Titulo, Autor, NumeroEdicion, Volumen, Fecha)
          VALUES (@tipo, @codigo, @titulo, @autor, @numeroedicion, @volumen, @fecha);
          SELECT CAST(SCOPE_IDENTITY() AS int);",
            conexion);


        comando.Parameters.AddWithValue("@tipo", tipo);
        comando.Parameters.AddWithValue("@codigo", material.Codigo);
        comando.Parameters.AddWithValue("@titulo", material.Titulo);
        comando.Parameters.AddWithValue("@autor", autor);
        comando.Parameters.AddWithValue("@numeroedicion", numeroEdicion);
        comando.Parameters.AddWithValue("@volumen", volumen);
        comando.Parameters.AddWithValue("@fecha", fecha);


        int nuevoId = (int)comando.ExecuteScalar();
        material.Id = nuevoId;
    }







    public Material? ObtenerPorId(int id)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        conexion.Open();
        var comando = new SqlCommand("SELECT Id,Tipo, Codigo, Titulo, Autor, NumeroEdicion, Volumen, Fecha FROM Materiales WHERE Materiales.Id = @id ", conexion);
        comando.Parameters.AddWithValue("@id", id);
        using (SqlDataReader lector = comando.ExecuteReader())
        {
            if (lector.Read())
            {
                return ConvertirFilaEnMaterial(lector);

            }
            return null;
        }

    }


    public bool Actualizar(Material material)
    {
        object autor = DBNull.Value;
        object numeroEdicion = DBNull.Value;
        object volumen = DBNull.Value;
        object fecha = DBNull.Value;

        if (material is Libro libro)
        {
            autor = libro.Autor;
        }
        else if (material is Revista revista)
        {
            numeroEdicion = revista.NumeroEdicion;
        }
        else if (material is LibroColeccion libroColeccion)
        {
            autor = libroColeccion.Autor;
        }
        else if (material is Enciclopedia enciclopedia)
        {
            volumen = enciclopedia.Volumen;
        }
        else if (material is Periodico periodico)
        {
            fecha = DateTime.ParseExact(periodico.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        else
        {
            throw new NotImplementedException("Tipo aún no soportado");
        }

        using var conexion = new SqlConnection(_cadenaConexion);
        conexion.Open();

        var comando = new SqlCommand(
            @"UPDATE Materiales
          SET Codigo = @codigo, Titulo = @titulo, Autor = @autor,
              NumeroEdicion = @numeroedicion, Volumen = @volumen, Fecha = @fecha
          WHERE Id = @id",
            conexion);

        // aquí van los siete AddWithValue, incluido @id con material.Id
        comando.Parameters.AddWithValue("@id", material.Id);
        comando.Parameters.AddWithValue("@codigo", material.Codigo);
        comando.Parameters.AddWithValue("@titulo", material.Titulo);
        comando.Parameters.AddWithValue("@autor", autor);
        comando.Parameters.AddWithValue("@numeroedicion", numeroEdicion);
        comando.Parameters.AddWithValue("@volumen", volumen);
        comando.Parameters.AddWithValue("@fecha", fecha);

        int filasAfectadas = comando.ExecuteNonQuery();
        return filasAfectadas == 1;
    }


    public bool Eliminar(int id)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        conexion.Open();

        var comando = new SqlCommand(
            "DELETE FROM Materiales WHERE Id = @id",
            conexion);

        comando.Parameters.AddWithValue("@id", id);

        int filasAfectadas = comando.ExecuteNonQuery();
        return filasAfectadas == 1;
    }

}