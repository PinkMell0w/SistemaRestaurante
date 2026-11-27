using System;
using System.Runtime.Serialization;

namespace ClienteSistemaRestaurante.Datos
{
    [Serializable]
    internal class MesaException : Exception
    {
        public MesaException()
        {
        }

        public MesaException(string message) : base(message)
        {
        }

        public MesaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MesaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}