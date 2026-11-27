using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class IngredienteDAO
    {
        public static List<Ingrediente> ObtenerTodos()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Ingredientes.Include(i => i.Medida).ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al cargar los ingredientes.");
            }
        }

        public static Ingrediente ObtenerIngrediente(int idIngrediente)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Ingredientes.Include(i => i.Medida).FirstOrDefault(i => i.idIngrediente == idIngrediente);
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al obtener el ingrediente solicitado.");
            }
        }

        public static Ingrediente ConsultarIngrediente(string nombre)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Ingredientes.Include(i => i.Medida).FirstOrDefault(i => i.nombre == nombre);
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar el ingrediente solicitado.");
            }
        }

        public static bool InsertarIngrediente(Ingrediente ingredienteNuevo)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    db.Ingredientes.Add(ingredienteNuevo);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                throw new Exception("Error de conexión al intentar registrar.");
            }
        }

        public static bool ModificarIngrediente(int idIngrediente, string nombre, int cantidad, int idMedida)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var ingrediente = db.Ingredientes.FirstOrDefault(i => i.idIngrediente == idIngrediente);
                    if (ingrediente != null)
                    {
                        ingrediente.nombre = nombre;
                        ingrediente.cantidad = cantidad;
                        ingrediente.idMedida = idMedida;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        throw new Exception("Ingrediente no encontrado.");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error de conexión al intentar actualizar.");
            }
        }

        public static int ObtenerCantidadActual(int idIngrediente)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var ingrediente = db.Ingredientes.FirstOrDefault(i => i.idIngrediente == idIngrediente);
                    return ingrediente != null ? ingrediente.cantidad : 0;
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al obtener la cantidad del ingrediente solicitado.");
            }
        }

        public static bool ActualizarInventario(int idIngrediente, int cantidadNueva)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var ingrediente = db.Ingredientes.FirstOrDefault(i => i.idIngrediente == idIngrediente);
                    if (ingrediente != null)
                    {
                        ingrediente.cantidad = cantidadNueva;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        throw new Exception("Ingrediente no encontrado.");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al actualizar el inventario del ingrediente.");
            }
        }
    }
}
