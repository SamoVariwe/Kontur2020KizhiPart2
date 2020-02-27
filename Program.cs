using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System;

namespace KizhiPart2
{
    class Program
    {
       
        static void Main()
        {
            TextWriter Writer = Console.Out;
            Interpreter itpr = new Interpreter(Writer);
            string str = null;
            while (true)
            {
                str = Console.ReadLine();
                if (str == "stop") break;
                itpr.ExecuteLine(str);
            }
            

        }
        
    }
    
}