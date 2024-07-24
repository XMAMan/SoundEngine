using System;
using System.Linq;

namespace CmdTools
{
    class Program
    {
        //CreateCleanBat ..\..\..\..\ ..\..\..\..\..\Batch-Tools\
        //CountLineOfCodes ..\..\..\..\
        static void Main(string[] args)
        {
            Console.WriteLine(string.Join("\n", args.Select((x, i) => "[" + i + "]=" + x)));
            Console.WriteLine();

            if (args.Length < 2)
            {
                Console.WriteLine("Two args are needet: <CmdName> <Parameter1> <Optional-Parameter2>");
                Console.WriteLine("Example: CreateCleanBat ..\\..\\..\\..\\ ..\\..\\..\\..\\..\\Batch-Tools\\");
            }
            
            try
            {
                if (args[0] == "CreateCleanBat")
                {
                    CleanBatCreator.CreateCleanFile(args[1], args[2]);
                }
                if (args[0] == "CountLineOfCodes")
                {
                    Console.Write(LineOfCodeCounter.CountLineOfCodes(args[1]));
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            Console.WriteLine();
        }
    }
}
