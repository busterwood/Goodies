using System;
using System.Runtime.Serialization;

namespace BusterWood.Testing
{
    /// <summary>Thrown when a <see cref="Test"/> is <see cref="Test.Skipped"/></summary>
    [Serializable]
    internal class SkipException : Exception
    {
        public SkipException()
        {
        }

        public SkipException(string message) : base(message)
        {
        }

        public SkipException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SkipException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}