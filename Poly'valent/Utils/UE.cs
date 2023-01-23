namespace Poly_valent.Utils
{
    internal class UE
    {
        public string? _code;
        public string? _name;
        public float? _ECTS;
        public float? _grade;
        public float? _avg;
        public string? _rank;
        public string? _result;

        public UE(string? code, string? name, float? ECTS, float? grade, float? avg, string? rank, string? result)
        {
            _code = code;
            _name = name;
            _ECTS = ECTS;
            _grade = grade;
            _avg = avg;
            _rank = rank;
            _result = result;
        }

        public override string ToString()
        {
            return $"{_code} - {_name}\nECTS: {(_ECTS >= 0 ? _ECTS : "-")}, Note: {(_grade >= 0 ? _grade : "-")}, Moyenne: {(_avg >= 0 ? _avg : "-")}, Rang: {_rank}\nRésultat: {_result}\n";
        }
    }
}
