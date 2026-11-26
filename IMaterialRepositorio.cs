public interface IMaterialRepositorio
{
    void Agregar(Material material);
    Material? ObtenerPorId(int id);        // ¿qué tipo devuelve?
    List<Material> ObtenerTodos();              // ¿colección de qué?
    bool Actualizar(Material material);
    bool Eliminar(int id);
}