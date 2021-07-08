using System;
using System.Runtime.Serialization;

namespace SqlPrivilegeManager
{
    public class SqlPrivilegeException : Exception
    {
        public SqlPrivilegeException()
        {
        }

        public SqlPrivilegeException(string message) : base(message)
        {
        }

        public SqlPrivilegeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqlPrivilegeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
