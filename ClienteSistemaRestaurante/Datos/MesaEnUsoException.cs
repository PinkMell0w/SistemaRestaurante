using System;
using System.Runtime.Serialization;

namespace ClienteSistemaRestaurante.Datos
{
    [Serializable]
    internal class MesaEnUsoException : Exception
    {
        public MesaEnUsoException()
        {
        }

        public MesaEnUsoException(string message) : base(message)
        {
        }

        public MesaEnUsoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MesaEnUsoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}