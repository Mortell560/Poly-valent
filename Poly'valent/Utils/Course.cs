namespace Poly_valent.Utils
{
    internal class Course
    {
        private List<string> months = new() { "janvier", "février", "mars", "avril", "mai", "juin", "juillet", "août", "septembre", "octobre", "novembre", "décembre" };

        public string? _name;
        public string? _subject;
        public string? _subject_id;

        public float? _grade;
        public string? _date_str;
        public string? _appr;
        public float? _class_avg;
        public string? _rank;
        public DateTime? _date;

        public Course(string? name, string? subject, string? subject_id, float? grade, float? class_avg, string? rank ,string date_str, string? appr)
        {
            _name = name;
            _subject = subject;
            _subject_id = subject_id;
            _grade = grade;
            _date_str = date_str;
            _date = _ConvertDate(date_str);
            _appr = appr;
            _class_avg = class_avg;
            _rank = rank;
        }

        private DateTime _ConvertDate(string date_str)
        {
            string[] a = date_str.Split(" ");
            DateTime d = new DateTime(int.Parse(a[2]), months.IndexOf(a[1])+1, int.Parse(a[0]));
            return d;
        }

        public override string ToString()
        {
            return $"{_subject_id} - {_subject}\n{_name} - {_date_str} - {(_grade>=0 ? _grade : "Inconnue")} - {(_class_avg >= 0 ? _class_avg : "Inconnue")} - {_rank}\n{_appr}\n";
        }
    }
}
