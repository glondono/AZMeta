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
using System.IO;

namespace AZMeta
{
    class Options
    {
        [Option("ConnectionString", Required = true, HelpText = "Azure storage connection string")]
        public string ConnectionString { get; set; }

        [Option("Service", Required = true, HelpText = "Azure Service (BLOB|QUEUE). For Example --Service=BLOB")]
        public string Service { get; set; }

        [Option("SetCorsRuleMethods", Required = false, HelpText = "The methods (DELETE, GET, HEAD, MERGE, POST, OPTIONS and PUT) that the origin domain may use for a CORS request. Use , to separate multiple entries. For example : PUT , GET , OPTIONS.")]
        public string SetCorsRuleMethods { get; set; }

        [Option("SetCorsRuleOrigins", Required = false, HelpText = "The origin domains that are permitted to make a request against the storage service via CORS (use * for all or use , for multiple entries)", DefaultValue = "*")]
        public string SetCorsRuleOrigins { get; set; }

        [Option("SetCorsRuleHeadersAllowed", Required = false, HelpText = "The request headers that the origin domain may specify on the CORS request (use * for all headers)", DefaultValue = "*")]
        public string SetCorsRuleHeadersAllowed { get; set; }

        [Option("SetCorsRuleHeadersExposed", Required = false, HelpText = "The response headers that may be sent in the response to the CORS request and exposed by the browser to the request issuer (use * for all headers)", DefaultValue = "*")]
        public string SetCorsRuleHeadersExposed { get; set; }

        [Option("SetCorsRuleMaxAge", Required = false, HelpText = "The maximum amount time that a browser should cache the preflight OPTIONS request (in seconds)", DefaultValue=1800)]
        public int SetCorsRuleMaxAge { get; set; }

        [Option("GetCorsRules", Required = false, HelpText = "Get the list of cors rules")]
        public bool GetCorsRules { get; set; }

        [Option("GetListContainer", Required = false, HelpText = "Set to True to get list of containers for BLOB Service")]
        public bool GetListContainer { get; set; }

        [Option("MoveFromContainer", Required = false, HelpText = "In BLOB move operation, the source container. Use with CsvPath to only move the files names in the csv.")]
        public string MoveFromContainer { get; set; }

        [Option("MoveToContainer", Required = false, HelpText = "In BLOB move operation, the destination container.  Use with CsvPath to only move the files names in the csv.")]
        public string MoveToContainer { get; set; }

        [Option("RemoveCorsRules", Required = false, HelpText = "Remove all items in the CORS list")]
        public bool RemoveCorsRules { get; set; }

        [Option("CsvToQueue", Required = false, HelpText = "Azure queue name for QUEUE service to wich you will like to create messages from csvPath property. For example --CsvToQueue=new-orders")]
        public string CsvToQueue { get; set; }

        [Option("CsvPath", Required = false, HelpText = "File path to Csv.")]
        public string CsvPath { get; set; }

        [Option("CsvJson", Required = false, DefaultValue = false, HelpText = "If set, every row but header will be converted to a json queue message. If not used, each row will represent a queue message as a string")]
        public bool CsvJson { get; set; }

        [Option("GetAdHocSASContainer", Required = false, HelpText = "Set a container for blob lookup when GetAdHocSASBlob is specified or create ad hoc sas for container if GetAdHocSASBlob is not specified")]
        public string GetAdHocSASContainer { get; set; }

        [Option("GetAdHocSASBlob", Required = false, HelpText = "Create an ad hoc SAS url for the specific blob")]
        public string GetAdHocSASBlob { get; set; }

        [Option("GetAdHocSASPermissions", Required = false, HelpText = "Ad Hoc SAS permisisons (Add, Create, Delete, List, None, Read, Write). Use , to separate multiple entries")]
        public string GetAdHocSASPermisisons { get; set; }

        [Option("GetAdHocSASStart", Required = false, HelpText = "Ad Hoc SAS Start in minutes from current time. Leave empty to stat inmediatly")]
        public int GetAdHocSASStart { get; set; }

        [Option("GetAdHocSASExpire", Required = false, DefaultValue = 30, HelpText = "Ad Hoc SAS Expire in minutes from current time. By default is 30 min")]
        public int GetAdHocSASExpire { get; set; }

        [Option("GetServiceSASPoliciesContainer", Required = false, HelpText = "Get all the SAS policies for the specified container")]
        public string GetSASPolicies { get; set; }

        [Option("SetServiceSASPolicyContainer", Required = false, HelpText = "Container name to set a new Service SAS policy. Use with SetServiceSASPolicy properties. For example --SetServiceSASPolicyContainer=my-container --SetServiceSASPolicyExpire=30 --SetServiceSASPolicyPermissions=read|write")]
        public string SetServiceSASPolicyContainer { get; set; }

        [Option("SetServiceSASPolicyName", Required = false, HelpText = "New Service  SAS policy name")]
        public string SetServiceSASPolicyName { get; set; }

