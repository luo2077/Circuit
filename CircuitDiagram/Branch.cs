namespace CircuitDiagram
{
    internal class Branch
    {
        private double r, e, i;

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

        public override string ToString()
        {
            return $"R:{R}\tE:{E}\tI:{I}";
        }
    }
}
