using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Circuit
{
    /// <summary>
    /// 有向网(有多重边，无自环)
    /// </summary>
    /// <typeparam name="TVexValue">点的数据类型</typeparam>
    /// <typeparam name="TArcValue">边的数据类型</typeparam>
    public class OLDNet<TVexValue, TArcValue>
    {
        //索引与点的字典
        internal Dictionary<int, VexNode<TVexValue, TArcValue>> _id2Vex;
        //索引与边的字典
        internal Dictionary<int, ArcNode<TArcValue>> _id2Arc;
        //点的访问表
        internal bool[] _isVisited;

        //顶点数量最大值
        internal int _maxVexNum = 0;

        /// <summary>
        /// 点的数量
        /// </summary>
        public int VexNum
        {
            get { return _id2Vex.Count; }
        }

        //用来生成独一无二的索引
        internal int _arcIdPool, _vexIdPool;

        /// <summary>
        /// 得到一个独一无二的边ID
        /// </summary>
        public int ArcIdPool
        {
            get
            {
                if (_arcIdPool == int.MaxValue) return -1;
                while (_id2Arc.ContainsKey(++_arcIdPool));
                return _arcIdPool;
            }
        }

        /// <summary>
        /// 得到一个独一无二的点ID
        /// </summary>
        public int VexIdPool
        {
            get
            {
                if (_vexIdPool == int.MaxValue) return -1;
                while (_id2Vex.ContainsKey(++_vexIdPool)) ;
                return _vexIdPool;
            }
        }

        //用于回调边的访问状态
        private delegate void SetState(bool state);
        private event SetState stateChanged;

        //仅用于遍历
        protected delegate void TraverseFunc(int index, Action<int> action, FirstAdjVex firstAdjVex, NextAdjVex nextAdjVex);
        protected delegate int FirstAdjVex(int vexId, out ArcNode<TArcValue> firstArc);
        protected delegate int NextAdjVex(int vexId, ArcNode<TArcValue> lastArc, out ArcNode<TArcValue> firstArc);

        #region basic functions

        //构造函数
        public OLDNet(int maxVexNum)
        {
            _isVisited = new bool[maxVexNum];
            _id2Vex = new Dictionary<int, VexNode<TVexValue, TArcValue>>();
            _vexIdPool = _arcIdPool = -1;
            _id2Arc = new Dictionary<int, ArcNode<TArcValue>>();
        }

        //点
        /// <summary>
        /// 返回点的值
        /// </summary>
        /// <param name="id">点的索引</param>
        /// <returns></returns>
        protected TVexValue GetVex(int id)
        {
            return _id2Vex[id].Value;
        }

        /// <summary>
        /// 给点赋值
        /// </summary>
        /// <param name="id">点的索引</param>
        /// <param name="value">值</param>
        protected void PutVex(int id, TVexValue value)
        {
            _id2Vex[id].Value = value;
        }

        /// <summary>
        /// 插入点
        /// </summary>
        /// <param name="data">点值</param>
        /// <param name="index">点的索引</param>
        protected void InsertVex(int index, TVexValue data)
        {
            _id2Vex.Add(index, new VexNode<TVexValue, TArcValue>() { Value = data, ID = index });
        }

        /// <summary>
        /// 删除点及其对应边
        /// </summary>
        /// <param name="vexId">点的id</param>
        protected void DeleteVex(int vexId)
        {
            //删除所有出边
            ArcNode<TArcValue> arc;
            while ((arc = _id2Vex[vexId].FirstOut) != null)
            {
                DeleteArc(arc.TailVexId, arc.HeadVexId, arc.Id);
            }
            //删除所有入边
            while ((arc = _id2Vex[vexId].FirstIn) != null)
            {
                DeleteArc(arc.TailVexId, arc.HeadVexId, arc.Id);
            }
            //删除点
            _id2Vex.Remove(vexId);
        }

        /// <summary>
        /// 点的第一个出弧邻接点
        /// </summary>
        /// <param name="id">点的id</param>
        /// <returns></returns>
        protected virtual int FirstOutArcAdjVex(int id, out ArcNode<TArcValue> firstOutAdjArc)
        {
            if ((firstOutAdjArc = _id2Vex[id].FirstOut) != null)
                return _id2Vex[id].FirstOut.HeadVexId;
            return -1;
        }

        /// <summary>
        /// 点的第一条入弧邻接点
        /// </summary>
        /// <param name="vexId">点id</param>
        /// <param name="firstInAdjArc">第一条入弧</param>
        /// <returns></returns>
        protected virtual int FirstInArcAdjVex(int vexId, out ArcNode<TArcValue> firstInAdjArc)
        {
            if ((firstInAdjArc = _id2Vex[vexId].FirstIn) != null)
                return _id2Vex[vexId].FirstIn.TailVexId;
            return -1;
        }

        /// <summary>
        /// 点的下一个出弧邻接点
        /// </summary>
        /// <param name="vexId">点id</param>
        /// <param name="lastOutAdjArc">上一条出弧</param>
        /// <param name="nextOutAdjArc">下一条出弧</param>
        /// <returns></returns>
        protected virtual int NextOutArcAdjVex(int vexId, ArcNode<TArcValue> lastOutAdjArc, out ArcNode<TArcValue> nextOutAdjArc)
        {
            //如果还有下一条出边
            if ((nextOutAdjArc = lastOutAdjArc.NextTLink) != null)
                return nextOutAdjArc.HeadVexId;
            else
                return -1;
        }

        /// <summary>
        /// 点的下一个入弧邻接点
        /// </summary>
        /// <param name="vexId">点id</param>
        /// <param name="lastInAdjArc">上一条入弧</param>
        /// <param name="nextInAdjArc">下一条入弧</param>
        /// <returns></returns>
        protected virtual int NextInArcAdjVex(int vexId, ArcNode<TArcValue> lastInAdjArc, out ArcNode<TArcValue> nextInAdjArc)
        {
            //如果还有下一条入边
            if ((nextInAdjArc = lastInAdjArc.NextHLink) != null)
                return nextInAdjArc.TailVexId;
            else
                return -1;
        }

        /// <summary>
        /// 点的第一条边邻接点
        /// </summary>
        /// <param name="vexId">点的id</param>
        /// <param name="firstAdjEdge">第一条边</param>
        /// <returns></returns>
        protected virtual int FirstEdgeAdjVex(int vexId, out ArcNode<TArcValue> firstAdjEdge)
        {
            int res = -1;
            //如果没有出边邻接点
            if ((res = FirstOutArcAdjVex(vexId, out firstAdjEdge)) != -1)
                return res;
            else
                return FirstInArcAdjVex(vexId, out firstAdjEdge);
        }

        /// <summary>
        /// 点的下一条邻接边
        /// </summary>
        /// <param name="vexId">点的id</param>
        /// <param name="lastArcEdge">上一条邻接边</param>
        /// <param name="nextAdjEdge">下一条邻接边</param>
        /// <returns></returns>
        protected virtual int NextEdgeAdjVex(int vexId, ArcNode<TArcValue> lastArcEdge, out ArcNode<TArcValue> nextAdjEdge)
        {
            bool isFind = false;
            nextAdjEdge = null;

            foreach (ArcNode<TArcValue> arc in _id2Vex[vexId])
            {
                //如果上次找到了上一条邻接边
                if (isFind)
                {
                    nextAdjEdge = arc;
                    break;
                }
                isFind = (arc == lastArcEdge);
            }

            //如果找到了
            if (isFind && nextAdjEdge != null)
            {
                //如果是出边
                if (nextAdjEdge.TailVexId == vexId)
                    return nextAdjEdge.HeadVexId;
                else
                    return nextAdjEdge.TailVexId;
            }
            else
            {
                //没找到
                return -1;
            }
        }

        //弧
        /// <summary>
        /// 插入一个弧(自动生成id)
        /// </summary>
        /// <param name="tVexId">弧尾点id</param>
        /// <param name="hVexId">弧头点id</param>
        /// <param name="value">弧的数据</param>
        protected void InsertArc(int tVexId, int hVexId, TArcValue value)
        {
            //插入弧
            var arc = new ArcNode<TArcValue>()
            {
                TailVexId = tVexId,
                HeadVexId = hVexId,
                Value = value,
                Id = ArcIdPool
            };

            //头插入tVex邻接表
            if (_id2Vex[tVexId].FirstOut != null)
                _id2Vex[tVexId].FirstOut.LastTLink = arc;
            arc.NextTLink = _id2Vex[tVexId].FirstOut;
            _id2Vex[tVexId].FirstOut = arc;

            //头插入hVex的逆邻接表
            if (_id2Vex[hVexId].FirstIn != null)
                _id2Vex[hVexId].FirstIn.LastHLink = arc;
            arc.NextHLink = _id2Vex[hVexId].FirstIn;
            _id2Vex[hVexId].FirstIn = arc;

            //点的出入度更新
            _id2Vex[tVexId].OutDegree++;
            _id2Vex[hVexId].InDegree++;

            //更新边列表
            _id2Arc.Add(arc.Id, arc);

            //订阅事件
            stateChanged += arc.SetVisitedState;
        }

        /// <summary>
        /// 加入一个边结点(自动id)
        /// </summary>
        /// <param name="arcNode">边结点</param>
        protected void InsertArcNode(ArcNode<TArcValue> arcNode)
        {
            //把关系清零
            arcNode.NextHLink = null;
            arcNode.NextTLink = null;
            arcNode.LastTLink = null;
            arcNode.LastHLink = null;
            arcNode.Id = ArcIdPool;
            //头插入tVex邻接表
            if (_id2Vex[arcNode.TailVexId].FirstOut != null)
                _id2Vex[arcNode.TailVexId].FirstOut.LastTLink = arcNode;
            arcNode.NextTLink = _id2Vex[arcNode.TailVexId].FirstOut;
            _id2Vex[arcNode.TailVexId].FirstOut = arcNode;

            //头插入hVex的逆邻接表
            if (_id2Vex[arcNode.HeadVexId].FirstIn != null)
                _id2Vex[arcNode.HeadVexId].FirstIn.LastHLink = arcNode;
            arcNode.NextHLink = _id2Vex[arcNode.HeadVexId].FirstIn;
            _id2Vex[arcNode.HeadVexId].FirstIn = arcNode;

            //点的出入度更新
            _id2Vex[arcNode.TailVexId].OutDegree++;
            _id2Vex[arcNode.HeadVexId].InDegree++;

            //更新边列表
            _id2Arc.Add(arcNode.Id, arcNode);

            //订阅事件
            stateChanged += arcNode.SetVisitedState;
        }

        /// <summary>
        /// 插入一个弧，并设置id
        /// </summary>
        /// <param name="tVexId">弧尾点id</param>
        /// <param name="hVexId">弧头点id</param>
        /// <param name="value">弧的权值</param>
        /// <param name="id">弧的id</param>
        protected void InsertArcWithID(int tVexId, int hVexId, TArcValue value, int id)
        {
            //插入弧
            var arc = new ArcNode<TArcValue>()
            {
                TailVexId = tVexId,
                HeadVexId = hVexId,
                Value = value,
                Id = id
            };

            //头插入tVex
            if (_id2Vex[tVexId].FirstOut != null)
                _id2Vex[tVexId].FirstOut.LastTLink = arc;
            arc.NextTLink = _id2Vex[tVexId].FirstOut;
            _id2Vex[tVexId].FirstOut = arc;

            //头插入hVex的逆邻接表
            if (_id2Vex[hVexId].FirstIn != null)
                _id2Vex[hVexId].FirstIn.LastHLink = arc;
            arc.NextHLink = _id2Vex[hVexId].FirstIn;
            _id2Vex[hVexId].FirstIn = arc;

            //点的出入度更新
            _id2Vex[tVexId].OutDegree++;
            _id2Vex[hVexId].InDegree++;

            //更新边列表
            _id2Arc.Add(arc.Id, arc);

            //订阅事件
            stateChanged += arc.SetVisitedState;
        }

        /// <summary>
        /// 删除一个弧，如果弧存在就删除，不存在什么也不干
        /// </summary>
        /// <param name="tVexId">弧尾点索引</param>
        /// <param name="hVexId">弧头点索引</param>
        /// <param name="id">弧的id</param>
        protected void DeleteArc(int tVexId, int hVexId, int id)
        {
            //找边
            ArcNode<TArcValue> arc = null;
            //找边
            foreach (ArcNode<TArcValue> arci in _id2Vex[tVexId])
            {
                if (arci.Id == id)
                {
                    arc = arci;
                    break;
                }
            }

            //删除边
            if (arc != null)
            {
                //出边链表
                //弧无前驱，无后继
                if (arc.LastTLink == null && arc.NextTLink == null)
                {
                    _id2Vex[tVexId].FirstOut = null;
                }
                //弧无前驱，有后继
                else if (arc.LastTLink == null)
                {
                    _id2Vex[tVexId].FirstOut = arc.NextTLink;
                    arc.NextTLink.LastTLink = null;
                }
                //弧有前驱，无后继
                else if (arc.NextTLink == null)
                {
                    arc.LastTLink.NextTLink = null;
                }
                //弧有前驱，有后继
                else
                {
                    arc.LastTLink.NextTLink = arc.NextTLink;
                    arc.NextTLink.LastTLink = arc.LastTLink;
                }

                //入边链表
                //弧无前驱，无后继
                if (arc.LastHLink == null && arc.NextHLink == null)
                {
                    _id2Vex[hVexId].FirstIn = null;
                }
                //弧无前驱，有后继
                else if (arc.LastHLink == null)
                {
                    _id2Vex[hVexId].FirstIn = arc.NextHLink;
                    arc.NextHLink.LastHLink = null;
                }
                //弧有前驱，无后继
                else if (arc.NextHLink == null)
                {
                    arc.LastHLink.NextHLink = null;
                }
                //弧有前驱，有后继
                else
                {
                    arc.LastHLink.NextHLink = arc.NextHLink;
                    arc.NextHLink.LastHLink = arc.LastHLink;
                }

                //点的出入度更新
                _id2Vex[tVexId].OutDegree--;
                _id2Vex[hVexId].InDegree--;

                //更新边列表
                _id2Arc.Remove(arc.Id);

                //订阅事件
                stateChanged -= arc.SetVisitedState;
            }


        }

        //遍历
        /// <summary>
        /// 深度优先遍历一个顶点(递归)
        /// </summary>
        /// <param name="id">访问起点id</param>
        /// <param name="action">访问方法</param>
        private void DFS_R(int id, Action<int> action, FirstAdjVex firstAdjVex, NextAdjVex nextAdjVex)
        {
            //先访问邻接点
            if (_isVisited[GetVexIndexById(id)]) return;
            action(id);
            //标记为已访问
            _isVisited[GetVexIndexById(id)] = true;
            ArcNode<TArcValue> tempArc;
            //遍历所有出边邻接点
            for (int vi = firstAdjVex(id, out tempArc); vi != -1; vi = nextAdjVex(id, tempArc, out tempArc))
            {
                if (!_isVisited[GetVexIndexById(vi)])
                    DFS_R(vi, action, firstAdjVex, nextAdjVex);
            }
        }

        /// <summary>
        /// 深度优先遍历一个顶点(非递归)
        /// </summary>
        /// <param name="index">起点</param>
        /// <param name="action">访问方法</param>
        protected void DFS_NR(int index, Action<int> action, FirstAdjVex firstAdjVex, NextAdjVex nextAdjVex)
        {
            //遍历邻接点
            Stack<int> stack = new Stack<int>();
            ArcNode<TArcValue> tempArc;

            stack.Push(index);
            while (stack.Count > 0)
            {
                index = stack.Pop();
                action(index);
                _isVisited[GetVexIndexById(index)] = true;

                //将这个点未入栈且未访问的所有邻接点全部入栈
                for (int vi = firstAdjVex(index, out tempArc); vi != -1; vi = nextAdjVex(index, tempArc, out tempArc))
                {
                    if (!_isVisited[GetVexIndexById(vi)] && !stack.Contains(vi))
                    {
                        stack.Push(vi);
                    }
                }
            }

        }

        /// <summary>
        /// 广度优先遍历一个顶点(非递归)
        /// </summary>
        /// <param name="index">起点</param>
        /// <param name="action">访问方法</param>
        protected void BFS_NR(int index, Action<int> action, FirstAdjVex firstAdjVex, NextAdjVex nextAdjVex)
        {
            action(index);
            _isVisited[GetVexIndexById(index)] = true;
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(index);
            ArcNode<TArcValue> tempArc;
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                //检查邻接点
                for (int i = firstAdjVex(v, out tempArc); i >= 0; i = nextAdjVex(v, tempArc, out tempArc))
                {
                    if (!_isVisited[GetVexIndexById(i)])
                    {
                        action(i);
                        _isVisited[GetVexIndexById(i)] = true;
                        queue.Enqueue(i);
                    }
                }
            }
        }

        /// <summary>
        /// 遍历并访问所有顶点
        /// </summary>
        /// <param name="traverse">遍历方法</param>
        /// <param name="action">访问方法</param>
        protected void Traverse(TraverseFunc traverse, Action<int> action, FirstAdjVex firstAdjVex, NextAdjVex nextAdjVex)
        {
            ResetIsVisited();
            foreach (int vexId in _id2Vex.Keys)
            {
                traverse(vexId, action, firstAdjVex, nextAdjVex);
            }
        }

        //两点简单路径
        /// <summary>
        /// 找从某点到某点的路径
        /// </summary>
        /// <param name="startVexID">起点</param>
        /// <param name="endVexID">终点</param>
        /// <returns></returns>
        protected List<List<int>> GetRoute(int startVexID, int endVexID)
        {
            //(0)准备工作
            //返回结果路径
            List<List<int>> res = new List<List<int>>();

            //主栈、储存路径
            Stack<int> route = new Stack<int>();
            //辅栈、储存主栈元素对应支路点
            Stack<List<int>> branchVex = new Stack<List<int>>();
            //临时边
            ArcNode<TArcValue> tempArc;


            //(1)首次建栈
            route.Push(startVexID);
            //临时辅栈元素
            List<int> tempAdjVex = new List<int>();
            //遍历该点所有邻接点
            for (int adjVexID = FirstEdgeAdjVex(startVexID, out tempArc); adjVexID != -1; adjVexID = NextEdgeAdjVex(startVexID, tempArc, out tempArc))
            {
                tempAdjVex.Add(adjVexID);
            }
            //邻接点入辅栈
            branchVex.Push(tempAdjVex);

            //只要主栈不为空
            while (route.Count > 0)
            {
                //获取邻接列表
                tempAdjVex = branchVex.Peek();

                //(2)如果邻接点列表不为空，继续建栈
                if (tempAdjVex.Count > 0)
                {
                    //将第一个邻接点入栈
                    route.Push(tempAdjVex[0]);
                    //移出第一个元素
                    tempAdjVex.RemoveAt(0);
                    startVexID = route.Peek();

                    var temp = new List<int>();
                    //遍历该点所有邻接点
                    for (int adjVexID = FirstEdgeAdjVex(startVexID, out tempArc); adjVexID != -1; adjVexID = NextEdgeAdjVex(startVexID, tempArc, out tempArc))
                    {
                        //剔除路径里已有的点
                        if (!route.Contains(adjVexID) && !temp.Contains(adjVexID))
                            temp.Add(adjVexID);
                    }
                    //入辅栈
                    branchVex.Push(temp);
                }
                //(3)削栈
                else
                {
                    route.Pop();
                    branchVex.Pop();
                    continue;
                }

                //如果此时主栈顶就是目标元素
                if (route.Peek() == endVexID)
                {
                    //储存这个路径
                    res.Add(route.ToList());
                    res.Last().Reverse();
                    //削栈
                    route.Pop();
                    branchVex.Pop();
                }
            }
            return res;
        }

        //重写
        /// <summary>
        /// 字符表示图的全貌
        /// </summary>
        /// <returns>表示图的全貌的字符</returns>
        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append($"顶点数量：{VexNum}\n");
            foreach (int i in _id2Vex.Keys)
            {
                res.Append($"v{i}:{_id2Vex[i].Value}\n");
            }

            res.Append($"\n边的数量:{_id2Arc.Count}\n");
            //边
            ArcNode<TArcValue> arc = null;

            foreach (int i in _id2Vex.Keys)
            {
                res.Append($"v{i}:Indegree:{_id2Vex[i].InDegree}\tOutDegree:{_id2Vex[i].OutDegree}\n");
                res.Append($"v{i}.Out:\t");
                arc = _id2Vex[i].FirstOut;
                while (arc != null)
                {
                    res.Append($"v{i}->v{arc.HeadVexId}:ID:{arc.Id}\t");
                    arc = arc.NextTLink;
                }
                res.Append($"\nv{i}.In:\t");
                arc = _id2Vex[i].FirstIn;
                while (arc != null)
                {
                    res.Append($"v{i}<-v{arc.TailVexId}:ID:{arc.Id}\t");
                    arc = arc.NextHLink;
                }
                res.Append("\n\n");
            }

            return res.ToString();
        }

        #endregion




        #region extern functions

        /// <summary>
        /// 添加一个带有自动id的点
        /// </summary>
        /// <param name="value">点的值</param>
        protected void AddVex(TVexValue value)
        {
            InsertVex(VexIdPool, value);
        }

        /// <summary>
        /// 设置边的访问状态
        /// </summary>
        /// <param name="state"></param>
        protected void SetVisitedStateOfAllArcs(bool state)
        {
            stateChanged(state);
        }

        /// <summary>
        /// 将访问表设置为全部未访问
        /// </summary>
        protected void ResetIsVisited()
        {
            for (int i = 0; i < _isVisited.Length; i++)
            {
                _isVisited[i] = false;
            }
        }

        /// <summary>
        /// 由点id得到点字典序号
        /// </summary>
        /// <param name="id">点id</param>
        /// <returns></returns>
        protected int GetVexIndexById(int id)
        {
            int i = 0;
            foreach (var vex in _id2Vex.Keys)
            {
                if (vex == id)
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// 由弧id得到弧字典序号
        /// </summary>
        /// <param name="id">弧id</param>
        /// <returns></returns>
        protected int GetArcIndexById(int id)
        {
            int i = 0;
            foreach (var vex in _id2Arc.Keys)
            {
                if (vex == id)
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// 通过字典序号(非键值，就是代表第几个的序号)得到点id
        /// </summary>
        /// <param name="index">点id</param>
        /// <returns></returns>
        protected int GetVexIdByIndex(int index)
        {
            int i = 0;
            foreach (var item in _id2Vex.Keys)
            {
                if (i == index) return item;
                i++;
            }
            return -1;
        }



        #endregion

    }
}