        [Option("SetServiceSASPolicyExpire", Required = false, HelpText = "New Service SAS policy expire in minutes from current date")]
        public int SetServiceSASPolicyExpire { get; set; }

        [Option("SetServiceSASPolicyStart", Required = false, HelpText = "New Service SAS policy start in minutes from current date. If not use, the policy will be effective rigth away")]
        public int SetServiceSASPolicyStart { get; set; }

        [Option("SetServiceSASPolicyPermissions", Required = false, HelpText = "New Service SAS policy permissions based on SharedAccessBlobPermissions enum. (none,read,write,delete,list,add,create). For example, SetServiceSASPolicyPermissions=read|write ")]
        public string SetServiceSASPolicyPermissions { get; set; }

        [Option("RemoveServiceSASPolicyContainer", Required = false, HelpText = "Container name to remove all service SAS policies")]
        public string RemoveServiceSASPolicyContainer { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    var account = CloudStorageAccount.Parse(options.ConnectionString);
                    if (options.Service.Equals("BLOB", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var blob = new BlobService(account.CreateCloudBlobClient());
                        Console.WriteLine("SERVICE BLOB {0}", blob.Url);

                        if(options.GetCorsRules)
                        {
                            blob.PrintCors();
                        }

                        if (!string.IsNullOrEmpty(options.SetCorsRuleMethods))
                        {
                            blob.SetCorsRule(options.SetCorsRuleMethods, options.SetCorsRuleOrigins, options.SetCorsRuleHeadersExposed, options.SetCorsRuleHeadersAllowed, options.SetCorsRuleMaxAge);
                        }
                        if (options.RemoveCorsRules)
                        {
                            blob.RemoveCors();
                        }
                        if (options.GetListContainer)
                        {
                            blob.PrintContainers();
                        }

                        if(!string.IsNullOrEmpty(options.MoveFromContainer) && !string.IsNullOrEmpty(options.MoveToContainer))
                        {
                            blob.Move(options.MoveFromContainer, options.MoveToContainer, options.CsvPath);
                        }

                        if(!string.IsNullOrEmpty(options.GetSASPolicies))
                        {
                            blob.PrintSASPolicies(options.GetSASPolicies);
                        }

                        if(!string.IsNullOrEmpty(options.GetAdHocSASContainer) || !string.IsNullOrEmpty(options.GetAdHocSASPermisisons))
                        {
                            if(string.IsNullOrEmpty(options.GetAdHocSASContainer))
                            {
                                Console.WriteLine("Ad Hoc SAS container is missing. Please use GetAdHocSASContainer to specify the container you want Ad Hoc SAS to apply to, or as the base for the BLOB lookup");
                                return;
                            }
                            if(string.IsNullOrEmpty(options.GetAdHocSASPermisisons))
                            {
                                Console.WriteLine("Ad Hoc SAS permissions is missing. Please use GetAdHocSASPermisisons to specify the permission level you want to apply to your SAS");
                                return;
                            }
                            blob.GetAdHocSAS(options.GetAdHocSASContainer, options.GetAdHocSASBlob, options.GetAdHocSASPermisisons, options.GetAdHocSASStart, options.GetAdHocSASExpire);
                        }

                        if (!string.IsNullOrEmpty(options.SetServiceSASPolicyContainer) || !string.IsNullOrEmpty(options.SetServiceSASPolicyPermissions) ||
                            !string.IsNullOrEmpty(options.SetServiceSASPolicyName))
                        {
                            if(string.IsNullOrEmpty(options.SetServiceSASPolicyContainer))
                            {
                                Console.WriteLine("New SAS policy container is missing. Please use SetServiceSASPolicyContainer to specify the container this policy will apply to");
                                return;
                            }

                            if(string.IsNullOrEmpty(options.SetServiceSASPolicyName))
                            {
                                Console.WriteLine("New SAS policy name is missing. Please use SetServiceSASPolicyName to speficy the policy name");
                                return;
                            }
                            if(string.IsNullOrEmpty(options.SetServiceSASPolicyPermissions))
                            {
                                Console.WriteLine("New SAS policy permissions property is missing. Please use SetServiceSASPolicyPermissions to speficy the level of access for the policy");
                                return;
                            }
                            blob.SetSASPolicy(options.SetServiceSASPolicyContainer, options.SetServiceSASPolicyName, options.SetServiceSASPolicyExpire, options.SetServiceSASPolicyStart, options.SetServiceSASPolicyPermissions);
                        }
                        if(!string.IsNullOrEmpty(options.RemoveServiceSASPolicyContainer))
                        {
                            blob.RemoveSAS(options.RemoveServiceSASPolicyContainer);
                        }
                    }
                    else if (options.Service.Equals("QUEUE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var queue = new QueueService(account.CreateCloudQueueClient());
                        Console.WriteLine("SERVICE QUEUE {0}", queue.Url);
                        if (!string.IsNullOrWhiteSpace(options.CsvToQueue) && !string.IsNullOrWhiteSpace(options.CsvPath))
                        {
                            queue.CsvLoad(options.CsvToQueue, options.CsvPath, options.CsvJson);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
    }
}
