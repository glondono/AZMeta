using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZMeta
{
    public class ServiceProp
    {
        private readonly ServiceProperties prop;

        public ServiceProp(ServiceProperties properties)
        {
            this.prop = properties;
        }

        public void PrintProperties()
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

        public void PrintCors(string methods, string allowedHeaders, string allowOrigins, string exposedHeaders, int maxage)
        {
            Console.WriteLine("Allowed Methods {0}", methods);
            Console.WriteLine("Allowed Headers {0}", allowedHeaders);
            Console.WriteLine("Allowed Origins {0}", allowOrigins);
            Console.WriteLine("Exposed Headers {0}", exposedHeaders);
            Console.WriteLine("Max age {0} seconds", maxage);
            Console.WriteLine();
        }

        public void PrintCors(CorsRule rule)
        {
            PrintCors(rule.AllowedMethods.ToString(), string.Join(",", rule.AllowedHeaders), string.Join(",", rule.AllowedOrigins), string.Join(",", rule.ExposedHeaders), rule.MaxAgeInSeconds);
        }

        public void PrintCors()
        {
            var cors = prop.Cors;
            if (cors != null && cors.CorsRules != null & cors.CorsRules.Count > 0)
            {
                var i = 1;
                foreach (var item in cors.CorsRules)
                {
                    Console.WriteLine("Cors Rule {0}", i);
                    PrintCors(item);
                    i += 1;
                }
            }
            else
            {
                Console.WriteLine("Cors no set");
            }
        }

        public ServiceProperties RemoveCors()
        {
            prop.Cors.CorsRules.Clear();
            return prop;
        }

        public ServiceProperties AddCorsRule(string methods, string origins, string headersexposed, string headersallowed, int maxage)
        {
            CorsRule rule = new CorsRule();
            rule.AllowedHeaders = headersallowed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            rule.AllowedOrigins = origins.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            rule.ExposedHeaders = headersexposed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            rule.MaxAgeInSeconds = maxage;

            if (methods.IndexOf("GET", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods = CorsHttpMethods.Get;
            }
            if (methods.IndexOf("POST", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Post;
            }
            if (methods.IndexOf("PUT", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Put;
            }
            if (methods.IndexOf("TRACE", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Trace;
            }
            if (methods.IndexOf("CONNECT", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Connect;
            }
            if (methods.IndexOf("DELETE", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Delete;
            }
            if (methods.IndexOf("HEAD", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Head;
            }
            if (methods.IndexOf("MERGE", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Merge;
            }
            if (methods.IndexOf("OPTIONS", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                rule.AllowedMethods |= CorsHttpMethods.Options;
            }
            prop.Cors.CorsRules.Add(rule);
            return prop;
        }
    }
}
