using System;
using System.Runtime.Serialization;

namespace BusterWood.Testing
{
    /// <summary>Thrown when a <see cref="Test"/> has <see cref="Test.Failed"/></summary>
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