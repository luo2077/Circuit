using System.Collections;
using System.Text;

namespace Circuit
{
    /// <summary>
    /// 点节点
    /// </summary>
    /// <typeparam name="T1">点的数据类型</typeparam>
    /// <typeparam name="T2">边的数据类型</typeparam>
    public class VexNode<T1, T2> : IEnumerable, IEnumerator
    {
        private int _id, _inDegree, _outDegree, _record;
        private T1 _value;
        private ArcNode<T2> _firstIn, _firstOut, _currentArc;

        /// <summary>
        /// 点的值类型
        /// </summary>
        public T1 Value { get => _value; set => _value = value; }

        /// <summary>
        /// 点的唯一ID(只用于区分点)
        /// </summary>
        public int ID { get => _id; set => _id = value; }

        /// <summary>
        /// 点的入度
        /// </summary>
        public int InDegree { get => _inDegree; set => _inDegree = value; }

        /// <summary>
        /// 点的出度
        /// </summary>
        public int OutDegree { get => _outDegree; set => _outDegree = value; }

        /// <summary>
        /// 点的度
        /// </summary>
        public int Degree { get => _inDegree + _outDegree; }

        /// <summary>
        /// 点的第一条入弧
        /// </summary>
        internal ArcNode<T2> FirstIn { get => _firstIn; set => _firstIn = value; }

        /// <summary>
        /// 点的第一条出弧
        /// </summary>
        internal ArcNode<T2> FirstOut { get => _firstOut; set => _firstOut = value; }



        #region interface 实现接口用来遍历所有邻接弧(出弧 + 入弧)
        object IEnumerator.Current
        {
            get
            {
                return _currentArc;
            }
        }

        /// <summary>
        /// 遍历自己所有的邻接边
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            ((IEnumerator)this).Reset();
            return this;
        }

        bool IEnumerator.MoveNext()
        {
            //如果是第一次步进
            if (_record == -1)
            {
                _record++;
                //如果还有出边
                if (FirstOut != null)
                {
                    _currentArc = FirstOut;
                    return true;
                }
                //如果没有出边，如果有入边
                else if (FirstIn != null)
                {
                    _currentArc = FirstIn;
                    return true;
                }
                return false;
            }
            //如果不是第一次步进，上一次步进的边是出边
            else if (_currentArc.TailVexId == ID)
            {
                //如果出边步进成功
                if ((_currentArc = _currentArc.NextTLink) != null)
                    return true;
                //出边步进失败，如果入边步进成功
                else if ((_currentArc = FirstIn) != null)
                    return true;
                //出边步进失败，并且入边步进失败
                else
                    return false;

            }
            //上一次步进的边是入边，并且入边步进成功
            else if ((_currentArc = _currentArc.NextHLink) != null)
                return true;
            else
                return false;
        }

        void IEnumerator.Reset()
        {
            _record = -1;
            _currentArc = FirstOut;
        }

        #endregion

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.Append($"v{ID}:Outdegree:{OutDegree}\tInDegree:{InDegree}\n");
            res.Append($"v{ID}.Out:\t");
            var arc = FirstOut;
            while (arc != null)
            {
                res.Append($"v{ID}->v{arc.HeadVexId}:({arc.Id})\t");
                arc = arc.NextTLink;
            }
            res.Append($"\nv{ID}.In:\t");
            arc = FirstIn;
            while (arc != null)
            {
                res.Append($"v{ID}<-v{arc.TailVexId}:({arc.Id})\t");
                arc = arc.NextHLink;
            }
            return res.ToString();
        }
    }


}
