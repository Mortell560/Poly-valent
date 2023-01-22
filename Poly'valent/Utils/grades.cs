using HtmlAgilityPack;
using System.Globalization;
using System.Net;

namespace Poly_valent.Utils
{
    internal class Grades
    {
        public static async Task<List<Course>> GetGrades(string LOGIN, string PASSWORD, int s)
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
                HtmlNodeCollection coursesHTML = html.DocumentNode.ChildNodes;
                List<Course> courses = new List<Course>();

                if (html.DocumentNode.SelectSingleNode("//table[@id='Tests12022']").SelectSingleNode("./tbody").ChildNodes.Count == 1)
                {
                    return courses;
                }
                if (s == 1 || s == 3)
                {
                    coursesHTML = html.DocumentNode.SelectSingleNode("//table[@id='Tests12022']").SelectSingleNode("./tbody").SelectNodes("./tr");

                    if (s == 3)
                    {
                        if (!(html.DocumentNode.SelectSingleNode("//table[@id='Tests22022']").SelectSingleNode("./tbody").ChildNodes.Count == 1))
                        {
                            coursesHTML.Concat(html.DocumentNode.SelectSingleNode("//table[@id='Tests22022']").SelectSingleNode("./tbody").SelectNodes("./tr"));
                        }
                    }
                }

                if (s == 2)
                {
                    if (html.DocumentNode.SelectSingleNode("//table[@id='Tests22022']").SelectSingleNode("./tbody").ChildNodes.Count == 1){
                        return courses;
                    }
                    coursesHTML = html.DocumentNode.SelectSingleNode("//table[@id='Tests22022']").SelectSingleNode("./tbody").SelectNodes("./tr");
                }

                foreach (var course_html in coursesHTML)
                {
                    string? subject, subject_id, name, date_str, grade, appr, avg, rank;
                    subject_id = course_html.SelectSingleNode("./td/div").Attributes["data-code"].Value;

                    subject = course_html.SelectSingleNode("./td/div/b").InnerText.Trim();

                    name = course_html.SelectSingleNode("./td[2]").InnerText.Trim();

                    date_str = course_html.SelectSingleNode("./td[3]").InnerText.Trim();

                    grade = course_html.SelectSingleNode("./td[4]").InnerText.Trim();

                    avg = course_html.SelectSingleNode("./td[5]").InnerText.Trim();
                    
                    rank = course_html.SelectSingleNode("./td[6]").InnerText.Trim();

                    appr = course_html.SelectSingleNode("./td[7]").InnerText.Trim();

                    // C'est un peu Ohio mais si on met un breakpoint mais en fait tout est decale de 1 pour aucune raison valide

                    grade = grade.Replace(',', '.');
                    avg = avg.Replace(',', '.');
                    bool g, a;
                    g = float.TryParse(grade, NumberStyles.Float, CultureInfo.InvariantCulture, out float g_d);
                    a = float.TryParse(avg, NumberStyles.Float, CultureInfo.InvariantCulture, out float a_d);

                    Course c = new Course(name, subject, subject_id, g ? g_d : -1.0f, a ? a_d : -1.0f, rank, date_str, appr);
                    courses.Add(c);
                }
                return courses;
            }

        }
    }
}
