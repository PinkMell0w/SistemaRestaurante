using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class DetalleOrdenDAO
    {
        public static bool InsertarDetalleOrden(DetalleOrden detalleOrden)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    db.DetalleOrdens.Add(detalleOrden);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                throw new DetallesOrdenException("Error al registrar los detalles de la orden.");
            }
        }

        public static bool ActualizarCantidadDetalle(int idDetalleOrden, int cantidadNuevaTotal)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var detalleOrden = db.DetalleOrdens.Find(idDetalleOrden);
                    if (detalleOrden != null)
                    {
                        detalleOrden.cantidadPlatillo = cantidadNuevaTotal;
                        detalleOrden.estado = "En espera";
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                throw new DetallesOrdenException("Error al actualizar los detalles de la orden.");
            }
        }

        public static bool ActualizarEstadoPlatillo(int idDetalleOrden, string nuevoEstado)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var detalleOrden = db.DetalleOrdens.Find(idDetalleOrden);
                    if (detalleOrden != null)
                    {
                        detalleOrden.estado = nuevoEstado;
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al actualizar el estado del platillo.");
            }
        }

        public static List<DetalleOrden> ObtenerDetallesOrden(int idOrden)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.DetalleOrdens.Include(d => d.Platillo).Where(d => d.idOrden == idOrden).ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al cargar los platillos de la orden.");
            }
        }
    }
}
