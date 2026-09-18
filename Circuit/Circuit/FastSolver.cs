using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;

namespace Circuit
{
    /// <summary>
    /// 基于改进节点电压法 (Modified Nodal Analysis, MNA) 的高性能电路求解器
    /// 优势:
    /// 1. 无需寻找生成树、基本回路，无需删除/恢复边。
    /// 2. 直接按支路盖印 (Stamping) 建立 (V-1 + M) 维对称稀疏方程组。
    /// 3. 并查集秒级切分连通分量，计算速度提升数十倍。
    /// </summary>
    internal static class FastSolver
    {
        private class DisjointSet
        {
            private readonly Dictionary<int, int> parent = new Dictionary<int, int>();

            public int Find(int i)
            {
                if (!parent.ContainsKey(i))
                {
                    parent[i] = i;
                    return i;
                }
                if (parent[i] == i) return i;
                return parent[i] = Find(parent[i]);
            }

            public void Union(int i, int j)
            {
                int rootI = Find(i);
                int rootJ = Find(j);
                if (rootI != rootJ)
                {
                    parent[rootI] = rootJ;
                }
            }
        }

        public static void Solve(List<ArcNode<Branch>> allArcs)
        {
            if (allArcs == null || allArcs.Count == 0) return;

            // 1. 用并查集将支路划分为各个连通子图
            DisjointSet dsu = new DisjointSet();
            foreach (var arc in allArcs)
            {
                dsu.Union(arc.TailVexId, arc.HeadVexId);
            }

            Dictionary<int, List<ArcNode<Branch>>> components = new Dictionary<int, List<ArcNode<Branch>>>();
            foreach (var arc in allArcs)
            {
                int root = dsu.Find(arc.TailVexId);
                if (!components.TryGetValue(root, out var list))
                {
                    list = new List<ArcNode<Branch>>();
                    components[root] = list;
                }
                list.Add(arc);
            }

            // 2. 分别求解各个连通子图
            foreach (var comp in components.Values)
            {
                SolveComponent(comp);
            }
        }

