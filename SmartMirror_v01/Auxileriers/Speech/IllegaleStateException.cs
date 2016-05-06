using System;

namespace SmartMirror.Auxileriers.Speech
{
    internal class IllegaleStateException : Exception
    {
        public IllegaleStateException()
        {
        }

        public IllegaleStateException(string message) : base(message)
        {
        }

        public IllegaleStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}