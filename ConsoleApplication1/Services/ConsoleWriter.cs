using System;

namespace ConsoleApplication1.Services
{
    public class ConsoleWriter
    {
       public static void WriteError(string message)
        {
            Console.ForegroundColor  = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor(); 
        }
       
       public static void WriteSuccess(string message)
       {
           Console.ForegroundColor  = ConsoleColor.Green;
           Console.WriteLine(message);
           Console.ResetColor(); 
       }
       
       public static void WriteDanger(string message)
       {
           Console.ForegroundColor  = ConsoleColor.Yellow;
           Console.WriteLine(message);
           Console.ResetColor(); 
       }
    }
}