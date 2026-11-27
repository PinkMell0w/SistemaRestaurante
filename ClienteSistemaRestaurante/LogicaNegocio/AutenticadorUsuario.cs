using ClienteSistemaRestaurante.Datos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.LogicaNegocio
{
    public class AutenticadorUsuario
    {
        public string Autenticar(string nombreUsuario, string passwordIngresado, out object datosUsuario, out string mensajeError)
        {
            try
            {
                datosUsuario = null;
                mensajeError = string.Empty;
                string username = nombreUsuario.Trim();

                var usuario = UsuarioDAO.ConsultarUsuarioPorNombre(username);
                if (usuario == null)
                {
                    datosUsuario = null;
                    mensajeError = "Usuario o contraseña incorrectos.";
                    return null;
                }

                var passwordHasher = new PasswordHasher<object>();
                var resultadoVerificacion = passwordHasher.VerifyHashedPassword(null, usuario.contrasenia, passwordIngresado);

                if (resultadoVerificacion == PasswordVerificationResult.Failed)
                {
                    datosUsuario = null;
                    mensajeError = "Usuario o contraseña incorrectos.";
                    return null;
                }

                if (username == "usuarioGerente") return "Gerente";
                if (username == "usuarioHost") return "Host";
                if (username == "usuarioCocina") return "Cocinero";

                var empleado = UsuarioDAO.ConsultarEmpleadoPorIdUsuario(usuario.idUsuario);
                if (empleado != null && empleado.Rol != null)
                {
                    if (empleado.estado == false)
                    {
                        datosUsuario = null;
                        mensajeError = "No tiene permisos para acceder.";
                        return null;
                    }

                    datosUsuario = empleado;
                    return empleado.Rol.nombreRol;
                }

                mensajeError = "Usuario o contraseña incorrectos.";
                return null;

            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;
                datosUsuario = null;
                return null;
            }
        }
    }
}
