using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;

/******************************************************
 *                2024/04/05 20:10                    *  
 *                Arthur: Luo2077                     *
 *                  Version 1.0                       *                     
 *      这是一个关于电路的类,可构造直流基本电路求解电流   *
 *           This is a basic circuit class            *                                              
 *          © Nobody All Rights Reserved              *                          
 *              Open Source C# Code                   *                
 ******************************************************/

namespace CircuitDiagram
{
    internal class CDiagram : OLDNet<CPoint, Branch>
    {
        public CDiagram(int maxVexNum) : base(maxVexNum)
        {
        }

        #region 求解电路电流 solve circuit current

        /// <summary>
        /// 得到关联矩阵A
        /// </summary>
        /// <returns></returns>
        private Matrix<double> GetMatrixA()
        {
            //每个点都看做节点
            //行数为节点数，列数为支路数
            Matrix<double> matrix_A = Matrix.Build.Dense(_id2Vex.Count, _id2Arc.Count);
            foreach (var arc in _id2Arc.Values)
            {
                foreach (var vex in _id2Vex.Values)
                {
                    if (arc.TailVexId != vex.ID && arc.HeadVexId != vex.ID)
                        matrix_A[vex.ID, arc.Id] = 0;
                    else
                        matrix_A[vex.ID, arc.Id] = arc.TailVexId == vex.ID ? 1 : -1;
                }
            }
            //留下n - 1行独立kcl的方程
            Matrix<double> res = Matrix.Build.Dense(_id2Vex.Count - 1, _id2Arc.Count);
            for (int r = 0; r < res.RowCount; r++)
            {
                for (int c = 0; c < res.ColumnCount; c++)
                {
                    res[r, c] = matrix_A[r, c];
                }
            }
            return res;
        }

        /// <summary>
        /// 得到阻抗矩阵Z(对角阵)
        /// Z = Diag(z1,z2,...,zb)
        /// </summary>
        /// <returns></returns>
        private Matrix<double> GetMatrixZ()
        {
            Matrix<double> matrix_Z = Matrix.Build.Diagonal(_id2Arc.Count, _id2Arc.Count);
            foreach (var arc in _id2Arc.Values)
            {
                matrix_Z[arc.Id, arc.Id] = arc.Value.R;
            }
            return matrix_Z;
        }

        /// <summary>
        /// 得到支路电动势列向量()
        /// U = (u1,u2,...ub)^T
        /// </summary>
        /// <returns></returns>
        private Matrix<double> GetMatrixU()
        {
            Matrix<double> matrix_U = Matrix.Build.Dense(1, _id2Arc.Count);
            foreach (var arc in _id2Arc.Values)
            {
                matrix_U[0, arc.Id] = arc.Value.E;
            }
            return matrix_U.Transpose();
        }

        /// <summary>
        /// 回路矩阵B
        /// </summary>
        /// <returns></returns>
        private Matrix<double> GetMatrixB()
        {
            Matrix<double> matrix_B = Matrix.Build.Dense(_id2Arc.Count - _id2Vex.Count + 1, _id2Arc.Count);
            //得到基本回路
            var basicLoops = GetBasicLoops();

            //所有基本回路
            foreach (var loop in basicLoops)
            {
                //所有支路
                foreach (var branch in _id2Arc.Values)
                {
                    matrix_B[loop.Id, branch.Id] = loop.GetRelatedValue(branch);
                }
            }

            return matrix_B;
        }

        /// <summary>
        /// 得到电流列向量
        /// </summary>
        /// <returns></returns>
        public Matrix<double> GetMatrixI()
        {
            Matrix<double> A = GetMatrixA();
            Matrix<double> B = GetMatrixB();
            Matrix<double> Z = GetMatrixZ();
            Matrix<double> U = GetMatrixU();

            var left = Matrix.Build.DenseOfMatrixArray(new Matrix<double>[,] { { A }, { B * Z } });
            var rightUp = Matrix.Build.Dense(A.RowCount, 1);
            var right = Matrix.Build.DenseOfMatrixArray(new Matrix<double>[,] { { rightUp }, { B * U } });
            var res = left.LU().Solve(right);

            return res;
        }

