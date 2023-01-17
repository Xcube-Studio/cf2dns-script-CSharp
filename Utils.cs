using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Aliyun.Acs.Alidns.Model.V20150109;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth;
using Aliyun.Acs.Core.Profile;
using Newtonsoft.Json;

namespace cf2dns_script_DotnetCSharp
{
    internal class Utils
    {
        public class ip
        {
            [JsonProperty("ip")]
            public string ipnumber { get; set; }
            [JsonProperty("latency")]
            public int ping { get; set; }
            [JsonProperty("speed")]
            public int speedkbs { get; set; }
        }

        public class iplist
        {
            [JsonProperty("CM")]
            public List<ip> CMCCip { get; set; }
            [JsonProperty("CT")]
            public List<ip> CTCCip { get; set; }
            [JsonProperty("CU")]
            public List<ip> CUCCip { get; set; }
        }
        public class CloudFlare_Optimization_IP
        {
            [JsonProperty("info")]
            public iplist info { get; set; }
        }

        public class AliDNSRecordsResult
        {
            [JsonProperty("DomainRecords")]
            public DomainRecords info { get; set; }
        }

        public class DomainRecords
        {
            public List<DNSRecord> Record { get; set; }
        }
        public class DNSRecord
        {
            public string RR { get; set; }
            public string Line { get; set; }
            public string Type { get; set; }
            public string RecordId { get; set; }
        }
        public enum LogEnum
        {
            Info=1,
            Warning=2,
            Error=3
        }

        public enum LineEnum
        {
            telecom=1,
            unicom=2,
            mobile=3,
            oversea=4
        }
        public class IPKey
        {
            public string key { get; set; }
        }

        public static string POST(string host, string body)
        {
            string result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "POST";
            request.Headers.Add("Content-Type", "application/json");
            byte[] data = Encoding.UTF8.GetBytes(body);
            request.ContentLength = data.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }

            request.ContentLength = data.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;

        }

        public static void WriteLog(LogEnum e, string body)
        {
            Console.WriteLine("["+e.ToString()+"] "+body);
        }

        public static void AddDomainRecord(DefaultAcsClient client, string Type, string RR,
            string DomainName, string Value, LineEnum line)
        {
            WriteLog(LogEnum.Info,RR+"."+DomainName+" "+line.ToString()+" "+Value);
            var request = new AddDomainRecordRequest();
            request.Type=Type;
            request.RR=RR;
            request.DomainName=DomainName;
            request._Value=Value;
            request.Line = line.ToString();

            var response = client.GetAcsResponse(request);
            WriteLog(LogEnum.Info,System.Text.Encoding.Default.GetString(response.HttpResponse.Content));
        }

        public static string DescribeDomainRecords(DefaultAcsClient client, string DomainName)
        {
            var request=new DescribeDomainRecordsRequest();
            request.DomainName = DomainName;
            request.PageSize = 500;

            var response = client.GetAcsResponse(request);
            WriteLog(LogEnum.Info,System.Text.Encoding.Default.GetString(response.HttpResponse.Content));
            return Encoding.Default.GetString(response.HttpResponse.Content);
        }

        public static void DeleteDomainRecord(DefaultAcsClient client, string RecordId)
        {
            var request = new DeleteDomainRecordRequest();
            request.RecordId = RecordId;
            var response = client.GetAcsResponse(request);
            WriteLog(LogEnum.Info,Encoding.Default.GetString(response.HttpResponse.Content));
        }

    }
}
