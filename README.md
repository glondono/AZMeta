# Get and Set Metadata, Security and Property information to your Azure Storage account services

## Overview
AzMeta is a Windows command-line utility tool to query and set metadata, security and property information to your services, containers, tables and queues. For example, enable CORS to your services with one command, no complicated REST calls needed. 

> This guide assumes that you are already familiar with [Azure Storage](https://azure.microsoft.com/services/storage/).

## Download and install AZMeta
Donwload the zip file under releases. Unzip and install.

> The current version has a very limited set of options. The initial effort was on getting and enabling CORS on services, but many other functionality may be acommodated for other scenarios where this functionlity is limited in the portal of visual studio tools.

## BLOB Services
### Get Properties
AzMeta --ConnectionString=`azure-connection-string` --Service=BLOB --GetServiceProperties

You can use `UseDevelopmentStorage=true` as your connection string for your azure emulator.
The result of the command will display the following service properties:

* Service Version
* Metric Level
* Metric Retention Days
* Metric Version
* Loggin Retention Days
* Loggin Operations
* CORS rules
  
### Set New CORS Rule
AzMeta --ConnectionString=`azure-connection-string` --Service=BLOB --SetCorsRule=`allowed-methods=get|put|post;allowed-header=*;allowed-origins=*;exposed-header=*;max-age=1800`

**allowed-methods** : GET | POST | DELETE | TRACE | CONNECT | HEAD | MERGE | OPTIONS

**allowed-headers** : Comma separated string with allowed headers request (`*` for all)

**allowed-origins** : Comma separated string with allowed origins domains (`*` for all)

**exposed-header**  : Comma separated string with allowed header returns (`*` for all)

**max-age**         : In seconds for prefligh cache

> If you are not sure about the values you need to set in your CORS rule, you can read more [here](https://developer.mozilla.org/en-US/docs/Web/HTTP/Access_control_CORS).

