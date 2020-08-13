using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using WC.API.SMTP;
using WC.Logger;

namespace WC.API
{
    public class CrawlerLogic : IDisposable
    {
        private readonly string _baseURL;
        private readonly string _resourcesPath;
        private readonly string _announcementsFileName;
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _fromAddress;
        private readonly List<string> _toAddresses;
        private readonly string _subject;
        private readonly string _announcesNbrMatch = "{AnnouncesNbr}";
        private readonly int _maxPrice;
        private readonly int _minSquareMeters;
        private readonly SMTPClient _smtpClient;
        public CrawlerLogic()
        {
            _baseURL = "https://www.idealista.pt";
            _resourcesPath = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            _announcementsFileName = "Announcements.json";
            _host = "smtp.gmail.com";
            _port = 587;
            _user = "alexmmreis.work@gmail.com";
            _password = "Work12345";
            _fromAddress = "alexmmreis.work@gmail.com";
            _toAddresses = new List<string>() { "ana.sofia.sardinha@gmail.com", "alexmmreis@gmail.com" };
            _smtpClient = new SMTPClient(_host, _port, _user, _password, _fromAddress, _toAddresses);

            _subject = $"Existem {_announcesNbrMatch} novos anuncios no idealista!";
            _maxPrice = 150000;
            _minSquareMeters = 50000;
        }

        private string BuildEmailBody(List<string> newAnnouncements, List<string> htmlAnnouncements)
        {
            string output = null;
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"FILTRO = [Preço <= {_maxPrice}€] & [Area(m2) >={_minSquareMeters}]");
                sb.AppendLine("");
                sb.AppendLine("Novos Anuncios:");
                newAnnouncements.ForEach(a => sb.AppendLine(a));
                sb.AppendLine("");
                sb.AppendLine("Todos os Anuncios:");
                htmlAnnouncements.ForEach(a => sb.AppendLine(a));

                return sb.ToString();
            }
            catch (Exception ex)
            {
                LogEngine.CrawlerLogger.Error($"BLL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = null;
            }
            finally
            {
                sw.Stop();
                LogEngine.CrawlerLogger.Info($"BLL.{methodName}(LEN(newAnnouncements)={newAnnouncements.Count}, LEN(htmlAnnouncements)={htmlAnnouncements.Count}) => OUT={output} in {sw.ElapsedMilliseconds}ms");
            }
        } 

