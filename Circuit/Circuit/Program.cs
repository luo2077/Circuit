using System;

namespace Circuit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 验证单次计算结果正确性
            Circuit c = new Circuit();
            c.Add(0, 2, new Branch() { R = 3, E = -12, Name = "i1" });
            c.Add(0, 2, new Branch() { R = 3, Name = "i2" });
            c.Add(1, 2, new Branch() { R = 2, Name = "i3" });
            c.Add(2, 1, new Branch() { R = 2, E = 8, Name = "i4" });
            c.Add(0, 1, new Branch() { R = 1.5, Name = "i5" });
            c.Solve();
            Console.WriteLine("=== 基准结果验证 ===");
            c.Print();

            // 预热
            Run50Branches(5);
            Run120Branches(5);

            // 50 支路测试
            int it50 = 500;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Run50Branches(it50);
            sw.Stop();
            Console.WriteLine($"\n[50支路电路 - FastSolver MNA] 执行 {it50} 次: {sw.ElapsedMilliseconds} ms, 平均每次: {sw.Elapsed.TotalMilliseconds / it50:F3} ms ({sw.Elapsed.TotalMicroseconds / it50:F1} µs)");

            // 120 支路测试
            int it120 = 200;
            sw.Restart();
            Run120Branches(it120);
            sw.Stop();
            Console.WriteLine($"[120支路电路 - FastSolver MNA] 执行 {it120} 次: {sw.ElapsedMilliseconds} ms, 平均每次: {sw.Elapsed.TotalMilliseconds / it120:F3} ms ({sw.Elapsed.TotalMicroseconds / it120:F1} µs)");
        }

        static void Run50Branches(int count)
        {
            for (int k = 0; k < count; k++)
            {
                Circuit c = new Circuit();
                for (int i = 0; i < 24; i++)
                    c.Add(i, i + 1, new Branch() { R = 2, E = (i == 0 ? 10 : 0), Name = $"b_{i}" });
                for (int i = 0; i < 12; i++)
                    c.Add(i, i + 12, new Branch() { R = 3, Name = $"cross_{i}" });
                for (int i = 0; i < 14; i++)
                    c.Add(i, (i + 5) % 25, new Branch() { R = 4, Name = $"diag_{i}" });
                c.Solve();
            }
        }

        static void Run120Branches(int count)
        {
            for (int k = 0; k < count; k++)
            {
                Circuit c = new Circuit();
                for (int i = 0; i < 49; i++)
                    c.Add(i, i + 1, new Branch() { R = 2, E = (i == 0 ? 10 : 0), Name = $"b_{i}" });
                for (int i = 0; i < 35; i++)
                    c.Add(i, (i + 15) % 50, new Branch() { R = 3, Name = $"c_{i}" });
                for (int i = 0; i < 36; i++)
                    c.Add(i, (i + 25) % 50, new Branch() { R = 4, Name = $"d_{i}" });
                c.Solve();
            }
        }
    }
}