        /// <summary>
        /// 得到树支
        /// </summary>
        /// <returns>树支</returns>
        private List<ArcNode<Branch>> GetBranchs()
        {
            //准备
            List<ArcNode<Branch>> res = new List<ArcNode<Branch>>();
            Stack<int> stack = new Stack<int>();
            ArcNode<Branch> adjArc;
            ResetIsVisited();

            //默认从第一个节点开始
            int vexId = 0;
            _isVisited[vexId] = true;
            stack.Push(vexId);

            while (stack.Count > 0)
            {
                vexId = stack.Pop();
                for (int adjVexId = FirstEdgeAdjVex(vexId, out adjArc); adjVexId != -1; adjVexId = NextEdgeAdjVex(vexId, adjArc, out adjArc))
                {
                    //如果没访问过这个点，就保留这条边，并且将点设置为已访问，并且将这条边对应点入栈
                    if (!_isVisited[adjVexId] && !stack.Contains(vexId))
                    {
                        res.Add(adjArc);
                        _isVisited[adjVexId] = true;
                        stack.Push(adjVexId);
                    }
                }
            }
            return res;

        }

        /// <summary>
        /// 得到连支
        /// </summary>
        /// <param name="branchs">树支</param>
        /// <returns></returns>
        private List<ArcNode<Branch>> GetLinks(List<ArcNode<Branch>> branchs)
        {
            List<ArcNode<Branch>> res = new List<ArcNode<Branch>>();
            foreach (ArcNode<Branch> branch in _id2Arc.Values)
            {
                if (!branchs.Contains(branch))
                    res.Add(branch);
            }
            return res;
        }

        /// <summary>
        /// 得到一条路径的边
        /// </summary>
        /// <param name="route">路径</param>
        /// <param name="isCorrelated">关联数组</param>
        /// <returns></returns>
        private List<ArcNode<Branch>> GetRouteArcs(List<int> route, out List<bool> isCorrelated)
        {
            //边对应的关联值
            isCorrelated = new List<bool>(route.Count - 1);
            bool temp = false;
            List<ArcNode<Branch>> res = new List<ArcNode<Branch>>();
            //从第一个点遍历到倒数第二个点
            for (int i = 0; i < route.Count - 1; i++)
            {
                //找到链接这两个点的弧
                res.Add(GetEdge(route[i], route[i + 1], out temp));
                isCorrelated.Add(temp);
            }
            return res;
        }

        /// <summary>
        /// 按照id的顺序，找到第一条链接vexID1和vexID2的弧
        /// isCorrelated为true:弧为<vexID1, vexID2>
        /// isCorrelated为false:弧为<vexID2, vexID1>
        /// </summary>
        /// <param name="vexID1">点id</param>
        /// <param name="vexID2">点id</param>
        /// <param name="isCorrelated">是否方向关联</param>
        /// <returns></returns>
        private ArcNode<Branch> GetEdge(int vexID1, int vexID2, out bool isCorrelated)
        {
            foreach (var arc in _id2Arc.Values)
            {
                //当成出边或者入边看看找得到不
                if (arc.TailVexId == vexID1 && arc.HeadVexId == vexID2)
                {
                    isCorrelated = true;
                    return arc;
                }
                else if (arc.TailVexId == vexID2 && arc.HeadVexId == vexID1)
                {
                    isCorrelated = false;
                    return arc;
                }

            }
            isCorrelated = false;
            return null;
        }

