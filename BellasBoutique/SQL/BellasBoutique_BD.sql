/* BELLASBOUTIQUE — Proyecto IHC, SQL Server local / SSMS
   Versión funcional académica. Pagos y correo local de demostración.
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
-- Contraseña inicial de demostración: 123456789 (PBKDF2).
IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE Usuario='AdminUIA')
 INSERT dbo.Usuarios(Usuario,Cedula,Nombre,Apellidos,Correo,Telefono,Direccion,Contrasena,Rol)
 VALUES('AdminUIA','1-1111-1111','Eduardo','Montoya Acuña','admin@bellasboutique.local','8888-0001','San José','v1:Z0fXHmjac8odwc/oo3V/qQ==:Y+TIXEeiPSNUwJnvZlv3YUlsuw0hAsurENcc1c0uKEQ=','Administrador');
IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE Usuario='vendedorUIA')
 INSERT dbo.Usuarios(Usuario,Cedula,Nombre,Apellidos,Correo,Telefono,Direccion,Contrasena,Rol)
 VALUES('vendedorUIA','1-2222-2222','Juan Pablo','Sandí Álvarez','vendedor@bellasboutique.local','8888-0002','San José','v1:efQk6IwiyU3lm0xVOknyGg==:R0F6XZ4yjikV1Xhh+L/Nc73SFb7Lsixl0OwIRS/6DiQ=','Vendedor');
IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE Usuario='clienteUIA')
 INSERT dbo.Usuarios(Usuario,Cedula,Nombre,Apellidos,Correo,Telefono,Direccion,Contrasena,Rol)
 VALUES('clienteUIA','1-3333-3333','Joel','Zúñiga González','cliente@bellasboutique.local','8888-0003','San José','v1:My+VEOogvWi7++vl+p21Ww==:z/d00pz0W0x9eWiehuxD8yaXgtRnVb9QK9atgDf/fT8=','Cliente');
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

IF OBJECT_ID('dbo.Clientes','U') IS NULL CREATE TABLE Clientes(UsuarioId INT PRIMARY KEY REFERENCES Usuarios(Id), Fecha DATETIME2 DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.IntentosLogin','U') IS NULL CREATE TABLE IntentosLogin(Id INT IDENTITY PRIMARY KEY, Identificador NVARCHAR(150) NOT NULL, Exitoso BIT NOT NULL, Fecha DATETIME2 DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.Carritos','U') IS NULL CREATE TABLE Carritos(Id INT IDENTITY PRIMARY KEY, UsuarioId INT NOT NULL REFERENCES Usuarios(Id), Fecha DATETIME2 DEFAULT SYSDATETIME(), Cerrado BIT DEFAULT 0 NOT NULL);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_CarritoAbierto' AND object_id=OBJECT_ID('dbo.Carritos')) CREATE UNIQUE INDEX IX_CarritoAbierto ON Carritos(UsuarioId) WHERE Cerrado=0;
IF OBJECT_ID('dbo.DetallesCarrito','U') IS NULL CREATE TABLE DetallesCarrito(CarritoId INT REFERENCES Carritos(Id), ProductoId INT REFERENCES Productos(Id), Cantidad INT NOT NULL CHECK(Cantidad>0), PRIMARY KEY(CarritoId,ProductoId));
IF OBJECT_ID('dbo.Pagos','U') IS NULL CREATE TABLE Pagos(Id INT IDENTITY PRIMARY KEY, CarritoId INT NOT NULL REFERENCES Carritos(Id), Metodo NVARCHAR(20) NOT NULL CHECK(Metodo IN ('Tarjeta','SINPE','Transferencia')), Estado NVARCHAR(20) NOT NULL CHECK(Estado IN ('Exitoso','Fallido','Pendiente')), Fecha DATETIME2 DEFAULT SYSDATETIME(), Monto DECIMAL(18,2) NOT NULL);
IF OBJECT_ID('dbo.Facturas','U') IS NULL CREATE TABLE Facturas(Id INT IDENTITY PRIMARY KEY, UsuarioId INT NOT NULL REFERENCES Usuarios(Id), CarritoId INT NOT NULL UNIQUE REFERENCES Carritos(Id), PagoId INT NOT NULL UNIQUE REFERENCES Pagos(Id), Fecha DATETIME2 DEFAULT SYSDATETIME(), Cliente NVARCHAR(220) NOT NULL, Correo NVARCHAR(150) NOT NULL, Direccion NVARCHAR(350) NOT NULL, Subtotal DECIMAL(18,2) NOT NULL, Impuesto DECIMAL(18,2) NOT NULL, Total DECIMAL(18,2) NOT NULL);
IF OBJECT_ID('dbo.DetallesFactura','U') IS NULL CREATE TABLE DetallesFactura(Id INT IDENTITY PRIMARY KEY, FacturaId INT REFERENCES Facturas(Id), ProductoId INT REFERENCES Productos(Id), Nombre NVARCHAR(180) NOT NULL, Categoria NVARCHAR(40) NOT NULL, Cantidad INT NOT NULL, Precio DECIMAL(12,2) NOT NULL);
IF OBJECT_ID('dbo.Ventas','U') IS NULL CREATE TABLE Ventas(Id INT IDENTITY PRIMARY KEY, FacturaId INT REFERENCES Facturas(Id), ProductoId INT REFERENCES Productos(Id), Producto NVARCHAR(180) NOT NULL, Categoria NVARCHAR(40) NOT NULL, Cantidad INT NOT NULL, Precio DECIMAL(12,2) NOT NULL, Fecha DATETIME2 DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.Encuestas','U') IS NULL CREATE TABLE Encuestas(Id INT IDENTITY PRIMARY KEY, UsuarioId INT REFERENCES Usuarios(Id), Puntaje INT CHECK(Puntaje BETWEEN 1 AND 5), Comentario NVARCHAR(1000), Fecha DATETIME2 DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.Sugerencias','U') IS NULL CREATE TABLE Sugerencias(Id INT IDENTITY PRIMARY KEY, UsuarioId INT REFERENCES Usuarios(Id), Texto NVARCHAR(1000) NOT NULL, Fecha DATETIME2 DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.MensajesChat','U') IS NULL CREATE TABLE MensajesChat(Id INT IDENTITY PRIMARY KEY, ClienteId INT NOT NULL REFERENCES Usuarios(Id), EmisorId INT NOT NULL REFERENCES Usuarios(Id), Texto NVARCHAR(1000) NOT NULL, Fecha DATETIME2 DEFAULT SYSDATETIME());
IF OBJECT_ID('dbo.Bitacora','U') IS NULL CREATE TABLE Bitacora(Id INT IDENTITY PRIMARY KEY, UsuarioId INT REFERENCES Usuarios(Id), Usuario NVARCHAR(220) NOT NULL, Rol NVARCHAR(25) NOT NULL, Accion NVARCHAR(40) NOT NULL, Detalle NVARCHAR(1000) NOT NULL, Fecha DATETIME2 DEFAULT SYSDATETIME());
GO
INSERT Clientes(UsuarioId) SELECT Id FROM Usuarios u WHERE Rol='Cliente' AND NOT EXISTS(SELECT 1 FROM Clientes c WHERE c.UsuarioId=u.Id);
GO
UPDATE Usuarios SET Contrasena='v1:Z0fXHmjac8odwc/oo3V/qQ==:Y+TIXEeiPSNUwJnvZlv3YUlsuw0hAsurENcc1c0uKEQ=' WHERE Usuario='AdminUIA' AND Contrasena='123456789';
UPDATE Usuarios SET Contrasena='v1:efQk6IwiyU3lm0xVOknyGg==:R0F6XZ4yjikV1Xhh+L/Nc73SFb7Lsixl0OwIRS/6DiQ=' WHERE Usuario='vendedorUIA' AND Contrasena='123456789';
UPDATE Usuarios SET Contrasena='v1:My+VEOogvWi7++vl+p21Ww==:z/d00pz0W0x9eWiehuxD8yaXgtRnVb9QK9atgDf/fT8=' WHERE Usuario='clienteUIA' AND Contrasena='123456789';
UPDATE dbo.Productos SET Imagen = N'https://dmfwoh5tsii9v.cloudfront.net/sites/files/hvv/productimg/202107/botaniste-floral-puff-sleeve-midi-dress-1.jpg'
WHERE Nombre = N'Vestido floral' AND (Imagen IS NULL OR Imagen='');

UPDATE dbo.Productos SET Imagen = N'https://staticv3.smallable.com/1609653-2000x2000q80/olaria-linen-blouse.webp'
WHERE Nombre = N'Blusa de lino' AND (Imagen IS NULL OR Imagen='');

UPDATE dbo.Productos SET Imagen = N'https://whitesunshoes.com/cdn/shop/files/JIHAR_sandales_en_cuir_camel-White-Sun-4_10d453fa-fb71-4e34-837a-eb61b8bdc81b.jpg?v=1743159103&width=2000'
WHERE Nombre = N'Sandalias trenzadas' AND (Imagen IS NULL OR Imagen='');

UPDATE dbo.Productos SET Imagen = N'https://hroylshoes.com/cdn/shop/files/Beige-Matte-Leather-Pumps-61.jpg?v=1755851547'
WHERE Nombre = N'Tacones nude' AND (Imagen IS NULL OR Imagen='');

UPDATE dbo.Productos SET Imagen = N'https://makeshop-multi-images.akamaized.net/creed/itemimages/000000002481_uP189CF.jpg'
WHERE Nombre = N'Bolso tote' AND (Imagen IS NULL OR Imagen='');

UPDATE dbo.Productos SET Imagen = N'https://www.linjer.co/cdn/shop/files/LEA-PEARLSTUDS-SS_38cddabb-27c5-48e0-84b7-3404c6f64de7_1080x.jpg?v=1716895334'
WHERE Nombre = N'Aretes de perla' AND (Imagen IS NULL OR Imagen='');


GO
