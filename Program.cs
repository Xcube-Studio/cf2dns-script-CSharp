using System;
using System.Runtime.InteropServices;
using Aliyun.Acs.Core.Auth;
using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Newtonsoft.Json;

namespace cf2dns_script_DotnetCSharp
{
    internal class Program
    {
        static void Main(string[] args)  //args[0]-->优选ipkey args[1]-->AliAPIKeyid arg[2]-->AliAPISecret
        {
            Console.WriteLine("Hello, World! Cf2dns_script_DotnetCSharp with AliCloudDNS");
            
            var body =new Utils.IPKey();
            body.key = args[0];
            var result=Utils.POST("https://api.hostmonit.com/get_optimization_ip", JsonConvert.SerializeObject(body));
            var CloudFlareOptimizationIp = JsonConvert.DeserializeObject<Utils.CloudFlare_Optimization_IP>(result);
            Utils.WriteLog(Utils.LogEnum.Info,"GET result \n"+result);

            #region Add Public CloudFlareOptimizationIP 
            body.key = "o1zrmHAF";
            var public_result=Utils.POST("https://api.hostmonit.com/get_optimization_ip", JsonConvert.SerializeObject(body));
            var public_CloudFlareOptimizationIp =
                JsonConvert.DeserializeObject<Utils.CloudFlare_Optimization_IP>(public_result);
            CloudFlareOptimizationIp.info.CMCCip.AddRange(public_CloudFlareOptimizationIp.info.CMCCip.Take(2));
            CloudFlareOptimizationIp.info.CTCCip.AddRange(public_CloudFlareOptimizationIp.info.CTCCip.Take(2));
            CloudFlareOptimizationIp.info.CUCCip.AddRange(public_CloudFlareOptimizationIp.info.CUCCip.Take(2));
            #endregion
            #region Sort CloudFlareOptimizationIP
            CloudFlareOptimizationIp.info.CMCCip.Sort((x,y) => x.ping.CompareTo(y.ping) );
            CloudFlareOptimizationIp.info.CMCCip.Sort((x,y)=> -x.speedkbs.CompareTo(y.speedkbs));
            CloudFlareOptimizationIp.info.CTCCip.Sort((x, y) => x.ping.CompareTo(y.ping));
            CloudFlareOptimizationIp.info.CTCCip.Sort((x, y) => -x.speedkbs.CompareTo(y.speedkbs));
            CloudFlareOptimizationIp.info.CUCCip.Sort((x, y) => x.ping.CompareTo(y.ping));
            CloudFlareOptimizationIp.info.CUCCip.Sort((x, y) => -x.speedkbs.CompareTo(y.speedkbs));
            #endregion
            var provider = new AccessKeyCredentialProvider(args[1], args[2]);
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou");
            DefaultAcsClient client = new DefaultAcsClient(profile, provider);

            var AliDNSResultModel =
                JsonConvert.DeserializeObject<Utils.AliDNSRecordsResult>(
                    Utils.DescribeDomainRecords(client, "natsurainko.work"));
            List<Utils.DNSRecord> now = AliDNSResultModel.info.Record
                .Where(x=>x.Type=="A"&&(x.RR=="fluentlauncher"||x.RR=="resource"||x.RR=="www")).ToList<Utils.DNSRecord>();
            foreach (var i in now)
            {
                Utils.DeleteDomainRecord(client,i.RecordId);
            }

            #region AddDomainRecord
            foreach (var i in CloudFlareOptimizationIp.info.CMCCip.Take(4).ToList())
            {
                Utils.AddDomainRecord(client,"A","resource","natsurainko.work",i.ipnumber,Utils.LineEnum.mobile);
                Utils.AddDomainRecord(client,"A","fluentlauncher","natsurainko.work",i.ipnumber,Utils.LineEnum.mobile);
                Utils.AddDomainRecord(client,"A","www","natsurainko.work",i.ipnumber,Utils.LineEnum.mobile);
            }
            foreach (var i in CloudFlareOptimizationIp.info.CTCCip.Take(4).ToList())
            {
                Utils.AddDomainRecord(client, "A", "resource", "natsurainko.work", i.ipnumber, Utils.LineEnum.telecom);
                Utils.AddDomainRecord(client, "A", "fluentlauncher", "natsurainko.work", i.ipnumber, Utils.LineEnum.telecom);
                Utils.AddDomainRecord(client, "A", "www", "natsurainko.work", i.ipnumber, Utils.LineEnum.telecom);
            }
            foreach (var i in CloudFlareOptimizationIp.info.CUCCip.Take(4).ToList())
            {
                Utils.AddDomainRecord(client, "A", "resource", "natsurainko.work", i.ipnumber, Utils.LineEnum.unicom);
                Utils.AddDomainRecord(client, "A", "fluentlauncher", "natsurainko.work", i.ipnumber, Utils.LineEnum.unicom);
                Utils.AddDomainRecord(client, "A", "www", "natsurainko.work", i.ipnumber, Utils.LineEnum.unicom);
            }
            #endregion

            Utils.WriteLog(Utils.LogEnum.Info, "Successful!");
        }
    }
}