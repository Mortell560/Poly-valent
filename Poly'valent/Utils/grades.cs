using HtmlAgilityPack;
using System.Globalization;
using System.Net;

namespace Poly_valent.Utils
{
    internal class Grades
    {
        const string dash = "&mdash;";
        public static async Task<List<Test>> GetGrades(string LOGIN, string PASSWORD, int s)
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
                HtmlDocument html = new();
                html.LoadHtml(await response.Content.ReadAsStringAsync());
                HtmlNodeCollection coursesHTML = html.DocumentNode.ChildNodes;
                List<Test> courses = new();
                string currYear = await GetCurrentSchoolYear(LOGIN, PASSWORD);

                if (html.DocumentNode.SelectSingleNode($"//table[@id='Tests1{currYear}']").SelectSingleNode("./tbody").ChildNodes.Count == 1)
                {
                    return courses;
                }
                if (s == 1 || s == 3)
                {
                    coursesHTML = html.DocumentNode.SelectSingleNode($"//table[@id='Tests1{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr");

                    if (s == 3)
                    {
                        if (!(html.DocumentNode.SelectSingleNode($"//table[@id='Tests2{currYear}']").SelectSingleNode("./tbody").ChildNodes.Count == 1))
                        {
                            foreach (HtmlNode? node in html.DocumentNode.SelectSingleNode($"//table[@id='Tests2{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr"))
                            {
                                coursesHTML.Add(node); // concat doesn't work for some reason
                            }
                        }
                    }
                }

                if (s == 2)
                {
                    if (html.DocumentNode.SelectSingleNode($"//table[@id='Tests2{currYear}']").SelectSingleNode("./tbody").ChildNodes.Count == 1){
                        return courses;
                    }
                    coursesHTML = html.DocumentNode.SelectSingleNode($"//table[@id='Tests2{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr");
                }

                foreach (HtmlNode? course_html in coursesHTML)
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

                    // C'est un peu Ohio mais si on met un breakpoint en fait tout est decale de 1 pour aucune raison valide

                    grade = grade.Replace(',', '.');
                    avg = avg.Replace(',', '.');
                    bool g, a;
                    g = float.TryParse(grade, NumberStyles.Float, CultureInfo.InvariantCulture, out float g_d);
                    a = float.TryParse(avg, NumberStyles.Float, CultureInfo.InvariantCulture, out float a_d);

                    Test c = new Test(name, subject, subject_id, g ? g_d : -1.0f, a ? a_d : -1.0f, rank, date_str, appr);
                    courses.Add(c);
                }
                return courses;
            }

        }

        public static async Task<Dictionary<string, List<Module>>> GetModules(string LOGIN, string PASSWORD, int s)
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
                HtmlNodeCollection modulesHTML = html.DocumentNode.ChildNodes;
                Dictionary<string, List<Module>> d = new();
                string currYear = await GetCurrentSchoolYear(LOGIN, PASSWORD);

                if (s == 2)
                {
                    modulesHTML = html.DocumentNode.SelectSingleNode($"//table[@id='Courses2{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr");
                }
                else
                {
                    modulesHTML = html.DocumentNode.SelectSingleNode($"//table[@id='Courses1{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr");
                    if (s == 3)
                    {
                        foreach (HtmlNode? node in html.DocumentNode.SelectSingleNode($"//table[@id='Courses2{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr"))
                        {
                            modulesHTML.Add(node);
                        }
                    }
                }

                foreach (HtmlNode? moduleN in modulesHTML)
                {

                    var modules = moduleN.SelectNodes("./td");
                    
                    //foreach (var item in modules)
                    //{
                    //    Console.WriteLine(item.InnerText.Trim());
                    //}
                    if (!d.ContainsKey(modules.ElementAt(0).InnerText.Trim().Replace(dash, "-")))
                    {
                        d.Add(modules.ElementAt(0).InnerText.Trim().Replace(dash, "-"), new List<Module>());
                    }
                    string? name, moduleID, block, rank, S;

                    moduleID = modules.ElementAt(1).InnerText.Trim();
                    name = modules.ElementAt(2).InnerText.Trim();
                    bool f_d = float.TryParse(modules.ElementAt(3).InnerText.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float coef);
                    block = modules.ElementAt(4).InnerText.Trim() == dash ? "-" : modules.ElementAt(4).InnerText.Trim();
                    bool g_b = float.TryParse(modules.ElementAt(5).SelectSingleNode("./span").InnerText.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float grade); // don't ask me why they put a span there
                    bool a_b = float.TryParse(modules.ElementAt(6).InnerText.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float avg);
                    rank = modules.ElementAt(7).InnerText.Trim();
                    S = modules.ElementAt(8).InnerText.Trim() == dash ? "-" : modules.ElementAt(8).InnerText.Trim();

                    string k = d.Keys.Where(x => x.Contains(moduleID.Length == 6 ? moduleID.Remove(4) : block.Remove(4))).First();
                    d[k].Add(new Module(name, moduleID, f_d ? coef : -1.0f, block, g_b ? grade : -1.0f, a_b ? avg : -1.0f, rank, S));
                

                }

                return d;

            }
        }

        public static async Task<List<UE>> GetUE(string LOGIN, string PASSWORD, int s)
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
                HtmlDocument html = new();
                html.LoadHtml(await response.Content.ReadAsStringAsync());
                HtmlNodeCollection UE_HTML = html.DocumentNode.ChildNodes;
                List<UE> UEs = new();
                string currYear = await GetCurrentSchoolYear(LOGIN, PASSWORD);

                if (s == 2)
                {
                    UE_HTML = html.DocumentNode.SelectSingleNode($"//table[@id='Modules2{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr");
                }
                else
                {
                    UE_HTML = html.DocumentNode.SelectSingleNode($"//table[@id='Modules1{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr");
                    if (s == 3)
                    {
                        foreach (HtmlNode node in html.DocumentNode.SelectSingleNode($"//table[@id='Modules2{currYear}']").SelectSingleNode("./tbody").SelectNodes("./tr"))
                        {
                            UE_HTML.Add(node);
                        }
                    }
                }

                foreach (HtmlNode? UEN in UE_HTML)
                {
                    HtmlNodeCollection? UE_c = UEN.SelectNodes("./td");
                    string? code, name, result, rank;
                    code = UE_c.ElementAt(0).InnerText.Trim();
                    name = UE_c.ElementAt(1).InnerText.Trim();
                    bool e_b = float.TryParse(UE_c.ElementAt(2).InnerText.Trim().Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out float ECTS);
                    bool g_b = float.TryParse(UE_c.ElementAt(4).InnerText.Trim().Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out float grade);
                    bool a_b = float.TryParse(UE_c.ElementAt(5).InnerText.Trim().Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out float avg);
                    rank = UE_c.ElementAt(6).InnerText.Trim();
                    result = UE_c.ElementAt(7).InnerText.Trim();
                    UEs.Add(new UE(code, name, e_b ? ECTS : -1.0f, g_b ? grade : -1.0f, a_b ? avg : -1.0f, rank, result));
                }

                return UEs;
            }
        }

        public static async Task<string> GetCurrentSchoolYear(string LOGIN, string PASSWORD)
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
                return html.DocumentNode.SelectSingleNode("//h2").Attributes["data-year"].Value;
            }
        }
    }
}
