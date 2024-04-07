namespace Circuit
{
    /// <summary>
    /// 弧
    /// </summary>
    /// <typeparam name="TArcValue">弧的数据类型</typeparam>
    public class ArcNode<TArcValue>
    {
        private int _id;
        private TArcValue _value;
        private int _tailVexId, _headVexId;
        private ArcNode<TArcValue> _nextHLink, _nextTLink, _lastHLink, _lastTLink;
        private bool _isVisited;

        /// <summary>
        /// 弧的权值类型
        /// </summary>
        public TArcValue Value { get => _value; set => _value = value; }

        /// <summary>
        /// 弧的唯一ID(只用于区分弧)
        /// </summary>
        public int Id { get => _id; set => _id = value; }

        /// <summary>
        /// 弧尾点ID
        /// </summary>
        public int TailVexId { get => _tailVexId; set => _tailVexId = value; }

        /// <summary>
        /// 弧头点ID
        /// </summary>
        public int HeadVexId { get => _headVexId; set => _headVexId = value; }

        /// <summary>
        /// 是否被访问(在找树支时用过)
        /// </summary>
        public bool IsVisited { get => _isVisited; set => _isVisited = value; }

        /// <summary>
        /// 下一条与自己弧头相同的边(也就是同一个点的入边)
        /// </summary>
        internal ArcNode<TArcValue> NextHLink { get => _nextHLink; set => _nextHLink = value; }

        /// <summary>
        /// 下一个与自己弧尾相同的边(也就是同一个点的出边)
        /// </summary>
        internal ArcNode<TArcValue> NextTLink { get => _nextTLink; set => _nextTLink = value; }

        /// <summary>
        /// 上一条与自己弧头相同的边(双向链表)
        /// </summary>
        internal ArcNode<TArcValue> LastHLink { get => _lastHLink; set => _lastHLink = value; }

        /// <summary>
        /// 上一条与自己弧尾相同的边(双向链表)
        /// </summary>
        internal ArcNode<TArcValue> LastTLink { get => _lastTLink; set => _lastTLink = value; }

        //设置自己的访问状态(用于别的地方回调，其实遍历设置_id2Arc也可以)
        public void SetVisitedState(bool isVisited)
        {
            IsVisited = isVisited;
        }

        public override string ToString()
        {
            return $"v{TailVexId}->{HeadVexId}\t(ID={Id},value:{Value})";
        }

    }
}
