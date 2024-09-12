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
        VOLTMETER,
        Complex
    }

    internal class Branch
    {
        private string name;
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

        /// <summary>
        /// 给这条支路取名
        /// </summary>
        public string Name { get => name; set => name = value; }
        public BranchType Type { get => _type; set => _type = value; }

        public override string ToString()
        {
            return $"Name:{name}\tType:{_type}\tR:{R}Ω\tE:{E}V\tI:{I}A\tU:{U}V\t";
        }
    }
}
