using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class ReporteDAO
    {
        public static bool InsertarReporte(Reporte nuevoReporte)
        {
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
                    DateTime inicioBuscado = nuevoReporte.fechaInicio.Date;
                    DateTime finBuscado = nuevoReporte.fechaFin.Date;

                    var reporteExistente = db.Reportes.FirstOrDefault(r =>
                        r.tipo == nuevoReporte.tipo &&
                        DbFunctions.TruncateTime(r.fechaInicio) == inicioBuscado &&
                        DbFunctions.TruncateTime(r.fechaFin) == finBuscado);

                    if (reporteExistente != null)
                    {
                        reporteExistente.tipo = nuevoReporte.tipo;
                        reporteExistente.fechaInicio = nuevoReporte.fechaInicio;
                        reporteExistente.fechaFin = nuevoReporte.fechaFin;
                    }
                    else
                    {
                        db.Reportes.Add(nuevoReporte);
                    }

                    db.SaveChanges();
                    return true;
                }
			}
			catch (Exception)
			{
				throw new ReporteException("Error de conexión al generar el reporte.");
			}
        }

        public static List<Reporte> ObtenerReportesDeTipo(string tipo, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    DateTime inicio = fechaInicio.Date;
                    DateTime fin = fechaFin.Date;

                    return db.Reportes.Where(r => r.tipo == tipo && DbFunctions.TruncateTime(r.fechaInicio) >= inicio && DbFunctions.TruncateTime(r.fechaFin) < fin).OrderByDescending(r => r.idReporte).ToList();
                }
            }
            catch (Exception)
            {
                throw new ReporteException("Error de conexión al obtener los datos para el reporte.");
            }
        }
    }
}
