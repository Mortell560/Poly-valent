using Ical.Net;

namespace PolyDatabase
{
    public class Newsletters
    {
        private readonly PolyContext _context;

        public Newsletters(PolyContext context)
        {
            _context = context;
        }

        public async Task AddNewsletterAsync(ulong userId, ulong id, string? calendar)
        {
            await _context.AddAsync(new Newsletter
            {
                UserId = userId,
                Id = id,
                Calendar = calendar
            });
            await _context.SaveChangesAsync();
        }

        public async Task RemoveNewsletterAsync(ulong userId)
        {
            Newsletter? n = await _context.Newsletters.FindAsync(userId);
            if (n != null)
            {
                _context.Remove(n);
            }
            await _context.SaveChangesAsync();
        }

        public async Task ChangeIdAsync(ulong userId, ulong id)
        {
            Newsletter? n = await _context.Newsletters.FindAsync(userId);
            if (n != null)
            {
                n.Id = id;
            }
            await _context.SaveChangesAsync();
        }

        public async Task ChangeCalendar(ulong userId, string? calendar)
        {
            Newsletter? n = await _context.Newsletters.FindAsync(userId);
            if (n != null)
            {
                n.Calendar = calendar;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Calendar> GetCalendar(ulong userId)
        {
            Newsletter? n = await _context.Newsletters.FindAsync(userId);
            if (n != null)
            {
                Calendar x = Calendar.Load(n.Calendar);
                return x;

            }
            Calendar c = new();
            return c;
        }

        public async Task<bool> ExistsInDB(ulong userId)
        {
            Newsletter? n = await _context.Newsletters.FindAsync(userId);
            return n != null;
        }

        public async Task<List<Tuple<ulong, ulong>>> GetAllIdsAsync()
        {
            List<Tuple<ulong, ulong>> l = new();
            foreach (Newsletter n in _context.Newsletters)
            {
                l.Add(new(n.UserId, n.Id));
            }
            return await Task.FromResult(l);
        }

        public async Task<List<Newsletter>> GetAllNewslettersAsync()
        {
            return await Task.FromResult(_context.Newsletters.ToList());
        }

        public async Task<ulong> GetIdAsync(ulong userId)
        {
            Newsletter? n = await _context.Newsletters.FindAsync(userId);
            if (n != null)
            {
                return n.Id;
            }
            return 0;
        }
    }
}
