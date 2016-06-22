using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

class Instagram
{
    private static Random Random = new Random();
    private CookieContainer Cookies = new CookieContainer();
    private async Task InitializeSession()
    {
        using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = Cookies, UseCookies = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
        using (HttpClient client = new HttpClient(handler))
        using (HttpRequestMessage request = new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new Uri("https://i.instagram.com/") })
        {
            request.Headers.UserAgent.ParseAdd("Instagram 7.1.1 Android (21/5.0.2; 480dpi; 1080x1776; LGE/Google; Nexus 5; hammerhead; hammerhead; en_US)");
            request.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.Headers.Add("accept-encoding", "gzip, deflate, sdch");
            request.Headers.Add("accept-language", "en-US,en;q=0.8");
            request.Headers.Add("upgrade-insecure-requests", "1");

            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
        }
    }
    public async Task<string> CreateAccount(string Username, string First_Name, string Password, string Email)
    {
        try
        {
            await InitializeSession();

            var RegistrationContent = new
            {
                username = Username,
                first_name = First_Name,
                password = Password,
                guid = RandomString(8) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(4) + "-" + RandomString(12),
                email = Email,
                device_id = "android-" + HMAC(Random.Next(1000, 9999).ToString()).ToString().Substring(0, Math.Min(64, 16))
            };

            string Json = JsonConvert.SerializeObject(RegistrationContent, Formatting.Indented, new IsoDateTimeConverter());

            Dictionary<string, string> RegistrationRequest = new Dictionary<string, string>()
            {
                { "signed_body",  HMAC(Json) + "." + Json },
                { "ig_sig_key_version", "4" }
            };

            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = Cookies, UseCookies = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            using (HttpClient client = new HttpClient(handler))
            using (HttpRequestMessage request = new HttpRequestMessage() { Method = HttpMethod.Post, RequestUri = new Uri("https://i.instagram.com/api/v1/accounts/create/"), Content = new FormUrlEncodedContent(RegistrationRequest) })
            {
                request.Headers.Host = "i.instagram.com";
                request.Headers.UserAgent.ParseAdd("Instagram 7.1.1 Android (21/5.0.2; 480dpi; 1080x1776; LGE/Google; Nexus 5; hammerhead; hammerhead; en_US)");
                request.Headers.Add("Accept-Language", "en-US");
                request.Headers.Add("Accept-Encoding", "gzip");
                request.Headers.Add("Cookie2", "$Version=1");
                request.Headers.Add("X-IG-Connection-Type", "WIFI");
                request.Headers.Add("X-IG-Capabilities", "BQ==");

                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();
                
                JObject JS = JObject.Parse(responseString);

                if ((string)JS["status"] != "fail")
                {
                    if ((bool)JS["account_created"] == true)
                    {
                        return "Account Created Successfully!";
                    }
                    else
                    {
                        if ((string)JS["errors"][" username"] != null)
                            return "Error: " + (string)JS["errors"]["username"];

                        if ((string)JS["errors"]["password"] != null)
                            return "Error: " + (string)JS["errors"]["password"];

                        if ((string)JS["errors"]["email"] != null)
                            return "Error: " + (string)JS["errors"]["email"];

                        return "Error: Unknown";
                    }
                }
                else
                {
                    if ((bool)JS["spam"] == true)
                        return "Error: Instgram thinks request is spam, change IP?";

                    return "Error: " + (string)JS["feedback_message"];
                }
            }
        }
        catch (Exception ex) { return "Error: " + ex.Message; }
    }
    private static string HMAC(string String)
    {
        var keyByte = Encoding.UTF8.GetBytes("3f0a7d75e094c7385e3dbaa026877f2e067cbd1a4dbcf3867748f6b26f257117");
        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(String));
            return ByteToString(hmacsha256.Hash).ToLower();
        }
    }
    private static string ByteToString(byte[] buff)
    {
        string sbinary = "";
        for (int i = 0; i < buff.Length; i++)
            sbinary += buff[i].ToString("X2");
        return sbinary;
    }
    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray()).ToLower();
    }
}