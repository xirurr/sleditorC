using System;

namespace ConsoleApplication1.Exceptions
{
    public class LackOfInformationException : Exception
    {
        public LackOfInformationException(String message):base(message) {
        }
    }
}