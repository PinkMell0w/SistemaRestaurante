using ClienteSistemaRestaurante.Datos;
using ClienteSistemaRestaurante.Modelos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.LogicaNegocio
{
    public class CreadorUsuario
    {

        public bool RegistrarEmpleadoConUsuario(Empleado nuevoEmpleado, string nombreRol)
        {
            try
            {
                int idUsuarioAsignado;
                string rolLower = nombreRol.Trim().ToLower();

                if (rolLower == "cocinero")
                {
                    var usuarioCocinero = UsuarioDAO.ConsultarUsuarioPorNombre("usuarioCocina");

                    if (usuarioCocinero != null)
                    {
                        idUsuarioAsignado = usuarioCocinero.idUsuario;
                    }
                    else
                    {
                        throw new Exception("No se encontró el usuario compartido para cocineros en la base de datos. Por favor, créelo previamente.");
                    }
                }
                else
                {
                    string prefijoRol = (rolLower == "cajero") ? "cj" : "ms";
                    string nombreEmpleado = nuevoEmpleado.nombres.Trim().Split(' ')[0];
                    string nombreUsuario = prefijoRol + nombreEmpleado;

                    var passwordHasher = new PasswordHasher<object>();
                    string hashedPassword = passwordHasher.HashPassword(null, nuevoEmpleado.rfc.Trim());

                    idUsuarioAsignado = UsuarioDAO.InsertarUsuario(nombreUsuario, hashedPassword);
                }

                nuevoEmpleado.idUsuario = idUsuarioAsignado;
                nuevoEmpleado.fechaRegistro = DateTime.Now;

                return EmpleadoDAO.InsertarEmpleado(nuevoEmpleado);
            }
            catch (SqlException)
            {
                throw new Exception("Error de conexión al intentar crear el usuario.");
            }
        }
    }
}
