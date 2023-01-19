using System.ComponentModel;

namespace Poly_valent.Utils
{
    internal class Course
    {
        private List<string> months = new() { "janvier", "février", "mars", "avril", "mai", "juin", "juillet", "août", "septembre", "octobre", "novembre", "décembre" };

        private string? _name;
        private string? _subject;
        private string? _subject_id;

        private float? _grade;
        private string? _date_str;
        private DateTime? _date;

        public Course(string? name, string? subject, string? subject_id, float? grade, string date_str)
        {
            _name = name;
            _subject = subject;
            _subject_id = subject_id;
            _grade = grade;
            _date_str = date_str;
            _date = _ConvertDate(date_str);
        }

        private DateTime _ConvertDate(string date_str)
        {
            string[] a = date_str.Split(" ");
            DateTime d = new DateTime(int.Parse(a[2]), months.IndexOf(a[1])+1, int.Parse(a[0]));
            return d;
        }
    }
}
