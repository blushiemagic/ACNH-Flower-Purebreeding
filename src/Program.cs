using System;

namespace AnimalCrossingFlowers
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide path to data text file");
                return;
            }
            string path = args[0];
            Data.Read(path);
            DrawUtils.Init();
            Output.Write(path);
        }
    }
}
