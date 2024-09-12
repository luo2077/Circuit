using System;

namespace Circuit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //instantiate Circuit
            Circuit c = new Circuit();
            //i1
            c.Add(0, 2, new Branch() { R = 3, E = -12, Name = "i1"});
            //i2
            c.Add(0, 2, new Branch() { R = 3, Name = "i2" });
            //i3
            c.Add(1, 2, new Branch() { R = 2, Name = "i3" });
            //i4
            c.Add(2, 1, new Branch() { R = 2, E = 8, Name = "i4" });
            //i5
            c.Add(0, 1, new Branch() { R = 1.5, Name = "i5" });

            //print the result
            c.Print();

            Console.ReadKey();
        }
    }
}
