using ClienteSistemaRestaurante.Modelos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.Datos
{
    public class UsuarioDAO
    {
        public static void InicializarUsuariosSistema()
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var passwordHasher = new PasswordHasher<object>();

                    if (!db.Usuarios.Any(u => u.nombreUsuario == "usuarioCocina"))
                    {
                        var usuarioCocinero = new Usuario
                        {
                            nombreUsuario = "usuarioCocina",
                            contrasenia = passwordHasher.HashPassword(null, "SGR_cocina80")
                        };
                        db.Usuarios.Add(usuarioCocinero);
                    }

                    if (!db.Usuarios.Any(u => u.nombreUsuario == "usuarioGerente"))
                    {
                        var usuarioGerente = new Usuario
                        {
                            nombreUsuario = "usuarioGerente",
                            contrasenia = passwordHasher.HashPassword(null, "SGR_admin2026")
                        };
                        db.Usuarios.Add(usuarioGerente);
                    }

                    if (!db.Usuarios.Any(u => u.nombreUsuario == "usuarioHost"))
                    {
                        var usuarioHost = new Usuario
                        {
                            nombreUsuario = "usuarioHost",
                            contrasenia = passwordHasher.HashPassword(null, "SGR_host60")
                        };
                        db.Usuarios.Add(usuarioHost);
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al inicializar usuarios del sistema.");
            }
            
        }
        public static int InsertarUsuario(string nombreUsuario, string hashedPassword)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    var nuevoUsuario = new Usuario
                    {
                        nombreUsuario = nombreUsuario,
                        contrasenia = hashedPassword
                    };

                    db.Usuarios.Add(nuevoUsuario);
                    db.SaveChanges();

                    return nuevoUsuario.idUsuario;
                }
            }
            catch (Exception)
            {
                throw new Exception("Error al insertar usuario.");
            }
            
        }

        public static Usuario ConsultarUsuarioPorNombre(string nombreUsuario)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Usuarios.FirstOrDefault(u => u.nombreUsuario == nombreUsuario);
                }
            }
            catch (Exception)
            {
                throw new Exception("Error de conexión con la base de datos.");
            }
            
        }

        public static Empleado ConsultarEmpleadoPorIdUsuario(int idUsuario)
        {
            try
            {
                using (var db = new SistemaRestauranteEntities())
                {
                    return db.Empleadoes.Include(e => e.Rol).FirstOrDefault(e => e.idUsuario == idUsuario);
                }
            }
            catch (Exception)
            {
                throw new Exception("Error de conexión con la base de datos.");
            }
        }
    }
}
