namespace Poly_valent.Utils
{
    internal class Module
    {
        public string? _Name { get; set; }
        public string? _ModuleID { get; set;}
        public float? _Coef { get; set; }
        public string? _Block { get; set;}
        public float? _Grade { get; set; }
        public float? _Avg { get; set; }
        public string? _Rank { get; set; }
        public string? _S { get; set; }

        public Module(string? name, string? moduleID, float? coef, string? block, float? grade, float? avg, string? rank, string? s)
        {
            _Name = name;
            _ModuleID = moduleID;
            _Coef = coef;
            _Block = block;
            _Grade = grade;
            _Avg = avg;
            _Rank = rank;
            _S = s;
        }

        public override string ToString()
        {
            return $"{_ModuleID} - {_Name}\nCoefficient: {_Coef}, Note: {_Grade}, Moyenne: {_Avg}, Rang: {_Rank}\n{_Block} - {_S}";
        }
    }

}
