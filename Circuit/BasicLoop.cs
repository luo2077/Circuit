using System.Collections.Generic;
using System.Text;

namespace Circuit
{
    /// <summary>
    /// 基本回路Basic loop
    /// </summary>
    internal class BasicLoop<TArcValue>
    {
        private List<int> _vex;
        private List<ArcNode<TArcValue>> _arcs;
        private List<bool> _isRelated;
        private ArcNode<TArcValue> _link;
        private int _id;

        /// <summary>
        /// 构造一条回路
        /// </summary>
        /// <param name="vex">回路点集</param>
        /// <param name="isRelated">边与回路的方向关联情况</param>
        /// <param name="arcs">回路的边集</param>
        /// <param name="link">基本回路对应的连支</param>
        public BasicLoop(List<int> vex, List<bool> isRelated, List<ArcNode<TArcValue>> arcs, ArcNode<TArcValue> link, int id)
        {
            Vex = vex;
            IsRelated = isRelated;
            Arcs = arcs;
            Link = link;
            Id = id;
        }

        /// <summary>
        /// 一个回路的点集，暗含了方向
        /// </summary>
        public List<int> Vex { get => _vex; set => _vex = value; }

        /// <summary>
        /// 边与回路的方向关联情况
        /// </summary>
        public List<bool> IsRelated { get => _isRelated; set => _isRelated = value; }

        /// <summary>
        /// 回路的唯一标识
        /// </summary>
        public int Id { get => _id; set => _id = value; }

        /// <summary>
        /// 回路的支路，暗含方向
        /// </summary>
        internal List<ArcNode<TArcValue>> Arcs { get => _arcs; set => _arcs = value; }

        /// <summary>
        /// 基本回路关联的连支
        /// </summary>
        internal ArcNode<TArcValue> Link { get => _link; set => _link = value; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"ID:\t{Id}\n");

            sb.Append("Vex:\t");
            foreach (var item in Vex)
                sb.Append(item + "\t\t");
            sb.Append('\n');

            sb.Append("Arcs:\t");
            foreach (var item in Arcs)
                sb.Append(item + "\t");
            sb.Append("\n");

            sb.Append("Rted:\t");
            foreach (var item in IsRelated)
                sb.Append(item + "\t\t");
            sb.Append("\n");

            sb.Append("link:\t");
            sb.Append(Link);
            sb.Append('\n');
            return sb.ToString();
        }

        /// <summary>
        /// 得到支路与此回路到的关联值，用于构造回路矩阵B
        /// branch与此回路关联：1(方向一致)、-1(方向相反)
        /// branch与子回路无关：0
        /// </summary>
        /// <param name="branch">支路</param>
        /// <returns></returns>
        public int GetRelatedValue(ArcNode<TArcValue> branch)
        {
            //如果就是回路的连支
            if (branch.Id == _link.Id)
            {
                return 1;
            }
            else
            {
                //遍历所有边
                for (int i = 0; i < _arcs.Count; i++)
                {
                    //如果找到了边
                    if (branch == _arcs[i])
                    {
                        if (IsRelated[i])
                        {
                            return 1;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }

                //无关
                return 0;
            }
        }

    }
}
