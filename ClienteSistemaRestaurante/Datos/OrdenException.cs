using System;
using System.Runtime.Serialization;

namespace ClienteSistemaRestaurante.Datos
{
    [Serializable]
    internal class OrdenException : Exception
    {
        public OrdenException()
        {
        }

        public OrdenException(string message) : base(message)
        {
        }

        public OrdenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OrdenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}