using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZMeta
{
    public class BlobService
    {
        private ServiceProp properties;
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

        private readonly CloudBlobClient client;


        public void Move(string origin, string destination, string path)
        {
            Console.Write("getting references... from {0} to {1}", origin, destination);
            var originContainer = client.GetContainerReference(origin);
            var destinationContainer = client.GetContainerReference(destination);
            destinationContainer.CreateIfNotExists();
            Console.WriteLine("\tdone");

            string[] files = null;
            if (path != null)
            {
                files = File.ReadAllLines(path);
            }
            else
            {
                files = originContainer.ListBlobs().Select(t => t.Uri.Fragment).ToArray();
            }
            Console.WriteLine("preparing to move {0} files", files.Count());
            var j = 0;
            foreach (var file in files)
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    try
                    {
                        var originBlob = originContainer.GetBlockBlobReference(file);
                        Console.Write("\r{0}% ({1}/{2}) - moving {3} , {4}Kb", (j * 100) / files.Count(), j, files.Count(), file, originBlob.StreamWriteSizeInBytes / 1024);
                        var destinationBlob = destinationContainer.GetBlockBlobReference(file);
                        originBlob.DownloadToStream(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        destinationBlob.UploadFromStream(stream);

                        originBlob.Delete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Coudl not move file {0} because {1}", file, ex.Message);
                    }
                    j += 1;
                }
            }
        }

        public BlobService(CloudBlobClient client)
        {
            this.client = client;
            this.properties = new ServiceProp(client.GetServiceProperties());
        }

        public void RemoveCors()
        {
            Console.Write("removing cors rules...");
            var prop = properties.RemoveCors();
            client.SetServiceProperties(prop);
            Console.WriteLine("\tdone");
            properties.PrintCors();
        }

        public void SetCorsRule(string methods, string origins, string headersexposed, string headersallowed, int maxage)
        {
            Console.WriteLine("creating new CORS rule...");   
            var prop = properties.AddCorsRule(methods,origins,headersexposed,headersallowed,maxage);
            client.SetServiceProperties(prop);
            Console.WriteLine("\tdone");
            properties.PrintCors();
        }

        public void PrintProperties()
        {
            properties.PrintProperties();
           
        }

        public void PrintCors()
        {
            properties.PrintCors();
        }

        public void PrintPermissions(string container)
        {

        }

        public void PrintContainers()
        {
            var list = client.ListContainers();
            if (list != null)
            {
                var array = list.ToArray();
                if (array.Length > 0)
                {
                    foreach (var item in array)
                    {
                        Console.WriteLine("{0} {1} ({2})", item.Name, item.Uri, item.GetPermissions().PublicAccess);
                    }
                }
                return;
            }
            Console.WriteLine("Service has no containers");
        }


        private bool CreateContainer(string container)
        {
            Console.WriteLine("container {0} do not exist, do you want to create it? [Y/N]", container);
            var input = Console.ReadLine();
            if(input.Equals("y", StringComparison.InvariantCultureIgnoreCase) || input.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                var blobcontainer = client.GetContainerReference(container);
                blobcontainer.Create();
                return true;
            }
            return false;
        }

        public void PrintSASPolicies(string container)
        {
            var blobcontainer = client.GetContainerReference(container);
            if(!blobcontainer.Exists())
            {
                if(!CreateContainer(container))
                {
                    return;
                }
            }
            var permissions = blobcontainer.GetPermissions();
            Console.WriteLine("Public Access {0}", permissions.PublicAccess.ToString());
            Console.WriteLine("SAS policies for container {0}", container);
            if (permissions.SharedAccessPolicies.Count == 0)
            {
                Console.WriteLine("No policies set");
            }
            else
            {
                foreach (var policy in permissions.SharedAccessPolicies)
                {
                    var policysettings = policy.Value;
                    var policyname = policy.Key;
                    Console.WriteLine("Policy\t\t{0}", policyname);
                    Console.WriteLine("Started\t\t{0}", policysettings.SharedAccessStartTime);
                    Console.WriteLine("Expire\t\t{0}", policysettings.SharedAccessExpiryTime);
                    Console.WriteLine("Permissions\t\t{0}", policysettings.Permissions.ToString());
                }
            }
        }

        public void SetSASPolicy(string container, string name, int expire, int start, string permissions)
        {
            var blobcontainer = client.GetContainerReference(container);
            if (!blobcontainer.Exists())
            {
                if (!CreateContainer(container))
                {
                    return;
                }
            }
            Console.Write("Creating SAS policy for {0}...", container);
            var newpolicy = new SharedAccessBlobPolicy();
            newpolicy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(expire);
            if(start > 0)
            {
                newpolicy.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(start);
            }
            if(expire > 0)
            {
                newpolicy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(expire);
            }
            if (permissions.IndexOf("read", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newpolicy.Permissions = SharedAccessBlobPermissions.Read;
            }
            if(permissions.IndexOf("write", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newpolicy.Permissions |= SharedAccessBlobPermissions.Write;
            }
            if (permissions.IndexOf("add", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newpolicy.Permissions |= SharedAccessBlobPermissions.Add;
            }
            if (permissions.IndexOf("create", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newpolicy.Permissions |= SharedAccessBlobPermissions.Create;
            }
            if (permissions.IndexOf("delete", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newpolicy.Permissions |= SharedAccessBlobPermissions.Delete;
            }
            if (permissions.IndexOf("list", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newpolicy.Permissions |= SharedAccessBlobPermissions.List;
            }
            if (permissions.IndexOf("none", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newpolicy.Permissions |= SharedAccessBlobPermissions.None;
            }

            var blobpermissions = blobcontainer.GetPermissions();
            blobpermissions.SharedAccessPolicies.Add(name, newpolicy);
            blobpermissions.PublicAccess = BlobContainerPublicAccessType.Off;

            blobcontainer.SetPermissions(blobpermissions);
            Console.WriteLine("\t\tdone");
        }

        public void RemoveSAS(string container, string policy)
        {
            var blobcontainer = client.GetContainerReference(container);
            if (!blobcontainer.Exists())
            {
                if (!CreateContainer(container))
                {
                    return;
                }
            }
            var blobpermissions = blobcontainer.GetPermissions();
            if (string.IsNullOrEmpty(policy))
            {
                Console.Write("removing all service SAS policies on {0}...", container);
                blobpermissions.SharedAccessPolicies.Clear();
                
            }
            else
            {
                Console.Write("removing service SAS policy {0} on {1}...", policy, container);
                blobpermissions.SharedAccessPolicies.Remove(policy);
            }
            blobcontainer.SetPermissions(blobpermissions);
            Console.WriteLine("\t\tdone");
        }

        public void GetAdHocSAS(string container, string blob, string permissions, int start, int expire)
        {
            var blobcontainer = client.GetContainerReference(container);
            if (!blobcontainer.Exists())
            {
                Console.WriteLine("Container {0} do not exist.", container);
                return;
            }
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy();
            if(permissions.IndexOf("Add", StringComparison.InvariantCultureIgnoreCase) > -1){
                policy.Permissions = SharedAccessBlobPermissions.Add;
            }
            if (permissions.IndexOf("Create", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                policy.Permissions |= SharedAccessBlobPermissions.Create;
            }
            if (permissions.IndexOf("Delete", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                policy.Permissions |= SharedAccessBlobPermissions.Delete;
            }
            if (permissions.IndexOf("List", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                policy.Permissions |= SharedAccessBlobPermissions.List;
            }
            if(permissions.IndexOf("None", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                policy.Permissions |= SharedAccessBlobPermissions.None;
            }
            if (permissions.IndexOf("Read", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                policy.Permissions |= SharedAccessBlobPermissions.Read;
            }
            if (permissions.IndexOf("Write", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                policy.Permissions |= SharedAccessBlobPermissions.Write;
            }
            if (start > 0)
            {
                policy.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(start);
            }
            if (expire > 0)
            {
                policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(expire);
            }
  
            if(!string.IsNullOrEmpty(blob))
            {
                var blobref = blobcontainer.GetBlobReference(blob);
                Console.WriteLine("SAS URL for BLOB {0}", blobref.Name);
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobref.Uri, blobref.GetSharedAccessSignature(policy)));
            }
            else
            {
                Console.WriteLine("SAS URL for container {0}", blobcontainer.Name);
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}{1}", blobcontainer.Uri, blobcontainer.GetSharedAccessSignature(policy)));
            }
        
        }
    }
}