        private bool SendEmail(List<string> oldAnnouncements, List<string> htmlAnnouncements)
        {
            bool output = false;
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try 
            {
                var newAnnouncements = htmlAnnouncements.Where(a => !oldAnnouncements.Contains(a)).ToList();
                string subject = _subject.Replace(_announcesNbrMatch, newAnnouncements.Count.ToString());
                string body = this.BuildEmailBody(newAnnouncements, htmlAnnouncements);

                return output = _smtpClient.SendEmail(subject, body);
            }
            catch (Exception ex)
            {
                LogEngine.CrawlerLogger.Error($"BLL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = false;
            }
            finally
            {
                sw.Stop();
                LogEngine.CrawlerLogger.Info($"BLL.{methodName}(LEN(oldAnnouncements)={oldAnnouncements.Count}, LEN(htmlAnnouncements)={htmlAnnouncements.Count}) => SUCCESS={output} in {sw.ElapsedMilliseconds}ms");
            }
        }

        private bool WriteAnnouncementsFile(string filePath, List<string> htmlAnnouncements)
        {
            bool output = false;
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string fileContent = JsonConvert.SerializeObject(htmlAnnouncements);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(htmlAnnouncements));

                return output = true;
            }
            catch (Exception ex)
            {
                LogEngine.CrawlerLogger.Error($"BLL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = false;
            }
            finally
            {
                sw.Stop();
                LogEngine.CrawlerLogger.Info($"BLL.{methodName}(filePath={filePath}, LEN(htmlAnnouncements)={htmlAnnouncements.Count}) => SUCCESS={output} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public List<string> RunCrawler()
        {
            List<string> output = new List<string>();
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try 
            { 
                var url = $"{_baseURL}/comprar-terrenos/portimao/com-preco-max_{_maxPrice},tamanho-min_{_minSquareMeters}/?ordem=atualizado-desc";
                Dictionary<string, string> headers = new Dictionary<string, string>()
                {
                    {"cache-control","keep-alive" },
                    {"upgrade-insecure-requests","1" },
                    {"user-agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36" },
                    {"accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9" },
                    {"Sec-Fetch-Site","same-origin" },
                    { "Sec-Fetch-Mode","navigate"},
                    { "Sec-Fetch-User", "?1"},
                    { "Sec-Fetch-Dest", "document"},
                    {"Referer", "https://www.idealista.pt/comprar-terrenos/portimao/mapa" },
                    { "Accept-Encoding", "gzip, deflate, br"},
                    { "Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8,pt;q=0.7,fr;q=0.6"}
                    //{ "Cookie",  "userUUID=bf9a5f09-474c-4a6b-950b-c46bf7d243f6; _pxhd=ab061788c8ced3ed310fdac492f3dd6e6a91c8de698d3fb7d9e746106ad5a80a:7b96b841-841b-11ea-a321-d3a9b39374f6; cto_lwid=5d576ff9-4968-4a0f-9ec5-2be160e6d414; xtvrn=$410875$; xtant410875=1; xtan410875=2-anonymous; cookieDirectiveClosed=true; _pxvid=7b96b841-841b-11ea-a321-d3a9b39374f6; atuserid=%7B%22name%22%3A%22atuserid%22%2C%22val%22%3A%22827a3e2c-3503-46e8-bc2b-dd42d981b981%22%2C%22options%22%3A%7B%22end%22%3A%222021-05-23T22%3A00%3A16.747Z%22%2C%22path%22%3A%22%2F%22%7D%7D; atidvisitor=%7B%22name%22%3A%22atidvisitor%22%2C%22val%22%3A%7B%22vrn%22%3A%22-582068-%22%7D%2C%22options%22%3A%7B%22path%22%3A%22%2F%22%2C%22session%22%3A15724800%2C%22end%22%3A15724800%7D%7D; _hjid=c749c6a8-c2bc-448c-91f3-a27b59604ce2; atreman=%7B%22name%22%3A%22atreman%22%2C%22val%22%3A%7B%22camp%22%3A%22AL-40-%5Bmashup%5D-%5Btrovit_trovit.pt%5D-%5Bintext%5D-%5B440145818%5D-%5Bcampaing%5D%7C%5Bhouse_sale_faro%5D-%5B30056920%5D%22%2C%22date%22%3A440974.02969638887%7D%2C%22options%22%3A%7B%22path%22%3A%22%2F%22%2C%22session%22%3A2592000%2C%22end%22%3A2592000%7D%7D; news_test_ab=a; _cb_ls=1; _cb=BLKWmACrJB5WBkD2ur; listingGalleryBoostEnabled=false; _chartbeat2=.1587748963205.1588098153894.10001.D5sEHCC6R62aBioJ8OkhGQpC7rzyH.1; TestIfCookieP=ok; pbw=%24b%3d16810%3b%24o%3d11100%3b%24sw%3d1280%3b%24sh%3d768; pid=8286224279225407234; pdomid=24; lcsrd=2020-04-29T15:17:04.2490083Z; csync=0:231329635019753528|80:Sme-j0c2uYlSZ-zZTDP13xph7ohSYLyNRmDAqHw7|86:816095575458620178|79:3e3f1b73-7a05-4554-8c06-29c3efd9f9ec|111:ID5-ZHMOovf3RWXLhMlibvRphlC2DHclpGtcZ_Avd7Lvkw|117:2f3d8ef4348bad54c51a6db20be82bde|126:eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJzaWQiOjEwMTc2MCwidXNyIjoicWdZZXNnWWJNVkpuZFVGTlNIaEhWMDl5ZVRGd2FHbE9WbVJUWVZCaE5EQk0ifQ.xebpFKdMBL61QJvki-Nue_uIQYJS7ZdbPmMmIh5-sQc4hx6nD3hQzVUBihcfomYnn8l8fp2cKuJdfBUeNeVIjA|22:1014686008897247995|25:2d2c5be9-4376-4100-b3bf-240b24b1bf62|31:fc050b7e-5138-4656-9b47-1fee983ba985|76:CAESENZC28Mp_PvyzI6tSadfXRo|94:W_lMjQAAAKjIZ3rg|100:b61ac1e6-43fc-0280-13d7-0e5cbf3385ce|49:6622909020081027098|127:AAH-w0630x4AACGz927zTA|32:4350526228074331097|116:OGMsn19Z5KzlPr36s6Dn|130:bbodhz5khlr6wa0l40t4|107:8f03e7c8-f663-4e05-8e88-f12dae8132ad-tuct2e85bfa|68:f1d44f72-41e9-45df-b266-999ef2e259d7|75:93c04bea-3920-4d66-8124-ad84b99ce68d|113:OPTOUT|101:8vftj0VtmEw8Re6nNPSuGdiMkL1c2B-jC4NhJymrJS0=|110:3e3f1b73-7a05-4554-8c06-29c3efd9f9ec|104:JOE4LI9I-W-5ERR|33:Xae6wLlQJTUAAE4LXpgAAAAC&241|92:RCm3lq3GvAmQ|66:04532204003a4bb4c66a3f74; Trk0=Value=376599&Creation=29%2f04%2f2020+17%3a29%3a09; askToSaveAlertPopUp=false; senda6433ede-fd05-4fe0-8482-080694853614="{'friendsEmail':null,'email':null,'message':null}"; contacteb1b6c4e-ec0e-4265-93a6-f1116402419d="{'email':null,'phone':null,'phonePrefix':null,'friendEmails':null,'name':null,'message':null,'message2Friends':null,'maxNumberContactsAllow':10,'defaultMessage':true}"; SESSION=eb1b6c4e-ec0e-4265-93a6-f1116402419d; cookieSearch-1="/comprar-terrenos/portimao/com-preco-max_150000,tamanho-min_100000/:1589370402636"; WID=590acda5fe1fa3a5|XrveI|XrveF; ABTasty=uid=5xj5crhtv6shjt2x&fst=1587506415842&pst=1589277165997&cst=1589370393025&ns=52&pvt=1841&pvis=1841&th=; utag_main=v_id:01719ec18a93007666728b1af7bc0307300a406b00978$_sn:29$_se:8$_ss:0$_st:1589372205224$dc_visit:29$ses_id:1589370393085%3Bexp-session$_pn:2%3Bexp-session$_prevVtSource:portalSites%3Bexp-1589373993095$_prevVtCampaignCode:%3Bexp-1589373993095$_prevVtDomainReferrer:idealista.pt%3Bexp-1589373993095$_prevVtSubdomaninReferrer:www.idealista.pt%3Bexp-1589373993095$_prevVtUrlReferrer:https%3A%2F%2Fwww.idealista.pt%2Fcomprar-terrenos%2Fportimao%2Fmapa%3Bexp-1589373993095$_prevVtprevPageName:11%3A%3Alisting%3A%3AresultList%3A%3Aothers%3Bexp-1589374005229$dc_event:2%3Bexp-session$dc_region:eu-west-1%3Bexp-session; ABTastySession=mrasn=&referrer=https://www.idealista.pt/comprar-terrenos/portimao/mapa&lp=https://www.idealista.pt/comprar-terrenos/portimao/com-preco-max_150000%2Ctamanho-min_100000/&sen=3; _px2=eyJ1IjoiNTBhNjhlMjAtOTUwMS0xMWVhLTllNzItY2Q5ZWEzMDk1ZjE3IiwidiI6IjdiOTZiODQxLTg0MWItMTFlYS1hMzIxLWQzYTliMzkzNzRmNiIsInQiOjE1ODkzNzA5MDA1MzQsImgiOiI1NDlhM2U2N2M4YWIzYTU4YzM4Y2I5MDJkZmQyNTlmOTc1NDM5ODdkMTcyMjRhNGViYWI3ZTJkOTU4ZDA5ZjVlIn0="}
                    };

                var response = ApiRequest.GetHTML(url, headers);
                if(response.Data == null)
                {
                    return output = new List<string>();
                }

                response.Data = Regex.Replace(response.Data, @"\s+", "");
                var matches = Regex.Matches(response.Data, "<divclass=\"item-info-container\">(?:.|\n)*?<\\/div><\\/article>");

                //1- Parse html to announcements
                List<string> htmlAnnouncements = new List<string>();
                foreach (var match in matches)
                {
                    var divAnnounce = match.ToString();
                    string pattern = "\"\\/.*\\/\"";
                    string announceDetailPath = Regex.Match(divAnnounce, pattern).Value;
                    if (string.IsNullOrEmpty(announceDetailPath))
                    {
                        continue;
                    }
                    announceDetailPath = announceDetailPath.Replace("\"", "");
                    htmlAnnouncements.Add($"{_baseURL}{announceDetailPath}");
                }

                //2- read old announcements File
                string filePath = Path.Combine(_resourcesPath, _announcementsFileName);

                List<string> oldAnnouncements = new List<string>();
                if (File.Exists(filePath)) { 
                    string fileContent = File.ReadAllText(filePath);
                    oldAnnouncements = JsonConvert.DeserializeObject<List<string>>(fileContent);
                }

                //3- update announcements if new and old announcements are different

                bool announcementsAreEqual = htmlAnnouncements.SequenceEqual(oldAnnouncements);
                if (!announcementsAreEqual)
                {
                    var newAnnouncements = htmlAnnouncements.Where(a => !oldAnnouncements.Contains(a)).ToList();
                    this.SendEmail(newAnnouncements, htmlAnnouncements);
                    this.WriteAnnouncementsFile(filePath, newAnnouncements);
                }

                return htmlAnnouncements;

            }
            catch (Exception ex)
            {
                LogEngine.CrawlerLogger.Error($"BLL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = null;
            }
            finally
            {
                sw.Stop();
                LogEngine.CrawlerLogger.Info($"BLL.{methodName}() => OUT={output?.Count ?? 0} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public void Start()
        {
            this.RunCrawler();
        }

        public void Dispose()
        {
        }
    }
}
