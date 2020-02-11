using System;

namespace ConsoleApplication1.Exceptions
{
    public class EmptyConfigFieldException : Exception
    {
        public EmptyConfigFieldException(string message) : base(message)
        {
        }
    }
}