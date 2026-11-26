# Biblioteca (C# y SQL Server)

Catálogo de biblioteca en C# con SQL Server. Cubre cinco tipos de material con herencia y polimorfismo, y accede a la base de datos con consultas parametrizadas. Próximo paso: exponerlo como una API REST con ASP.NET Core.

## Tecnologías
- C# / .NET 9.0.303
- SQL Server Express
- ADO.NET (`Microsoft.Data.SqlClient`)

## Qué incluye
- Cinco tipos de material: libro, revista, libro de colección, enciclopedia y periódico, todos heredan de una clase abstracta `Material`.
- Patrón Repository: la interfaz `IMaterialRepositorio` separa la lógica del programa del acceso a datos.
- Operaciones: listar todos, buscar por Id, agregar, actualizar y eliminar.
- Consultas parametrizadas, que evitan la inyección SQL.
- Restricciones en la base de datos (`UNIQUE` sobre el código y `CHECK` sobre el tipo de material).

## Cómo ejecutarlo
1. Instala el SDK de .NET y SQL Server Express.
2. Crea la base de datos y la tabla ejecutando este script:

```sql
CREATE DATABASE Biblioteca;
GO
USE Biblioteca;
GO
CREATE TABLE Materiales (
    Id  int IDENTITY(1,1) PRIMARY KEY,
    Tipo NVARCHAR (200) NOT NULL CONSTRAINT CHK_Tipo CHECK (Tipo IN ('Libro', 'Revista', 'Periodico','LibroColeccion','Enciclopedia')),
    Codigo NVARCHAR(200) NOT NULL UNIQUE  CONSTRAINT CK_Materiales_Codigo_NoVacio CHECK (LEN(TRIM(Codigo)) > 0),
    Titulo NVARCHAR (200) NOT NULL CONSTRAINT CK_Materiales_Titulo_NoVacio CHECK (LEN(TRIM(Titulo)) > 0), 
    Autor NVARCHAR (200) NULL,
    NumeroEdicion int NULL,
    Volumen int NULL,
    Fecha DATE NULL
)
```

3. En `Program.cs`, ajusta la cadena de conexión a tu servidor.
    Server=localhost\SQLEXPRESS;Database=Biblioteca;Trusted_Connection=True;TrustServerCertificate=True;

4. Ejecuta el proyecto:

```
dotnet run
```

## Estado del proyecto
Terminada la capa de acceso a datos con las cinco operaciones. Siguientes pasos: pruebas unitarias, API REST con ASP.NET Core, Entity Framework Core y autenticación con JWT.