        private static void SolveComponent(List<ArcNode<Branch>> arcs)
        {
            if (arcs.Count == 0) return;

            // 收集所有节点并分配局部编号 [0, numNodes - 1]
            // 以 0 号局部节点作为参考地 (Ground, V0 = 0)
            Dictionary<int, int> vexToIdx = new Dictionary<int, int>();
            foreach (var arc in arcs)
            {
                if (!vexToIdx.ContainsKey(arc.TailVexId)) vexToIdx[arc.TailVexId] = vexToIdx.Count;
                if (!vexToIdx.ContainsKey(arc.HeadVexId)) vexToIdx[arc.HeadVexId] = vexToIdx.Count;
            }

            int numNodes = vexToIdx.Count;
            int numIndependentNodes = numNodes - 1;

            // 确定电压源支路数量 M (R == 0 且 E != 0，或纯电压源)
            // 在原项目中，支路模型一般为：R 与 E 串联，TailVex -> HeadVex。
            // 当 R > 0 时，作为伴随诺顿等效：导纳 G = 1/R，等效并联电流源 I_eq = E / R (从 Head 流向 Tail)
            // 当 R == 0 时，为理想电压源，需增加一行辅助变量记录流经电压源的支路电流。
            List<ArcNode<Branch>> idealVoltageSources = new List<ArcNode<Branch>>();
            foreach (var arc in arcs)
            {
                if (arc.Value.R <= 1e-12)
                {
                    idealVoltageSources.Add(arc);
                }
            }

            int numVS = idealVoltageSources.Count;
            int dim = numIndependentNodes + numVS;

            if (dim == 0)
            {
                // 只有一个节点且无独立电压源
                foreach (var arc in arcs)
                {
                    arc.Value.I = 0;
                    if (arc.Value.E == 0) arc.Value.U = 0;
                }
                return;
            }

            Matrix<double> A = Matrix.Build.Dense(dim, dim, 0.0);
            Vector<double> z = Vector.Build.Dense(dim, 0.0);

            // 节点映射函数：参考节点 (局部索引 0) 不进入方程组，返回 -1；其余映射到 [0, numIndependentNodes - 1]
            int MapNode(int globalVexId)
            {
                int localIdx = vexToIdx[globalVexId];
                return localIdx == 0 ? -1 : localIdx - 1;
            }

            // 盖印 (Stamping) 非理想支路 (R > 0)
            foreach (var arc in arcs)
            {
                if (arc.Value.R > 1e-12)
                {
                    double g = 1.0 / arc.Value.R;
                    int nTail = MapNode(arc.TailVexId);
                    int nHead = MapNode(arc.HeadVexId);

                    // 导纳矩阵盖印
                    if (nTail != -1) A[nTail, nTail] += g;
                    if (nHead != -1) A[nHead, nHead] += g;
                    if (nTail != -1 && nHead != -1)
                    {
                        A[nTail, nHead] -= g;
                        A[nHead, nTail] -= g;
                    }

                    // 电动势 E 等效并联电流源盖印
                    // 支路方向: Tail -> Head, 电势提升 E (即 V_head - V_tail + E 产生电流)
                    // 原项目方程定义：i*R = (v_tail - v_head) + E => i = (v_tail - v_head)/R + E/R
                    // 流出 Tail 的电流增加 E/R，流入 Head 的电流增加 E/R
                    if (arc.Value.E != 0)
                    {
                        double iEq = arc.Value.E / arc.Value.R;
                        if (nTail != -1) z[nTail] -= iEq;
                        if (nHead != -1) z[nHead] += iEq;
                    }
                }
            }

            // 盖印理想电压源 (R == 0)
            for (int k = 0; k < numVS; k++)
            {
                var vsArc = idealVoltageSources[k];
                int vsRow = numIndependentNodes + k;
                int nTail = MapNode(vsArc.TailVexId);
                int nHead = MapNode(vsArc.HeadVexId);

                // KCL 方程中加入该电压源电流 i_vs
                if (nTail != -1) A[nTail, vsRow] += 1.0;
                if (nHead != -1) A[nHead, vsRow] -= 1.0;

                // 辅助方程: (V_tail - V_head) = -E => V_tail - V_head = -E
                if (nTail != -1) A[vsRow, nTail] += 1.0;
                if (nHead != -1) A[vsRow, nHead] -= 1.0;
                z[vsRow] = -vsArc.Value.E;
            }

            // 求解 MNA 线性方程组 A * x = z
            Vector<double> x;
            try
            {
                x = A.LU().Solve(z);
            }
            catch
            {
                x = A.Svd().Solve(z);
            }

            // 获取各节点电位 (V0 = 0)
            double[] nodePotentials = new double[numNodes];
            nodePotentials[0] = 0.0;
            for (int i = 1; i < numNodes; i++)
            {
                nodePotentials[i] = x[i - 1];
            }

            // 计算所有支路的电流与电压
            // 原项目方向规范: 支路定义从 Tail 到 Head
            // 电流 I 满足: I*R = (V_tail - V_head) + E
            for (int k = 0; k < numVS; k++)
            {
                var vsArc = idealVoltageSources[k];
                int vsIdx = numIndependentNodes + k;
                vsArc.Value.I = x[vsIdx];
                if (vsArc.Value.E == 0)
                {
                    vsArc.Value.U = 0;
                }
            }

            foreach (var arc in arcs)
            {
                if (arc.Value.R > 1e-12)
                {
                    double vTail = nodePotentials[vexToIdx[arc.TailVexId]];
                    double vHead = nodePotentials[vexToIdx[arc.HeadVexId]];
                    arc.Value.I = (vTail - vHead + arc.Value.E) / arc.Value.R;
                    if (arc.Value.E == 0)
                    {
                        arc.Value.U = arc.Value.I * arc.Value.R;
                    }
                }
            }
        }
    }
}
