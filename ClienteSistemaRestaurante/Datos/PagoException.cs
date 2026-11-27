using System;
using System.Runtime.Serialization;

namespace ClienteSistemaRestaurante.Datos
{
    [Serializable]
    internal class PagoException : Exception
    {
        public PagoException()
        {
        }

        public PagoException(string message) : base(message)
        {
        }

        public PagoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PagoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}