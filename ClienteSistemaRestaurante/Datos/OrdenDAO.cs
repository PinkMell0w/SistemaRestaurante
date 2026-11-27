using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class OrdenDAO
    {
        public static int InsertarOrden(Orden orden)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    db.Ordens.Add(orden);
                    db.SaveChanges();
                    return orden.idOrden;
                }
            }
            catch (Exception)
            {
                throw new OrdenException("Error al crear la orden.");
            }
        }

        public static List<Orden> ObtenerOrdenesServidas()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Ordens.Include(o => o.EstadoOrden).Include(o => o.Mesa).Include(o => o.Empleado).Where(o => o.EstadoOrden.nombreEstado == "Servida" && !db.Pagoes.Any(p => p.idOrden == o.idOrden)).ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al cargar las órdenes.");
            }
        }

        public static List<Orden> ObtenerOrdenesActivas(int idEmpleado)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Ordens.Include(o => o.EstadoOrden).Include(o => o.Mesa).Where(o => o.EstadoOrden.nombreEstado != "Finalizada" && o.idEmpleado == idEmpleado).ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al cargar las órdenes.");
            }
        }

        public static List<Orden> ObtenerOrdenesCocina()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Ordens.Include(o => o.EstadoOrden).Include(o => o.Mesa).Where(o => o.EstadoOrden.nombreEstado == "En espera" || o.EstadoOrden.nombreEstado == "En preparación" || o.EstadoOrden.nombreEstado == "Preparada").ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al cargar las órdenes.");
            }
        }

        public static bool ActualizarEstadoOrden(int idOrden, int idEstado)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var orden = db.Ordens.Find(idOrden);
                    if (orden != null)
                    {
                        orden.idEstado = idEstado;
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                throw new OrdenException("Error al actualizar el estado de la orden.");
            }
        }
    }
}
