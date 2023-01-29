namespace PolyDatabase
{
    public class Grades
    {
        private readonly PolyContext _context;
        private readonly List<string> months = new() { "janvier", "février", "mars", "avril", "mai", "juin", "juillet", "août", "septembre", "octobre", "novembre", "décembre" };

        public Grades(PolyContext context)
        {
            _context = context;
        }

        public async Task AddGrade(string name, string subject, string subject_id, float grade, float class_avg, string rank, string date_str, string appr)
        {
            await _context.AddAsync(new Grade
            {
                Name = name,
                Subject = subject,
                Subject_id = subject_id,
                grade = grade,
                Class_avg = class_avg,
                Rank = rank,
                Date_str = date_str,
                Date = _ConvertDate(date_str),
                Appr = appr
            });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveGrade(string subject_id, string name)
        {
            Grade? g = await _context.Grades.FindAsync(name, subject_id);
            if (g != null)
            {
                _context.Remove(g);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Grade?> GetGrade(string subject_id, string name)
        {
            return await _context.Grades.FindAsync(subject_id, name);
        }

        public async Task<List<Grade>> GetGrades(string look_for)
        {
            List<Grade> g = new();
            if (_context.Grades.Any())
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                g.AddRange(_context.Grades.Where(x => x.Name.Contains(look_for) || x.Subject.Contains(look_for)));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            return await Task.FromResult(g);
        }

        public async Task<List<Grade>> GetAllGrades()
        {
            List<Grade> g = new();
            if (_context.Grades.Any())
            {
                g.AddRange(_context.Grades.ToList());
            }
            return await Task.FromResult(g);
        }

        public async Task ModifyGrade(string name, string subject_id, float grade, float class_avg, string rank, string appr)
        {
            Grade? g = await _context.Grades.FindAsync(name, subject_id);
            if (g == null)
            {
                return;
            }
            g.Appr = appr;
            g.Rank = rank;
            g.grade = grade;
            g.Class_avg = class_avg;
            await _context.SaveChangesAsync();
        }

        private DateTime _ConvertDate(string date_str)
        {
            string[] a = date_str.Split(" ");
            DateTime d = new(int.Parse(a[2]), months.IndexOf(a[1]) + 1, int.Parse(a[0]));
            return d;
        }
    }
}
