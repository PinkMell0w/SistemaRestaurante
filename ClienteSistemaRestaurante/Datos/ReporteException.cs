using System;
using System.Runtime.Serialization;

namespace ClienteSistemaRestaurante.Datos
{
    [Serializable]
    internal class ReporteException : Exception
    {
        public ReporteException()
        {
        }

        public ReporteException(string message) : base(message)
        {
        }

        public ReporteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReporteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}