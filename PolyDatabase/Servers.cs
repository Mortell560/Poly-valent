namespace PolyDatabase
{
    public class Servers
    {
        private readonly PolyContext _context;

        public Servers(PolyContext context)
        {
            _context = context;
        }

        public async Task AddServerAsync(ulong id, ulong newsChannel)
        {
            await _context.AddAsync(new Server
            {
                Id = id,
                NewsChannel = newsChannel
            });
            await _context.SaveChangesAsync();
        }
        
        public async Task RemoveServerAsync(ulong id)
        {
            Server? s = await _context.Servers.FindAsync(id);
            if (s != null)
            {
                _context.Remove(s);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<ulong> GetNewsChannel(ulong id)
        {
            Server? s = await _context.Servers.FindAsync(id);
            if (s != null)
            {
                return s.NewsChannel;
            }
            return 0;
        }

        public async Task ChangeNewsChannel(ulong id, ulong newsId)
        {
            Server? s = await _context.Servers.FindAsync(id);
            if (s == null)
            {
                await AddServerAsync(id, newsId);
                await _context.SaveChangesAsync();
                return;
            }
            s.NewsChannel = newsId;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsInDB(ulong id)
        {
            Server? s = await _context.Servers.FindAsync(id);
            return s != null;
        }
    }
}
