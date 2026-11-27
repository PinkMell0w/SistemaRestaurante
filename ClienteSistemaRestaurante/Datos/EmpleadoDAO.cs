using ClienteSistemaRestaurante.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class EmpleadoDAO
    {
        public static Empleado ConsultarEmpleadoPorRfc(string rfc)
        {
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
					return db.Empleadoes.FirstOrDefault(e => e.rfc == rfc);
                }
			}
			catch (Exception)
			{
				throw new Exception("Error al consultar el empleado por RFC.");
			}
        }

        public static List<Empleado> ObtenerEmpleadosActivos()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Empleadoes.Include(e => e.Rol).Where(e => e.estado == true).ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar los empleados.");
            }
        }

        public static List<Empleado> ObtenerEmpleadosConRol(bool estado, int idRol)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Empleadoes.Include(e => e.Rol).Where(e => e.Rol.idRol == idRol && e.estado == estado).ToList();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al consultar los empleados con el rol seleccionado.");
            }
        }

        public static bool InsertarEmpleado(Empleado nuevoEmpleado)
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
				{
					db.Empleadoes.Add(nuevoEmpleado);
					db.SaveChanges();
                    return true;
                }
			}
			catch (Exception)
			{
				throw new Exception("Error de conexión al intentar registrar.");
			}
		}

        public static bool ModificarEmpleado(int idEmpleado, string nombres, string apellidoPaterno, string apellidoMaterno, int rol)
		{
			try
			{
				using (var db = new SistemaRestauranteEntities())
                {
                    var empleadoExistente = db.Empleadoes.FirstOrDefault(e => e.idEmpleado == idEmpleado);
                    if (empleadoExistente != null)
                    {
                        empleadoExistente.nombres = nombres;
                        empleadoExistente.apellidoPaterno = apellidoPaterno;
                        empleadoExistente.apellidoMaterno = apellidoMaterno;
                        empleadoExistente.idRol = rol;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        throw new Exception("Empleado no encontrado.");
                    }
                }
            }
			catch (Exception)
			{
				throw new Exception("Error de conexión al intentar actualizar.");
			}
		}

		public static bool EliminarEmpleado(int idEmpleado)
		{
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var empleadoExistente = db.Empleadoes.FirstOrDefault(e => e.idEmpleado == idEmpleado);
                    if (empleadoExistente != null)
                    {
                        empleadoExistente.estado = false;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        throw new Exception("Empleado no encontrado.");
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("Error de conexión al intentar eliminar.");
            }
        }
    }
}
