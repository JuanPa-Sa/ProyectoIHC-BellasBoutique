/* BELLASBOUTIQUE — Proyecto IHC, SQL Server local / SSMS
   SIMULACIÓN ACADÉMICA. No utilizar contraseñas en texto plano en producción.
   Ejecutar el script completo con SSMS. No elimina una BD existente.
*/
IF DB_ID(N'BellasBoutiqueUIA') IS NULL
    CREATE DATABASE BellasBoutiqueUIA;
GO
USE BellasBoutiqueUIA;
GO
IF OBJECT_ID(N'dbo.Usuarios',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.Usuarios(
  Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  Usuario NVARCHAR(80) NOT NULL UNIQUE,
  Cedula NVARCHAR(30) NOT NULL UNIQUE,
  Nombre NVARCHAR(90) NOT NULL,
  Apellidos NVARCHAR(120) NOT NULL,
  Correo NVARCHAR(150) NOT NULL UNIQUE,
  Telefono NVARCHAR(30) NOT NULL DEFAULT '',
  Direccion NVARCHAR(350) NOT NULL DEFAULT '',
  Contrasena NVARCHAR(200) NOT NULL,
  Rol NVARCHAR(25) NOT NULL DEFAULT 'Cliente' CHECK (Rol IN ('Cliente','Vendedor','Administrador')),
  Activo BIT NOT NULL DEFAULT 1,
  FechaRegistro DATETIME2 NOT NULL DEFAULT SYSDATETIME()
 );
END;
GO
IF OBJECT_ID(N'dbo.Productos',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.Productos(
  Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  Nombre NVARCHAR(180) NOT NULL,
  Descripcion NVARCHAR(1000) NOT NULL DEFAULT '',
  Categoria NVARCHAR(40) NOT NULL CHECK(Categoria IN ('Ropa','Calzado','Accesorios')),
  Proveedor NVARCHAR(150) NOT NULL DEFAULT '',
  Precio DECIMAL(12,2) NOT NULL CHECK(Precio>=0),
  Stock INT NOT NULL CHECK(Stock>=0),
  Imagen NVARCHAR(1000) NOT NULL DEFAULT '',
  Activo BIT NOT NULL DEFAULT 1
 );
END;
GO
-- Contraseña intencionalmente fija y visible SOLO para la demostración local.
IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE Usuario='AdminUIA')
 INSERT dbo.Usuarios(Usuario,Cedula,Nombre,Apellidos,Correo,Telefono,Direccion,Contrasena,Rol)
 VALUES('AdminUIA','1-1111-1111','Eduardo','Montoya Acuña','admin@bellasboutique.local','8888-0001','San José','123456789','Administrador');
IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE Usuario='vendedorUIA')
 INSERT dbo.Usuarios(Usuario,Cedula,Nombre,Apellidos,Correo,Telefono,Direccion,Contrasena,Rol)
 VALUES('vendedorUIA','1-2222-2222','Juan Pablo','Sandí Álvarez','vendedor@bellasboutique.local','8888-0002','San José','123456789','Vendedor');
IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE Usuario='clienteUIA')
 INSERT dbo.Usuarios(Usuario,Cedula,Nombre,Apellidos,Correo,Telefono,Direccion,Contrasena,Rol)
 VALUES('clienteUIA','1-3333-3333','Joel','Zúñiga González','cliente@bellasboutique.local','8888-0003','San José','123456789','Cliente');
GO
IF NOT EXISTS(SELECT 1 FROM dbo.Productos)
BEGIN
 INSERT dbo.Productos(Nombre,Descripcion,Categoria,Proveedor,Precio,Stock,Imagen,Activo) VALUES
 (N'Vestido floral',N'Vestido midi de algodón con estampado floral.','Ropa',N'Moda Tica',24500,12,'',1),
 (N'Blusa de lino',N'Blusa fresca de lino en tono crema.','Ropa',N'Moda Tica',15900,8,'',1),
 (N'Sandalias trenzadas',N'Sandalias planas de cuero trenzado.','Calzado',N'Pasos CR',21000,5,'',1),
 (N'Tacones nude',N'Tacón medio, cómodos para todo el día.','Calzado',N'Pasos CR',32000,3,'',1),
 (N'Bolso tote',N'Bolso amplio de tela con asas de cuero.','Accesorios',N'Detalles',18750,10,'',1),
 (N'Aretes de perla',N'Aretes pequeños con perla sintética.','Accesorios',N'Detalles',7500,20,'',1);
END;
GO
SELECT Usuario,Contrasena,Rol,Activo FROM Usuarios;
SELECT Id,Nombre,Categoria,Stock,Precio FROM Productos;
