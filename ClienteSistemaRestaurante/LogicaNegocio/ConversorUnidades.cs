using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteSistemaRestaurante.LogicaNegocio
{
    public class ConversorUnidades
    {
        public static int ConvertirABase(int cantidad, string unidad)
        {
            switch (unidad.ToLower())
            {
                case "kilogramos":
                    return cantidad * 1000;
                case "litros":
                    return cantidad * 1000;
                case "tazas":
                    return cantidad * 240;
                case "onzas":
                    return cantidad * 28;
                case "cucharadas":
                    return cantidad * 15;
                case "cucharaditas":
                    return cantidad * 5;
                default:
                    return cantidad;
            }
        }

        public static int ConvertirDeBase(int cantidad, string unidad)
        {
            switch (unidad.ToLower())
            {
                case "kilogramos":
                    return cantidad / 1000;
                case "litros":
                    return cantidad / 1000;
                case "tazas":
                    return cantidad / 240;
                case "onzas":
                    return cantidad / 28;
                case "cucharadas":
                    return cantidad / 15;
                case "cucharaditas":
                    return cantidad / 5;
                default:
                    return cantidad;
            }
        }
    }
}
