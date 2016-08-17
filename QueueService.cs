using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZMeta
{
    public class QueueService
    {
        private ServiceProp properties;
        private CsvParser csvParser;

        public ServiceProp Properties
        {
            get
            {
                return properties;
            }
        }
        public string Url
        {
            get
            {
                return client.BaseUri.ToString();
            }
        }

        private readonly CloudQueueClient client;


        public QueueService(CloudQueueClient client)
        {
            this.client = client;
            this.properties = new ServiceProp(client.GetServiceProperties());
            this.csvParser = new CsvParser();
        }

        public void PrintProperties()
        {
            properties.PrintProperties();
        }

        public void CsvLoad(string queue, string path, bool headers)
        {
            Console.Write(string.Format("parsing file {0} ...", path));
            string[] msgs = csvParser.Parse(path, headers);
            Console.WriteLine("\t\tdone");

            if(!string.IsNullOrWhiteSpace(queue) && msgs != null && msgs.Length > 0)
            {
               Console.Write(string.Format("getting {0} reference...", queue));
               var queueRef = client.GetQueueReference(queue);
               queueRef.CreateIfNotExists();
               Console.WriteLine("\tdone");
               for(var i = 0; i < msgs.Length; i++)
               {
                   queueRef.AddMessage(new CloudQueueMessage(msgs[i]));
                   int progress = (int)Math.Round(((i + 1) * 100f) / msgs.Length);
                   Console.Write("\rcreating... {0}%  ", progress);
               }
               Console.WriteLine("\t\tdone");
            }
            else
            {
                Console.WriteLine("\t\tno queue or file is empty");
            }
        }
    }
}
