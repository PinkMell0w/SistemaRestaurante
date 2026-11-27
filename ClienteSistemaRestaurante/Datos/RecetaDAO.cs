using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class RecetaDAO
    {
        public static List<Receta> ObtenerReceta(int idPlatillo)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Recetas.Where(r => r.idPlatillo == idPlatillo).Include(r => r.Ingrediente).Include(r => r.Ingrediente.Medida).ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar la receta.");
            }
        }

        public static bool InsertarReceta(Receta receta)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "EXEC sp_InsertarReceta @idPlatillo, @idIngrediente, @cantidadRequerida",
                        new SqlParameter("@idPlatillo", receta.idPlatillo),
                        new SqlParameter("@idIngrediente", receta.idIngrediente),
                        new SqlParameter("@cantidadRequerida", receta.cantidadRequerida)
                    );
                    return true;
                }
            }
            catch (SqlException)
            {
                throw new Exception("Error al insertar la receta.");
            }
        }

        public static bool EliminarReceta(int idPlatillo)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var recetaExistente = db.Recetas.FirstOrDefault(r => r.idPlatillo == idPlatillo);
                    if (recetaExistente != null)
                    {
                        db.Database.ExecuteSqlCommand(
                            "EXEC sp_EliminarRecetasPorPlatillo @idPlatillo",
                            new SqlParameter("@idPlatillo", idPlatillo)
                        );
                        return true;
                    }
                    else
                    {
                        throw new Exception("La receta no existe.");
                    }
                }
            }
            catch (SqlException)
            {
                throw new Exception("Error al eliminar la receta.");
            }
        }
    }
}
