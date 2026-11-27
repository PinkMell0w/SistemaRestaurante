using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class PagoDAO
    {
        public static bool InsertarPago(Pago pago)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    db.Pagoes.Add(pago);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                throw new PagoException("Error al generar el pago.");
            }
        }

        public static List<Pago> ObtenerPagosPendientes()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Pagoes.Include(p => p.Orden).Include(p => p.Orden.Mesa).Include(p => p.Orden.Empleado).Include(p => p.TipoPago).Where(p => p.Orden.EstadoOrden.nombreEstado == "Servida" && p.estado == "Pendiente").ToList();
                }
            }
            catch (Exception)
            {
                throw new PagoException("Error al cargar los pagos.");
            }
        }

        public static List<Pago> ObtenerPagosPagadosPorRango(DateTime fechaInicio, DateTime fechaFinExclusiva)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Pagoes.Include(p => p.Orden).Include(p => p.Orden.Empleado).Include(p => p.Orden.DetalleOrdens.Select(d => d.Platillo)).Where(p => p.estado == "Pagado" && p.fechaPago >= fechaInicio && p.fechaPago < fechaFinExclusiva).ToList();
                }
            }
            catch (Exception)
            {
                throw new PagoException("Error al consultar los pagos del periodo.");
            }
        }

        public static bool ActualizarEstadoPago(int idPago, DateTime fechaPago, string estado)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var pago = db.Pagoes.Find(idPago);
                    if(pago != null)
                    {
                        pago.fechaPago = fechaPago;
                        pago.estado = estado;
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                throw new PagoException("Error al aceptar el pago.");
            }
        }
    }
}
