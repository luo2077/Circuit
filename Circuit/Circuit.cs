using System;
using System.Collections.Generic;

namespace Circuit
{
    /// <summary>
    /// 电路类，求解插入的所有支路电流
    /// </summary>
    internal class Circuit
    {
        //储存电路图所有的边
        private List<ArcNode<Branch>> _arcs;

        public Circuit()
        {
            _arcs = new List<ArcNode<Branch>>();
        }





        #region basic functions

        /// <summary>
        /// 向电路图中加入一条弧
        /// </summary>
        /// <param name="tVexId">弧为节点</param>
        /// <param name="hVexId">弧头节点</param>
        /// <param name="value">弧的权值</param>
        private Branch AddBranch(int tVexId, int hVexId, Branch value)
        {
            ArcNode<Branch> node = new ArcNode<Branch>()
            {
                TailVexId = tVexId,
                HeadVexId = hVexId,
                Value = value,
            };
            _arcs.Add(node);
            return node.Value;
        }

        /// <summary>
        /// 将连通的边分组
        /// </summary>
        /// <returns></returns>
        private List<List<ArcNode<Branch>>> GetSubConArcs()
        {
            //先用这些所有边构建一个图
            CDiagram graph = new CDiagram(2 * _arcs.Count);
            foreach (var arcNode in _arcs)
            {
                graph.InsertBranch(arcNode);
            }
            //通过优先遍历找出连通的点
            var vexId = graph.GetSubConGraphVexId();
            List<List<ArcNode<Branch>>> res = new List<List<ArcNode<Branch>>>(vexId.Count);
            for (int i = 0; i < vexId.Count; i++)
            {
                res.Add(new List<ArcNode<Branch>>());
            }
            int j = 0;
            //遍历所有连通图组的点
            foreach (List<int> vexIds in vexId)
            {
                //遍历这一组所有点
                foreach (int id in vexIds)
                {
                    //找与这个点有关的所有边，还没有加入边集的加入
                    foreach (var arc in _arcs)
                    {
                        if ((arc.TailVexId == id || arc.HeadVexId == id) && !res[j].Contains(arc))
                            res[j].Add(arc);
                    }
                }
                j++;
            }
            return res;
        }

        /// <summary>
        /// 通过连通边来构造连通的电路图
        /// </summary>
        /// <param name="conArcs">连通图的边的组</param>
        /// <returns></returns>
        private List<CDiagram> GetCDiagrams(List<List<ArcNode<Branch>>> conArcs)
        {
            List<CDiagram> res = new List<CDiagram>(conArcs.Count);
            CDiagram cd;
            for (int i = 0; i < conArcs.Count; i++)
            {
                cd = new CDiagram(2 * conArcs[i].Count);
                for (int j = 0; j < conArcs[i].Count; j++)
                {
                    cd.InsertBranch(conArcs[i][j]);
                }
                res.Add(cd);
            }
            return res;
        }

        /// <summary>
        /// 得到所有支路电流
        /// </summary>
        public void Solve()
        {
            if (_arcs.Count == 0) return;
            var res = GetCDiagrams(GetSubConArcs());
            for (int i = 0; i < res.Count; i++)
            {
                res[i].SolveCircuit();
            }
        }

        //打印
        public void Print()
        {
            Solve();
            foreach (var item in _arcs)
            {
                Console.WriteLine(item);
            }
        }

        #endregion





        #region extern functions

        /// <summary>
        /// 添加一个电阻
        /// </summary>
        /// <param name="tVexId">负极点id</param>
        /// <param name="hVexId">正极点id</param>
        /// <param name="value">阻值</param>
        public Branch AddR(int tVexId, int hVexId, double value)
        {
            return AddBranch(tVexId, hVexId, new Branch() { R = value, Type = BranchType.RESISTANCE });
        }

        /// <summary>
        /// 插入电池
        /// </summary>
        /// <param name="vexID1">负极点id</param>
        /// <param name="vexID2">正极点id</param>
        /// <param name="value"></param>
        public Branch AddE(int vexID1, int vexID2, double value)
        {
            return AddBranch(vexID1, vexID2, new Branch() { E = value, U = -value, Type = BranchType.BATTERY });
        }

        /// <summary>
        /// 插入电池(带内阻)
        /// </summary>
        /// <param name="vexID1">负极点id</param>
        /// <param name="vexID2">正极点id</param>
        /// <param name="rValue">电池内阻</param>
        /// <param name="value"></param>
        public Branch AddE(int vexID1, int vexID2, double eValue, double rValue)
        {
            return AddBranch(vexID1, vexID2, new Branch() { E = eValue, U = -eValue, R = rValue, Type = BranchType.BATTERY });
        }

        /// <summary>
        /// 插入导线
        /// </summary>
        /// <param name="vexID1">负极点id</param>
        /// <param name="vexID2">正极点id</param>
        public Branch AddWire(int vexID1, int vexID2)
        {
            return AddBranch(vexID1, vexID2, new Branch());
        }

        /// <summary>
        /// 插入电流表
        /// </summary>
        /// <param name="vexID1">负极点id</param>
        /// <param name="vexID2">正极id</param>
        public Branch AddAmmeter(int vexID1, int vexID2)
        {
            return AddBranch(vexID1, vexID2, new Branch() { Type = BranchType.AMMETER });
        }

        /// <summary>
        /// 插入电压表
        /// </summary>
        /// <param name="vexID1">负极点id1</param>
        /// <param name="vexID2">正极点id2</param>
        public Branch AddVoltMeter(int vexID1, int vexID2)
        {
            return AddBranch(vexID1, vexID2, new Branch() { R = double.MaxValue, Type = BranchType.VOLTMETER });
        }

        #endregion
    }
}
