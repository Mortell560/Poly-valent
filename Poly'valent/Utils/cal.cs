using Ical.Net;
using Ical.Net.CalendarComponents;
using System.Net;

namespace Poly_valent.Utils
{
    internal class cal
    {
        private static readonly List<ulong> r620 = new() {
            652, 832, 787, 780, 696, 736, 775, 817, 704, 712, 790, 842, 837, 819, 702, 660, 574, 741, 668
        };
        // Dans l'ordre: amphi, salles de pc, salles de td, salles de projets/platformes


        public static Dictionary<string, List<ulong>> Ids_bat = new()
        {
                    { "620", r620 }
                };

        public static Calendar GetEDT(ulong id, DateTime dateS, DateTime dateF)
        {
            string url = $"https://ade-planning.polytech.universite-paris-saclay.fr/jsp/custom/modules/plannings/anonymous_cal.jsp?resources={id}&projectId=5&calType=ical&lastDate={dateF.Year}-{dateF.Month}-{dateF.Day}&firstDate={dateS.Year}-{dateS.Month}-{dateS.Day}";
            Calendar c = Calendar.Load(GetAsync(url));
            return c;
        }

        public static string GetAsync(string uri)
        {
            using var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            client.BaseAddress = new Uri(uri);
            HttpResponseMessage response = client.GetAsync(client.BaseAddress).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

        public static string GetEDTString(ulong id, DateTime dateS, DateTime dateF)
        {
            string url = $"https://ade-planning.polytech.universite-paris-saclay.fr/jsp/custom/modules/plannings/anonymous_cal.jsp?resources={id}&projectId=5&calType=ical&lastDate={dateF.Year}-{dateF.Month}-{dateF.Day}&firstDate={dateS.Year}-{dateS.Month}-{dateS.Day}";
            return GetAsync(url);
        }

        public static CalendarEvent? nextClass(Calendar c, DateTime date)
        {
            List<CalendarEvent> events = new();
            foreach (CalendarEvent e in c.Events)
            {
                DateTime d = new(e.Start.Year, e.Start.Month, e.Start.Day, e.Start.Hour, e.Start.Minute, e.Start.Second, e.Start.Millisecond);
                if (d >= date)
                {
                    events.Add(e);
                }
            }
            if (events.Count > 0)
            {
                return events.MinBy(x => x.Start);
            }
            return null;
        }

        public static List<string> FindCurrentlyOccupiedRoom(List<string> bats, DateTime d)
        {
            List<string> rooms = new();
            foreach (string bat in bats)
            {
                foreach (ulong roomId in Ids_bat[bat])
                {
                    Calendar c = GetEDT(roomId, DateTime.Now, DateTime.Now);
                    foreach (CalendarEvent e in c.Events)
                    {
                        DateTime dStart = new(e.Start.Year, e.Start.Month, e.Start.Day, e.Start.Hour, e.Start.Minute, e.Start.Second, e.Start.Millisecond);
                        DateTime dEnd = new(e.End.Year, e.End.Month, e.End.Day, e.End.Hour, e.End.Minute, e.End.Second, e.End.Millisecond);
                        if ((dStart <= d) && (d <= dEnd))
                        {
                            rooms.Add(e.Location.ToString());
                        }
                    }
                }
            }
            rooms.Sort();
            return rooms;
        }

        public static Calendar ToCalendar(string ical)
        {
            Calendar c = Calendar.Load(ical);
            return c;
        }

    }
}
