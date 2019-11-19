using System;
using System.Runtime.Serialization;

namespace DSFramework.Exceptions
{
    public class DSFrameworkException : Exception
    {
        public DSFrameworkException()
        { }

        public DSFrameworkException(string message)
            : base(message)
        { }

        public DSFrameworkException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public DSFrameworkException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        { }
    }
}