        /// <summary>
        /// 得到基本回路
        /// </summary>
        /// <returns></returns>
        private List<BasicLoop<Branch>> GetBasicLoops()
        {
            int loopID = 0;
            //基本回路
            List<BasicLoop<Branch>> loops = new List<BasicLoop<Branch>>();

            //连支
            var links = GetLinks(GetBranchs());

            //去掉电路图的所有连支，变成树
            foreach (ArcNode<Branch> link in links)
            {
                DeleteArc(link.TailVexId, link.HeadVexId, link.Id);
            }

            //找这一个连支的基本回路
            foreach (var link in links)
            {
                //找一个从连支头到尾的路径，由于是树，所以只有一条路径
                var route = GetRoute(link.HeadVexId, link.TailVexId);
                List<bool> isRelated;
                var arcs = GetRouteArcs(route[0], out isRelated);
                loops.Add(new BasicLoop<Branch>(route[0], isRelated, arcs, link, loopID++));
            }

            //把连支接回来
            foreach (var item in links)
            {
                InsertArcWithID(item.TailVexId, item.HeadVexId, item.Value, item.Id);
            }
            return loops;
        }

        /// <summary>
        /// 给各个弧的电流赋值
        /// </summary>
        public void SolveCircuit()
        {
            var currentVextor = GetMatrixI();
            foreach (var arc in _id2Arc.Values)
            {
                arc.Value.I = currentVextor[arc.Id, 0];
            }
        }

        #endregion





        #region 基本方法 basic functions

        /// <summary>
        /// 插入一条支路(如果点不存在就加点)
        /// </summary>
        /// <param name="vexId1">点id</param>
        /// <param name="vexId2">点id</param>
        /// <param name="value">权值</param>
        private void InsertBranch(int vexId1, int vexId2, Branch value)
        {
            //如果这两个点不存在那么就添加这两个点
            if (!_id2Vex.ContainsKey(vexId1))
                AddVex(vexId1);
            if (!_id2Vex.ContainsKey(vexId2))
                AddVex(vexId2);
            InsertArc(vexId1, vexId2, value);
        }

        /// <summary>
        /// 添加一个带有自动id的点(id从0开始生成)
        /// </summary>
        /// <param name="value">点的值</param>
        public void AddVex()
        {
            InsertVex(VexIdPool, new CPoint());
        }

        /// <summary>
        /// 添加一个带有指定id的点
        /// </summary>
        /// <param name="vexID">点id(0~int.MaxValue)</param>
        public void AddVex(int vexID)
        {
            InsertVex(vexID, new CPoint());
        }


        #endregion




        #region 扩展方法 external functions

        //插入电路元件(插入原件时如果元件端点不存在，则会先插入点，在插入原件)

        /// <summary>
        /// 插入电阻
        /// </summary>
        /// <param name="vexID1">点id</param>
        /// <param name="vexID2">点id</param>
        /// <param name="value">电阻值</param>
        public void InsertR(int vexID1, int vexID2, double value)
        {
            InsertBranch(vexID1, vexID2, new Branch() { R = value });
        }

        /// <summary>
        /// 插入电池
        /// </summary>
        /// <param name="vexID1">电池端点id</param>
        /// <param name="vexID2">电池端点id</param>
        /// <param name="value"></param>
        public void InsertE(int vexID1, int vexID2, double value)
        {
            InsertBranch(vexID1, vexID2, new Branch() { E = value });
        }

        /// <summary>
        /// 插入导线
        /// </summary>
        /// <param name="vexID1">导线端点id</param>
        /// <param name="vexID2">导线端点id</param>
        public void InsertWire(int vexID1, int vexID2)
        {
            InsertBranch(vexID1, vexID2, new Branch());
        }

        /// <summary>
        /// 插入电流表
        /// </summary>
        /// <param name="vexID1">电流表端点id</param>
        /// <param name="vexID2">电流表端点id</param>
        public void InsertAmmeter(int vexID1, int vexID2)
        {
            InsertWire(vexID1, vexID2);
        }

        /// <summary>
        /// 插入电压表
        /// </summary>
        /// <param name="vexID1">电压表端点id1</param>
        /// <param name="vexID2">电压表端点id2</param>
        public void InsertVoltMeter(int vexID1, int vexID2)
        {
            InsertR(vexID1, vexID2, double.MaxValue);
        }

        #endregion
    }
}
