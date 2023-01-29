using Ical.Net;
using Ical.Net.CalendarComponents;

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
    }
}
