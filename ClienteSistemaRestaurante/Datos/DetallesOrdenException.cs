using System;
using System.Runtime.Serialization;

namespace ClienteSistemaRestaurante.Datos
{
    [Serializable]
    internal class DetallesOrdenException : Exception
    {
        public DetallesOrdenException()
        {
        }

        public DetallesOrdenException(string message) : base(message)
        {
        }

        public DetallesOrdenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DetallesOrdenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}