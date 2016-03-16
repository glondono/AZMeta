using CommandLine;
using CommandLine.Text;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace AZMeta
{
    class Options
    {
        [Option("ConnectionString", Required = true, HelpText = "Azure storage connection string")]
        public string ConnectionString { get; set; }

        [Option("BlobContainer", Required = false, HelpText = "Azure storage container for BLOB service. For example --BlobContainer=img")]
        public string BlobContainer { get; set; }

        [Option("Service", Required = true, HelpText = "Azure Service (BLOB|TABLE|QUEUE). For Example --Service=BLOB")]
        public string Service { get; set; }

        [Option("SetCorsRule", Required = false, HelpText = "Cors rule to apply to the service. For example --SetCorsRule=allowed-methods=get|put|post|options|head;allowed-header=*;allowed-origins=*;exposed-header=*;max-age=1800")]
        public string SetCorsRule { get; set; }

        [Option("GetServiceProperties", Required = false, HelpText = "Get Properties of the service used")]
        public bool GetServiceProperties { get; set; }

        [Option("GetListContainer", Required = false, HelpText = "Get list of container for BLOB Service")]
        public bool GetListContainer { get; set; }

        [Option("RemoveCorsRules", Required = false, HelpText = "Remove all items in the CORS list")]
        public bool RemoveCorsRules { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        static void Containers(IEnumerable<CloudBlobContainer> list)
        {
            if (list != null)
            {
                var array = list.ToArray();
                if (array.Length > 0)
                {
                    foreach (var item in array)
                    {
                        Console.WriteLine("{0} {1} ({2})", item.Name, item.Uri);
                    }
                }
                return;
            }
            Console.WriteLine("Service has no containers");
        }
        static void Cors(CorsProperties cors)
        {
            if (cors != null && cors.CorsRules != null & cors.CorsRules.Count > 0)
            {
                var i = 1;
                foreach (var item in cors.CorsRules)
                {
                    Console.WriteLine("Cors Rule {0}", i);
                    Console.WriteLine("Allowed Methods {0}", item.AllowedMethods);
                    Console.WriteLine("Allowed Headers {0}", string.Join(",", item.AllowedHeaders));
                    Console.WriteLine("Allowed Origins {0}", string.Join(",", item.AllowedOrigins));
                    Console.WriteLine("Exposed Headers {0}", string.Join(",", item.ExposedHeaders));
                    Console.WriteLine("Max age {0} seconds", item.MaxAgeInSeconds);
                    Console.WriteLine();
                    i += 1;
                }
            }
            else
            {
                Console.WriteLine("Cors no set");
            }
        }
        static CorsRule CreateCors(string newcors)
        {
            Dictionary<string, string> newcorsdictionary = new Dictionary<string, string>();
            string[] newcorstoken = newcors.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string token in newcorstoken)
            {
                string[] tokenparts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                newcorsdictionary.Add(tokenparts[0], tokenparts[1]);
            }
            CorsRule rule = new CorsRule();
            rule.AllowedHeaders = newcorsdictionary["allowed-header"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            rule.AllowedOrigins = newcorsdictionary["allowed-origins"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            rule.ExposedHeaders = newcorsdictionary["exposed-header"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            rule.MaxAgeInSeconds = int.Parse(newcorsdictionary["max-age"]);
            if(newcorsdictionary["allowed-methods"].IndexOf("GET", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods = CorsHttpMethods.Get;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("POST", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Post;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("PUT", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Put;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("TRACE", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Trace;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("CONNECT", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Connect;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("DELETE", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Delete;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("HEAD", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Head;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("MERGE", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Merge;
            }
            if (newcorsdictionary["allowed-methods"].IndexOf("OPTIONS",  StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Options;
            }
            return rule;
        }
        static void ServiceProperties(ServiceProperties prop)
        {
            Console.WriteLine("Service Version {0}", prop.DefaultServiceVersion);
            if (prop.HourMetrics != null)
            {
                Console.WriteLine("Service Hour Metric Level {0}", prop.HourMetrics.MetricsLevel != null ? prop.HourMetrics.MetricsLevel.ToString() : "");
                Console.WriteLine("Service Hour Metric Retention days {0}", prop.HourMetrics.RetentionDays.HasValue ? prop.HourMetrics.RetentionDays.Value.ToString() : "");
                Console.WriteLine("Service Hour Metric Version {0}", prop.HourMetrics.Version);
            }
            else
            {
                Console.WriteLine("Service has no hour metrics");
            }
            if (prop.Logging != null)
            {
                Console.WriteLine("Service Logging Retention days {0}", prop.Logging.RetentionDays.HasValue ? prop.Logging.RetentionDays.Value.ToString() : "");
                Console.WriteLine("Service Logging Operations ", prop.Logging.LoggingOperations);
            }
            else
            {
                Console.WriteLine("Service has no logging");
            }
        }
        static void ContainerProperties(BlobContainerProperties prop)
        {
            Console.WriteLine("Container Last Modified {0}", prop.LastModified);
            Console.WriteLine("Container Lease Duration {0}", prop.LeaseDuration);
            Console.WriteLine("Container Lease State {0}", prop.LeaseState);
            Console.WriteLine("Container Lease Status {0}", prop.LeaseStatus);
            Console.WriteLine("Container ETag {0}", prop.ETag);
        }
        static void Metadata(IDictionary<string, string> meta)
        {
            if (meta != null && meta.Count > 0)
            {
                Console.WriteLine("Metadata");
                foreach (var item in meta)
                {
                    Console.WriteLine("\tKey: {0}", item.Key);
                    Console.WriteLine("\tValue: {0}", item.Value);
                }
            }
            else
            {
                Console.WriteLine("Metada no set");
            }
        }
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                var account = CloudStorageAccount.Parse(options.ConnectionString);
                if (options.Service.Equals("BLOB", StringComparison.InvariantCultureIgnoreCase))
                {
                    var blob = account.CreateCloudBlobClient();
                    Console.WriteLine("SERVICE BLOB {0}", blob.BaseUri.ToString());
                    if (options.GetServiceProperties)
                    {
                        ServiceProperties(blob.GetServiceProperties());
                        Cors(blob.GetServiceProperties().Cors);
                    }
                    if (options.SetCorsRule != null)
                    {
                        Console.WriteLine("setting new cors rule...");
                        var rule = CreateCors(options.SetCorsRule);
                        var properties = blob.GetServiceProperties();
                        properties.Cors.CorsRules.Add(rule);
                        blob.SetServiceProperties(properties);
                        Console.WriteLine("getting all cors rules...");
                        properties = blob.GetServiceProperties();
                        Cors(properties.Cors);
                    }
                    if(options.RemoveCorsRules)
                    {
                        Console.WriteLine("removing cors rules...");
                        var properties = blob.GetServiceProperties();
                        properties.Cors.CorsRules.Clear();
                        blob.SetServiceProperties(properties);
                        Console.WriteLine("getting all cors rules...");
                        properties = blob.GetServiceProperties();
                        Cors(properties.Cors);
                    }
                    if (options.BlobContainer != null)
                    {
                        Console.WriteLine();
                        var container = blob.GetContainerReference(options.BlobContainer);
                        container.FetchAttributes();
                        Console.WriteLine("CONTAINER {0}", container.StorageUri.PrimaryUri.ToString());
                        ContainerProperties(container.Properties);
                        Metadata(container.Metadata);
                    }
                    if (options.GetListContainer)
                    {
                        Containers(blob.ListContainers());
                    }
                }
            }
        }
    }
}
