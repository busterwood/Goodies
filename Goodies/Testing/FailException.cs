using System;
using System.Runtime.Serialization;

namespace BusterWood.Testing
{
    [Serializable]
    internal class FailException : Exception
    {
        public FailException()
        {
        }

        public FailException(string message) : base(message)
        {
        }

        public FailException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FailException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}