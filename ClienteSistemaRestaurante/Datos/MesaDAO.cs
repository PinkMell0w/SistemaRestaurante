using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class MesaDAO
    {
		public static Mesa ConsultarMesa(int numeroMesa)
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
					return db.Mesas.FirstOrDefault(m => m.numero == numeroMesa && m.estado != "Eliminada");
				}
			}
			catch (Exception)
			{
				throw new Exception("Error al consultar la mesa.");
			}
		}

		public static bool InsertarMesa(Mesa mesaNueva)
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
                    var mesaActivaExistente = db.Mesas.FirstOrDefault(m => m.numero == mesaNueva.numero && m.estado != "Eliminada");
                    if (mesaActivaExistente != null)
                    {
                        throw new Exception("Esta mesa ya existe.");
                    }

                    var mesaEliminada = db.Mesas.FirstOrDefault(m => m.numero == mesaNueva.numero && m.estado == "Eliminada");
                    if (mesaEliminada != null)
                    {
                        mesaEliminada.capacidad = mesaNueva.capacidad;
                        mesaEliminada.estado = "Disponible";
                        db.SaveChanges();
                        return true;
                    }

                    db.Mesas.Add(mesaNueva);
                    db.SaveChanges();
                    return true;
                }
			}
			catch (Exception)
			{
				throw new Exception("Error de conexión al intentar registrar.");
			}
		}

		public static bool ActualizarMesa(int idMesa, int capacidad)
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
					var mesaExistente = db.Mesas.FirstOrDefault(m => m.idMesa == idMesa);
					if (mesaExistente != null)
					{
						mesaExistente.capacidad = capacidad;
						db.SaveChanges();
						return true;
					}
					else
					{
						throw new Exception("Mesa no encontrada.");
					}
				}
			}
			catch (Exception)
			{
				throw new Exception("Error de conexión al intentar actualizar.");
			}
		}

		public static List<Mesa> ObtenerMesasInventario()
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
					return db.Mesas.Where(m => m.estado != "Eliminada").ToList();
				}
			}
			catch (Exception)
			{
				throw new Exception("Error al cargar la lista de mesas.");
			}
		}

        public static List<Mesa> ObtenerMesasDisponibles()
        {
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
                    db.Database.CommandTimeout = 1;
                    return db.Mesas.Where(m => m.estado == "Disponible").ToList();
				}
			}
			catch (Exception)
			{
				throw new Exception("Error al consultar mesas disponibles.");
			}
        }

		public static bool ActualizarDisponibilidadMesa(int idMesa, string estado)
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
					var mesaExistente = db.Mesas.FirstOrDefault(m => m.idMesa == idMesa);
					if (mesaExistente != null)
					{
						mesaExistente.estado = estado;
						db.SaveChanges();
						return true;
					}
					else
					{
						throw new Exception("Mesa no encontrada.");
					}
				}
			}
			catch (Exception)
			{
				throw new MesaException("Error al actualizar el estado de la mesa.");
			}
		}

		public static bool EliminarMesa(int idMesa)
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
					var mesaExistente = db.Mesas.FirstOrDefault(m => m.idMesa == idMesa);
					if (mesaExistente != null)
					{
						if (mesaExistente.estado == "Ocupada")
						{
							throw new MesaEnUsoException("No se puede eliminar la mesa porque está en uso.");
						}
						else
						{
                            mesaExistente.estado = "Eliminada";
                            db.SaveChanges();
                            return true;
                        }
					}
					else
					{
						throw new Exception("Mesa no encontrada.");
					}
				}
			}
			catch (MesaEnUsoException)
			{
				throw;
			}
			catch (Exception)
			{
				throw new Exception("Error de conexión al intentar eliminar.");
			}
		}
    }
}
