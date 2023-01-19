using HtmlAgilityPack;
using System.Net;
namespace Poly_valent.Utils
{
    internal class Grades
    {
        private async Task<List<Course>> GetGrades(string LOGIN, string PASSWORD)
        {
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                string baseUrl = "https://oasis.polytech.universite-paris-saclay.fr/";
                string url1 = baseUrl + "prod/bo/core/Router/Ajax/ajax.php?targetProject=oasis_polytech_paris&route=BO\\Connection\\User::login";
                client.BaseAddress = new Uri(url1);
                await client.GetAsync(client.BaseAddress);

                Dictionary<string, string> values = new Dictionary<string, string>
                {
                    { "login", LOGIN },
                    { "password", PASSWORD },
                    { "codepage", "MYMARKS"}
                };

                var content = new FormUrlEncodedContent(values);
                
                var response = await client.PostAsync(client.BaseAddress, content);
                string url = baseUrl + "prod/bo/core/Router/Ajax/ajax.php?targetProject=oasis_polytech_paris&route=BO\\Layout\\MainContent::load&codepage=MYMARKS";

                response = await client.GetAsync(url);
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(await response.Content.ReadAsStringAsync());

                List<Course> courses = new List<Course>();
                var coursesHTML = html.DocumentNode.Ancestors("tr")
                    .Where(x => x.HasClass("courseLine"))
                    .ToList();

                foreach (var course_html in coursesHTML)
                {
                    string? subject, subject_id, name, date_str, grade;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    subject_id = course_html.Descendants("td")
                        .ElementAt(0)
                        .Descendants("div")
                        .ToString()
                        .Split("\n")
                        .ElementAt(1)
                        .Trim()
                        .SkipLast(2)
                        .ToString();

                    subject = course_html.Descendants("td")
                        .ElementAt(0)
                        .Descendants("div")
                        .ToString()
                        .Split("\n")
                        .ElementAt(3)
                        .Trim();


                    name = course_html.Descendants("td")
                        .ElementAt(0)
                        .Descendants("div")
                        .ToString()
                        .Split("\n")
                        .ElementAt(3)
                        .Trim();

                    grade = course_html.Descendants("td")
                        .ElementAt(3)
                        .ToString()
                        .Trim();

                    date_str = course_html.Descendants("td")
                        .ElementAt(2)
                        .ToString()
                        .Trim();

                    Course c = new Course(name, subject, subject_id, float.Parse(grade.Replace(',', '.')), date_str);
                    courses.Add(c);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }

                return courses;
            }

        }
    }
}
