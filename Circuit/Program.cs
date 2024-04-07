using System;

namespace Circuit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //简单电路
            Circuit c = new Circuit();
            c.AddR(0, 1, 2.5);
            c.AddAmmeter(1, 2);
            c.AddE(2, 3, 10);
            c.AddWire(3, 0);
            c.AddVoltMeter(0, 1);
            c.Print();
            Console.ReadKey();
        }
    }
}
