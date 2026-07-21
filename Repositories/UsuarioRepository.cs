using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using UMG_API.DAL;
using UMG_API.Models;
using UMG_API.Models.DTO;

namespace UMG_API.Repositories
{
    public class UsuarioRepository
    {
        public Usuario Crear(string correo, string hashContrasena, string nombre, string apellido, int rolId)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"INSERT INTO UMG_USERS 
                                    (UMG_Usuario, UMG_Contrasena, UMG_Nombre, UMG_Apellido, UMG_Rol_ID)
                                  OUTPUT INSERTED.UMG_ID, INSERTED.UMG_Usuario, INSERTED.UMG_Nombre, 
                                         INSERTED.UMG_Apellido, INSERTED.UMG_Rol_ID, INSERTED.UMG_Estado,
                                         INSERTED.UMG_Ingreso, INSERTED.UMG_Fecha_Creacion
                                  VALUES (@Correo, @Contrasena, @Nombre, @Apellido, @RolId)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = correo;
                    cmd.Parameters.Add("@Contrasena", SqlDbType.VarChar, 255).Value = hashContrasena;
                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 50).Value = nombre;
                    cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 50).Value = apellido;
                    cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = rolId;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                UMG_ID = Convert.ToInt32(reader["UMG_ID"]),
                                UMG_Usuario = reader["UMG_Usuario"].ToString(),
                                UMG_Nombre = reader["UMG_Nombre"].ToString(),
                                UMG_Apellido = reader["UMG_Apellido"].ToString(),
                                UMG_Rol_ID = Convert.ToInt32(reader["UMG_Rol_ID"]),
                                UMG_Estado = Convert.ToInt32(reader["UMG_Estado"]),
                                UMG_Ingreso = Convert.ToInt32(reader["UMG_Ingreso"]),
                                UMG_Fecha_Creacion = Convert.ToDateTime(reader["UMG_Fecha_Creacion"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public bool ExisteCorreo(string correo)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = "SELECT COUNT(1) FROM UMG_USERS WHERE UMG_Usuario = @Correo";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = correo;
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public bool ExisteRol(int rolId)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = "SELECT COUNT(1) FROM UMG_ROLES WHERE UMG_ID = @RolId AND UMG_Estado = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = rolId;
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        // Inactivar usuario (RF de administración)
        public bool Inactivar(int userId)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = "UPDATE UMG_USERS SET UMG_Estado = 0 WHERE UMG_ID = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
        }

        // Resetear contraseña (usuario olvidó su clave) - Admin le asigna una temporal
        public bool ResetearContrasena(int userId, string contrasenaTemporal)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"UPDATE UMG_USERS 
                          SET UMG_Contrasena = @Contrasena,
                              UMG_Ingreso = 0,
                              UMG_Fecha_Modifica_Contrasena = GETDATE()
                          WHERE UMG_ID = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Contrasena", SqlDbType.VarChar, 255).Value = contrasenaTemporal;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
        }
        public List<UsuarioListDto> ObtenerTodos()
        {
            var usuarios = new List<UsuarioListDto>();

            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"SELECT u.UMG_ID, u.UMG_Usuario, u.UMG_Nombre, u.UMG_Apellido,
                                 u.UMG_Rol_ID, r.UMG_Nombre AS UMG_Rol_Nombre, u.UMG_Estado,
                                 u.UMG_Ingreso, u.UMG_Fecha_Creacion, u.UMG_Ultimo_Acceso
                          FROM UMG_USERS u
                          INNER JOIN UMG_ROLES r ON u.UMG_Rol_ID = r.UMG_ID
                          ORDER BY u.UMG_Nombre, u.UMG_Apellido";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new UsuarioListDto
                        {
                            UMG_ID = Convert.ToInt32(reader["UMG_ID"]),
                            UMG_Usuario = reader["UMG_Usuario"].ToString(),
                            UMG_Nombre = reader["UMG_Nombre"].ToString(),
                            UMG_Apellido = reader["UMG_Apellido"].ToString(),
                            UMG_Rol_ID = Convert.ToInt32(reader["UMG_Rol_ID"]),
                            UMG_Rol_Nombre = reader["UMG_Rol_Nombre"].ToString(),
                            UMG_Estado = Convert.ToInt32(reader["UMG_Estado"]),
                            UMG_Ingreso = Convert.ToInt32(reader["UMG_Ingreso"]),
                            UMG_Fecha_Creacion = Convert.ToDateTime(reader["UMG_Fecha_Creacion"]),
                            UMG_Ultimo_Acceso = reader["UMG_Ultimo_Acceso"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["UMG_Ultimo_Acceso"])
                        });
                    }
                }
            }

            return usuarios;
        }
        public LoginResponseDto ValidarCredenciales(string correo, string contrasena)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"SELECT u.UMG_ID, u.UMG_Usuario, u.UMG_Nombre, u.UMG_Apellido,
                                 u.UMG_Rol_ID, r.UMG_Nombre AS UMG_Rol_Nombre, u.UMG_Ingreso
                          FROM UMG_USERS u
                          INNER JOIN UMG_ROLES r ON u.UMG_Rol_ID = r.UMG_ID
                          WHERE u.UMG_Usuario = @Correo 
                            AND u.UMG_Contrasena = @Contrasena
                            AND u.UMG_Estado = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 100).Value = correo;
                    cmd.Parameters.Add("@Contrasena", SqlDbType.VarChar, 255).Value = contrasena;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new LoginResponseDto
                            {
                                UMG_ID = Convert.ToInt32(reader["UMG_ID"]),
                                UMG_Usuario = reader["UMG_Usuario"].ToString(),
                                UMG_Nombre = reader["UMG_Nombre"].ToString(),
                                UMG_Apellido = reader["UMG_Apellido"].ToString(),
                                UMG_Rol_ID = Convert.ToInt32(reader["UMG_Rol_ID"]),
                                UMG_Rol_Nombre = reader["UMG_Rol_Nombre"].ToString(),
                                RequiereCambioContrasena = Convert.ToInt32(reader["UMG_Ingreso"]) == 0
                            };
                        }
                    }
                }
            }

            return null;
        }


        public void ActualizarUltimoAcceso(int userId)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"UPDATE UMG_USERS 
                          SET UMG_Ultimo_Acceso = GETDATE()
                          WHERE UMG_ID = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CambiarContrasena(int userId, string nuevaContrasena)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = @"UPDATE UMG_USERS 
                          SET UMG_Contrasena = @Contrasena,
                              UMG_Fecha_Modifica_Contrasena = GETDATE(),
                              UMG_Ultimo_Acceso = GETDATE(),
                              UMG_Ingreso = 1
                          WHERE UMG_ID = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Contrasena", SqlDbType.VarChar, 255).Value = nuevaContrasena;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ExisteUsuarioActivo(int userId)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = "SELECT COUNT(1) FROM UMG_USERS WHERE UMG_ID = @UserId AND UMG_Estado = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

    }
}