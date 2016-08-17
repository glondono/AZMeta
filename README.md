## Overview
AzMeta is a Windows command-line utility tool to perform certain task that are not possible using the Azure Portal.
Most of the task are related to one time configurations or if you are doing client side development, (SPA and/or hybrid apps).
Task like setting CORS,  hydrate queues using data on premise and creating SAS policies and Ad Hoc SAS for testing are some of the functionality you can do with AzMeta without any programming. This guide assumes that you are already familiar with [Azure Storage](https://azure.microsoft.com/services/storage/), and that you have an storage account. 

## Clone, build and run
Use Visual Studio to build and generate .exe files. On windows, open CMD on your project bin folder to run AzMeta.exe

## Help
To see all available parameters, use --help.

To get you connection string, open your storage account (classic) on the [Azure Portal](https://portal.azure.com), under Keys.

If you have your account name and account key, your connection string will be

```
DefaultEndpointsProtocol=https; AccountName= your-storage-account-name; AccountKey= your-storage-account-key
```

# Cross-Origin resource sharing (CORS)
If you plan access a container directly from the browser, you will need to set a CORS rule in your storage service to allow the cross domain request from your custom domain. The scenarios I came across was using a container to host my static content and uploading files directly from the client side using Javascript. 

## GET
To get all the CORS rules that are applied to your service account, use

```
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --GetCorsRules
```

## SET
To set a new CORS rule you may use the following parameters

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| SetCorsRuleMethods   | (DELETE, GET, HEAD, MERGE, POST, OPTIONS and PUT) that the origin domain may use for a CORS request. Use comma to separate multiple entries. | YES |  |
| SetCorsRuleOrigins   | The origin domains that are permitted to make a request against the storage service via CORS (use * for all or use comma for multiple domain entries) | NO | * |
| SetCorsRuleHeadersAllowed | The request headers that the origin domain may specify on the CORS request (use * for all headers) | NO | * |
| SetCorsRuleHeadersExposed | The response headers that may be sent in the response to the CORS request and exposed by the browser to the request issuer (use * for all headers) | NO | * |
| SetCorsRuleMaxAge | The maximum amount time that a browser should cache the preflight OPTIONS request (in seconds) | NO | 1800 |

To allow a container be your host for static files

```
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --SetCorsRuleMethods=GET
```

To allow [posting files from the browser](http://gauravmantri.com/2013/02/16/uploading-large-files-in-windows-azure-blob-storage-using-shared-access-signature-html-and-javascript/)

```
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --SetCorsRuleMethods=GET,PUT --SetCorsRuleOrigins=https://myapp.com
```

## DELETE
This will remove all CORS rules set on the service

```
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --RemoveCorsRules
```

# Share Access Signatures (SAS)
If your container is private, GET and PUT request will also need a [SAS](https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-1/) as a way to grant limited access without sharing keys.
Usually a URL is generated on server side with the appropriate SAS and return it to your client to perform a specific operation (READ, WRITE, LIST, ...) on a BLOB or container for a specific period of time. 
You can create SERVICE SAS Policies or Ad Hoc SAS. Use service SAS policy when you need to give access to multiple people for a period of time and you don't want or can regenerate SAS. For example, create a SAS to allow your team to upload files for the next hour. 
The use of Ad Hoc SAS are targeted, ideally to a specific BLOB for a particular action that is short lived. Usually are generated per application request and those can not be revoked.  
 
## GET
Get the **SERVICE SAS** applied to the specify container

``` 
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --GetServiceSASPoliciesContainer=my-container
```

## SET
To create a new **SERVICE SAS POLICY** on a container you may use the following parameters

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| SetServiceSASPolicyContainer | Container name to set a new Service SAS policy | TRUE | |
| SetServiceSASPolicyName | Policy name | TRUE | |
| SetServiceSASPolicyPermissions | ADD, CREATE, DELETE, LIST, READ, WRITE. Use comma to separate multiple entries | TRUE | |
| SetServiceSASPolicyStart | Service SAS will start in (x) minutes from current time. Leave empty to stat immediately | FALSE | |
| SetServiceSASPolicyExpire | Service SAS will expire in (x) minutes from current time. | TRUE | |

This will create a new policy on a container to allow READ access for the next hour and return the URL to the container with the SAS

```
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --SetServiceSASPolicyContainer=my-container --SetServiceSASPolicyName=ReadForAnHour --SetServiceSASPolicyPermissions=LIST,READ --SetServiceSASPolicyExpire=60
```

To create a new **AD HOC SAS** on a BLOB or container you may use the following parameters

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| GetAdHocSASContainer | Set a container for blob lookup when GetAdHocSASBlob is also used, or create SAS for specified container otherwise | TRUE | |
| GetAdHocSASBlob | If specified, SAS will apply to this BLOB | FALSE | |
| GetAdHocSASPermissions | ADD, CREATE, DELETE, LIST, READ, WRITE. Use comma to separate multiple entries | TRUE | |
| GetAdHocSASStart | Ad hoc SAS will start in (x) minutes from current time. Leave empty to stat immediately | FALSE | |
| GetAdHocSASExpire | Ad hoc SAS will expire in (x) minutes from current time. | TRUE | 30 |

This will return a new URL to the Container with the SAS to allow creating a new file for the next 10 minutes

```
AZMeta.exe --ConnectionString=your-connection-string --Service=BLOB --GetAdHocSASContainer=my-container --GetAdHocSASPermissions=CREATE,WRITE --GetAdHocSASExpire=10
```

This will return a new URL to the BLOB with the SAS to allow putting new blocks on a BLOB for the next 30 minutes

```
AZMeta.exe --ConnectionString=your-connection-string --Service=BLOB --GetAdHocSASContainer=my-container --GetAdHocSASBlob=my-blob --GetAdHocSASPermissions=WRITE --GetAdHocSASExpire=30
```

## DELETE
Removing a policy will revoke all SAS created using the policy.

Remove all **Service SAS Policies** to the specify container. 

```
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --RemoveServiceSASPolicyContainer=my-container
```

Remove **One Service SAS Policy** on the specify container.

```
AzMeta.exe --ConnectionString=your-connection-string --Service=BLOB --RemoveServiceSASPolicyContainer=my-container --RemoveServiceSASPolicyName=ReadForAnHour
```

# Hydrate Queues with CSV
You can use the Visual Studio Cloud Explorer to add messages on development, but became impractical when you are migrating your application and need to reprocess thousands of items.
You can export the data you need, create a csv and then use AZMeta to create the messages for you.

To hydrate a queue with a csv, you may use the following parameters

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| CsvToQueue |  QUEUE that will receive the messages | TRUE | |
| CsvPath | Local file path to your csv file | TRUE | |
| CsvJson | If set, row header will be used as properties and the message will be an object in javascript notation. On Queue processing, you can have a class with the same definition to deserialize. If not set, entire row will be the message as string | FALSE | |

To the following CSV file

```
FileID,Name,Size
102,test.txt,1000
103,test2.txt,56
```

## Hydrate queue as objects 

```
AzMeta.exe --ConnectionString=your-connection-string --Service=QUEUE --CsvToQueue=new-files --CsvPath=c:\temp\files.csv --CsvJson
```

You will have the following messages created

Message 1
```javascript
{
    FileID : 102,
    Name : test.txt,
    Size: 1000
}
```
Message 2
```javascript
{
    FileID : 103,
    Name : test2.txt,
    Size: 56
}
```

## Hydrate queue as string

```
AzMeta.exe --ConnectionString=your-connection-string --Service=QUEUE --CsvToQueue=new-files --CsvPath=c:\temp\files.csv
```

You will have the following messages created

Message 1
```
FileID,Name,Size
```

Message 2
```
102,test.txt,1000
```

Message 3
```
103,test2.txt,56
```
