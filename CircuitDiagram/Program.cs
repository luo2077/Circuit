using System;

namespace CircuitDiagram
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //简单示例
            CDiagram cd = new CDiagram(10);

            //加入四个点
            for (int i = 0; i < 4; i++)
                cd.AddVex();

            //加电路元件
            cd.InsertE(0, 3, 10);
            cd.InsertR(1, 0, 2.5);
            cd.InsertVoltMeter(1, 0);
            cd.InsertAmmeter(1, 2);
            cd.InsertWire(2, 3);
            
            //求解
            cd.SolveCircuit();
            foreach (var i in cd._id2Arc.Values)
            {
                Console.WriteLine(i);
            }

            //结果完全正确
            Console.ReadKey();
        }
    }
}
