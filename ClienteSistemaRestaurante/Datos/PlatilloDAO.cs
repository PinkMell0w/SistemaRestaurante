using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ClienteSistemaRestaurante.Datos
{
    public class PlatilloDAO
    {
        public static bool ConsultarPlatillo(string nombre)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Platilloes.Any(p => p.nombre.Contains(nombre));
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar el platillo.");
            }
        }

        public static List<Platillo> ObtenerPlatillos()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Platilloes.Include(p => p.Categoria).Include(p => p.EstadoPlatillo).Where(p => p.EstadoPlatillo.nombreEstado != "Eliminado").ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar los platillos.");
            }
        }

        public static List<Platillo> ObtenerPlatillosDisponibles()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    TimeSpan horaActual = DateTime.Now.TimeOfDay;

                    TimeSpan hora7am = new TimeSpan(7, 0, 0);
                    TimeSpan hora12pm = new TimeSpan(12, 0, 0);
                    TimeSpan hora6pm = new TimeSpan(18, 0, 0);
                    TimeSpan hora10pm = new TimeSpan(22, 0, 0);

                    var query = db.Platilloes.Where(p => p.EstadoPlatillo.nombreEstado == "Disponible");

                    if (horaActual >= hora7am && horaActual < hora12pm)
                    {
                        query = query.Where(p => p.Categoria.nombreCategoria != "Alimento"
                                      || (p.tipo != "Entrada"
                                          && p.tipo != "Plato fuerte"
                                          && p.tipo != "Cena"));
                    }
                    else if (horaActual >= hora12pm && horaActual < hora6pm)
                    {
                        query = query.Where(p => p.Categoria.nombreCategoria != "Alimento"
                                      || (p.tipo != "Desayuno"
                                          && p.tipo != "Cena"));
                    }
                    else if (horaActual >= hora6pm && horaActual <= hora10pm)
                    {
                        query = query.Where(p => p.Categoria.nombreCategoria != "Alimento"
                                      || (p.tipo != "Desayuno"
                                          && p.tipo != "Entrada"
                                          && p.tipo != "Plato fuerte"));
                    }
                    else
                    {
                        return new List<Platillo>();
                    }

                    return query.ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar los platillos activos.");
            }
        }

        public static List<Platillo> ObtenerTodos()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Platilloes.ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar los platillos.");
            }
        }

        public static int InsertarPlatillo(Platillo nuevoPlatillo)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    db.Platilloes.Add(nuevoPlatillo);
                    db.SaveChanges();
                    return nuevoPlatillo.idPlatillo;
                }
            }
            catch (Exception)
            {
                throw new Exception("Error de conexión al registrar el platillo.");
            }
        }

        public static bool ModificarPlatillo(int idPlatillo, string nombre, int costo, string descripcion, int idCategoria, string tipo, int estadoPlatillo)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var platilloExistente = db.Platilloes.FirstOrDefault(p => p.idPlatillo == idPlatillo);
                    if (platilloExistente != null)
                    {
                        platilloExistente.nombre = nombre;
                        platilloExistente.costo = costo;
                        platilloExistente.descripcion = descripcion;
                        platilloExistente.idCategoria = idCategoria;
                        platilloExistente.tipo = tipo;
                        platilloExistente.idEstado = estadoPlatillo;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        throw new Exception("El platillo no existe.");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception($"Error al modificar el platillo.");
            }
        }

        public static bool EliminarPlatillo(int idPlatillo)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    RecetaDAO.EliminarReceta(idPlatillo);

                    var platilloExistente = db.Platilloes.FirstOrDefault(p => p.idPlatillo == idPlatillo);
                    if (platilloExistente != null)
                    {
                        platilloExistente.idEstado = 3;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        throw new Exception("El platillo no existe.");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al eliminar el platillo.");
            }
        }
    }
}
