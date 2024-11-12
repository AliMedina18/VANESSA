DROP DATABASE IF EXISTS Vanessa;
CREATE DATABASE Vanessa;
USE Vanessa;

CREATE TABLE Rol (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(20) NOT NULL
);

CREATE TABLE Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(40) NOT NULL,
    Documento INT NOT NULL UNIQUE CHECK (Documento >= 0),
    Correo VARCHAR(50) NOT NULL,
    Contraseña VARCHAR(255) NOT NULL,
    RolId INT,
    Activo BOOLEAN DEFAULT TRUE,
    FechaInactivo DATETIME,
    FOREIGN KEY (RolId) REFERENCES Rol(Id)
);

CREATE TABLE Permiso (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(100)
);

CREATE TABLE UsuarioPermiso (
    UsuarioId INT,
    PermisoId INT,
    PRIMARY KEY (UsuarioId, PermisoId),
    FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id),
    FOREIGN KEY (PermisoId) REFERENCES Permiso(Id)
);

CREATE TABLE Semillero (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Imagen TEXT NOT NULL,
    Area VARCHAR(40) NOT NULL,
    Descripcion TEXT NOT NULL,
    UsuarioId INT NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
);

-- Tabla: Proyecto
CREATE TABLE Proyecto (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    DocumentoProyecto TEXT NOT NULL,
    EquiposInvestigacion VARCHAR(50) NOT NULL,
    FechaInicio DATE NOT NULL,
    SemilleroId INT NOT NULL,
    FOREIGN KEY (SemilleroId) REFERENCES Semillero(Id)
);

-- Tabla: Publicacion
CREATE TABLE Publicacion (
    Id_Publicacion INT AUTO_INCREMENT PRIMARY KEY,
    NombrePublicacion VARCHAR(50) NOT NULL,
    ContenidoPublicacion TEXT NOT NULL,
    TipoPublicacion VARCHAR(35) NOT NULL,
    LugarPublicacion VARCHAR(30) NOT NULL,
    FechaPublicacion DATE NOT NULL,
    HoraPublicacion TIME NOT NULL,
    ImagenPublicacion TEXT NOT NULL,
    ActividadesPublicacion TEXT NOT NULL,
    UsuarioId INT NOT NULL,
    FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
);
