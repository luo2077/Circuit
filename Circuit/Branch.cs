namespace Circuit
{
    /// <summary>
    /// 电路原件类型
    /// </summary>
    public enum BranchType
    {
        WIRE,
        RESISTANCE,
        BATTERY,
        AMMETER,
        VOLTMETER
    }

    internal class Branch
    {
        private BranchType _type;
        private double r, e, i, u;

        /// <summary>
        /// 电阻值
        /// </summary>
        public double R { get => r; set => r = value; }

        /// <summary>
        /// 电动势值
        /// </summary>
        public double E { get => e; set => e = value; }

        /// <summary>
        /// 电流值
        /// </summary>
        public double I { get => i; set => i = value; }

        /// <summary>
        /// 电压值
        /// </summary>
        public double U { get => u; set => u = value; }
        public BranchType Type { get => _type; set => _type = value; }

        public override string ToString()
        {
            if (R == double.MaxValue)
                return $"\tType:{_type}\tR:∞\tE:{E}\tI:0\tU:{U}\t";
            return $"\tType:{_type}\tR:{R}\tE:{E}\tI:{I}\tU:{U}\t";
        }
    }
}
