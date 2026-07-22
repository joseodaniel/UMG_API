using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UMG_API.DAL;
using UMG_API.Models;
using UMG_API.Models.DTO;

namespace UMG_API.Repositories
{
    public class ReservaRepository
    {

        // LISTADO con filtros opcionales (labId, fecha, userId)
        public List<ReservaListDto> ObtenerTodas(int? labId, DateTime? fecha, int? userId)
        {
            var lista = new List<ReservaListDto>();

            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"SELECT r.UMG_ID, r.UMG_User_ID, 
                                 (u.UMG_Nombre + ' ' + u.UMG_Apellido) AS UMG_Docente_Nombre,
                                 u.UMG_Usuario AS UMG_Docente_Correo,
                                 r.UMG_Lab_ID, l.UMG_Nombre AS UMG_Lab_Nombre,
                                 r.UMG_Fecha_Reserva, r.UMG_Hora_Inicio, r.UMG_Hora_Fin,
                                 r.UMG_Motivo, r.UMG_Estado, r.UMG_Fecha_Registro
                          FROM UMG_RESERV r
                          INNER JOIN UMG_USERS u ON r.UMG_User_ID = u.UMG_ID
                          INNER JOIN UMG_LABS l ON r.UMG_Lab_ID = l.UMG_ID
                          WHERE (@LabId IS NULL OR r.UMG_Lab_ID = @LabId)
                            AND (@Fecha IS NULL OR r.UMG_Fecha_Reserva = @Fecha)
                            AND (@UserId IS NULL OR r.UMG_User_ID = @UserId)
                          ORDER BY r.UMG_Fecha_Reserva DESC, r.UMG_Hora_Inicio";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@LabId", SqlDbType.Int).Value = (object)labId ?? DBNull.Value;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = (object)fecha?.Date ?? DBNull.Value;
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = (object)userId ?? DBNull.Value;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new ReservaListDto
                            {
                                UMG_ID = Convert.ToInt32(reader["UMG_ID"]),
                                UMG_User_ID = Convert.ToInt32(reader["UMG_User_ID"]),
                                UMG_Docente_Nombre = reader["UMG_Docente_Nombre"].ToString(),
                                UMG_Docente_Correo = reader["UMG_Docente_Correo"].ToString(),
                                UMG_Lab_ID = Convert.ToInt32(reader["UMG_Lab_ID"]),
                                UMG_Lab_Nombre = reader["UMG_Lab_Nombre"].ToString(),
                                UMG_Fecha_Reserva = Convert.ToDateTime(reader["UMG_Fecha_Reserva"]),
                                UMG_Hora_Inicio = (TimeSpan)reader["UMG_Hora_Inicio"],
                                UMG_Hora_Fin = (TimeSpan)reader["UMG_Hora_Fin"],
                                UMG_Motivo = reader["UMG_Motivo"].ToString(),
                                UMG_Estado = reader["UMG_Estado"].ToString(),
                                UMG_Fecha_Registro = Convert.ToDateTime(reader["UMG_Fecha_Registro"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // Consultar una reserva puntual (para validar antes de cancelar, o para mostrar detalle)
        public ReservaListDto ObtenerPorId(int id)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"SELECT r.UMG_ID, r.UMG_User_ID, 
                                 (u.UMG_Nombre + ' ' + u.UMG_Apellido) AS UMG_Docente_Nombre,
                                 u.UMG_Usuario AS UMG_Docente_Correo,
                                 r.UMG_Lab_ID, l.UMG_Nombre AS UMG_Lab_Nombre,
                                 r.UMG_Fecha_Reserva, r.UMG_Hora_Inicio, r.UMG_Hora_Fin,
                                 r.UMG_Motivo, r.UMG_Estado, r.UMG_Fecha_Registro
                          FROM UMG_RESERV r
                          INNER JOIN UMG_USERS u ON r.UMG_User_ID = u.UMG_ID
                          INNER JOIN UMG_LABS l ON r.UMG_Lab_ID = l.UMG_ID
                          WHERE r.UMG_ID = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ReservaListDto
                            {
                                UMG_ID = Convert.ToInt32(reader["UMG_ID"]),
                                UMG_User_ID = Convert.ToInt32(reader["UMG_User_ID"]),
                                UMG_Docente_Nombre = reader["UMG_Docente_Nombre"].ToString(),
                                UMG_Docente_Correo = reader["UMG_Docente_Correo"].ToString(),
                                UMG_Lab_ID = Convert.ToInt32(reader["UMG_Lab_ID"]),
                                UMG_Lab_Nombre = reader["UMG_Lab_Nombre"].ToString(),
                                UMG_Fecha_Reserva = Convert.ToDateTime(reader["UMG_Fecha_Reserva"]),
                                UMG_Hora_Inicio = (TimeSpan)reader["UMG_Hora_Inicio"],
                                UMG_Hora_Fin = (TimeSpan)reader["UMG_Hora_Fin"],
                                UMG_Motivo = reader["UMG_Motivo"].ToString(),
                                UMG_Estado = reader["UMG_Estado"].ToString(),
                                UMG_Fecha_Registro = Convert.ToDateTime(reader["UMG_Fecha_Registro"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        // CANCELAR (RN-06: una vez cancelada, no se puede volver a cancelar ni modificar)
        public bool Cancelar(int id)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"UPDATE UMG_RESERV 
                          SET UMG_Estado = 'C'
                          WHERE UMG_ID = @Id AND UMG_Estado = 'R'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public Reserva Crear(int userId, int labId, DateTime fecha, TimeSpan horaInicio,
                              TimeSpan horaFin, string motivo)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"INSERT INTO UMG_RESERV 
                                    (UMG_User_ID, UMG_Lab_ID, UMG_Fecha_Reserva, UMG_Hora_Inicio, UMG_Hora_Fin, UMG_Motivo)
                                  OUTPUT INSERTED.UMG_ID, INSERTED.UMG_User_ID, INSERTED.UMG_Lab_ID,
                                         INSERTED.UMG_Fecha_Reserva, INSERTED.UMG_Hora_Inicio, INSERTED.UMG_Hora_Fin,
                                         INSERTED.UMG_Motivo, INSERTED.UMG_Estado, INSERTED.UMG_Fecha_Registro
                                  VALUES (@UserId, @LabId, @Fecha, @HoraInicio, @HoraFin, @Motivo)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@LabId", SqlDbType.Int).Value = labId;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;
                    cmd.Parameters.Add("@HoraInicio", SqlDbType.Time).Value = horaInicio;
                    cmd.Parameters.Add("@HoraFin", SqlDbType.Time).Value = horaFin;
                    cmd.Parameters.Add("@Motivo", SqlDbType.VarChar, 150).Value = motivo;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Reserva
                            {
                                UMG_ID = Convert.ToInt32(reader["UMG_ID"]),
                                UMG_User_ID = Convert.ToInt32(reader["UMG_User_ID"]),
                                UMG_Lab_ID = Convert.ToInt32(reader["UMG_Lab_ID"]),
                                UMG_Fecha_Reserva = Convert.ToDateTime(reader["UMG_Fecha_Reserva"]),
                                UMG_Hora_Inicio = (TimeSpan)reader["UMG_Hora_Inicio"],
                                UMG_Hora_Fin = (TimeSpan)reader["UMG_Hora_Fin"],
                                UMG_Motivo = reader["UMG_Motivo"].ToString(),
                                UMG_Estado = reader["UMG_Estado"].ToString(),
                                UMG_Fecha_Registro = Convert.ToDateTime(reader["UMG_Fecha_Registro"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        // RN-01: no debe existir otra reserva ACTIVA para el mismo lab/fecha con traslape de horario
        public bool ExisteTraslape(int labId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"SELECT COUNT(1) FROM UMG_RESERV
                          WHERE UMG_Lab_ID = @LabId
                            AND UMG_Fecha_Reserva = @Fecha
                            AND UMG_Estado = 'R'
                            AND UMG_Hora_Inicio < @HoraFin
                            AND UMG_Hora_Fin > @HoraInicio
                            AND (CAST(UMG_Fecha_Reserva AS DATETIME) + CAST(UMG_Hora_Fin AS DATETIME)) > GETDATE()";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@LabId", SqlDbType.Int).Value = labId;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;
                    cmd.Parameters.Add("@HoraInicio", SqlDbType.Time).Value = horaInicio;
                    cmd.Parameters.Add("@HoraFin", SqlDbType.Time).Value = horaFin;

                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        // Verifica si existe un bloqueo (UMG_CONDI) que traslape con el horario solicitado
        public bool ExisteBloqueo(int labId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();

                string query = @"SELECT COUNT(1) FROM UMG_CONDI
                          WHERE (UMG_Lab_ID = @LabId OR UMG_Lab_ID IS NULL)
                            AND UMG_Fecha = @Fecha
                            AND UMG_Estado = 1
                            AND UMG_Hora_Inicio < @HoraFin
                            AND UMG_Hora_Fin > @HoraInicio
                            AND (CAST(UMG_Fecha AS DATETIME) + CAST(UMG_Hora_Fin AS DATETIME)) > GETDATE()";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@LabId", SqlDbType.Int).Value = labId;
                    cmd.Parameters.Add("@Fecha", SqlDbType.Date).Value = fecha.Date;
                    cmd.Parameters.Add("@HoraInicio", SqlDbType.Time).Value = horaInicio;
                    cmd.Parameters.Add("@HoraFin", SqlDbType.Time).Value = horaFin;

                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public bool ExisteUsuario(int userId)
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

        public bool ExisteLab(int labId)
        {
            using (SqlConnection conn = Conexion.ObtenerConexion())
            {
                conn.Open();
                string query = "SELECT COUNT(1) FROM UMG_LABS WHERE UMG_ID = @LabId AND UMG_Estado = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@LabId", SqlDbType.Int).Value = labId;
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }
    }
}