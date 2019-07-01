# Syslog.Server

A simple .NET Core multi-threaded Syslog server, which saves Syslog entries to Azure Table Storage, Azure ServiceBus or a Local File.

## PreRequisite 

### Build
- Visual Studio, Visual Studio Code, or other .NET IDE's
- DotNet Core 2.2
- Open UDP Port 514 (syslog)

### Run
- DotNet Core 2.2
- Open UDP Port 514 (syslog)
- Configure the AppSettings with your Storage Connection String

## How to build
- Clone the Repo
- Use the Visual Studio Publish function (right click on the project -> publish)

## How to run

There are 3 Options to save the Messages to. 
- Azure Storage Account
- Azure Service Bus
- Local FileSystem (JSON File, not intended for production use)

### Azure Storage Account

- Create Azure Storage Account. See [Microsoft Docs](https://docs.microsoft.com/en-us/azure/storage/common/storage-quickstart-create-account?tabs=azure-portal)
- Retrieve the Azure Storage Connection String from the Azure Resource
- Add your Azure Storage Connection string to the appsettings.json

```json
{
  "StorageEndpointConfiguration": [
    {
      "ConnectionString": "[Your ConnectionString]",
      "Enabled": true,
      "ConnectionType": "TableStorage",
      "Name" : "syslogMessages"
    }
  ]
}
```

### Azure ServiceBus

- Create an Azure ServiceBus See [Microsoft Docs](https://docs.microsoft.com/de-de/azure/service-bus-messaging/service-bus-quickstart-portal)
- Retrieve the Connection String from the Azure ServiceBus (Hidden under "Shared access policy")
- Add your Connection string to the appsettings.json
- Create a Queue in the Service Bus

```json
{
  "StorageEndpointConfiguration": [
    {
      "ConnectionString": "[Your ConnectionString]",
      "Enabled": true,
      "ConnectionType": "ServiceBus",
      "Name" : "[your queue name]"
    }
  ]
}
```

### Local File System

- Create an empty File somewhere in your FileSystem and ensure access
- Add the File Path and File Name to the config

```json
{
  "StorageEndpointConfiguration": [
    {
      "ConnectionString": "[Path to the File]",
      "Enabled": true,
      "ConnectionType": "ServiceBus",
      "Name" : "[File name]"
    }
  ]
}
```

### All together

You can configure multiple outputs (even multiple times the same output type, e.g. 2 table storages)
Just add every output you like to the config and choose the correct ```ConnectionType```.

```json
{
  "StorageEndpointConfiguration": [
    {
      "ConnectionString": "",
      "Enabled": true,
      "ConnectionType": "ServiceBus",
      "Name": "syslogMessages"
    },
    {
      "ConnectionString": "",
      "Enabled": true,
      "ConnectionType": "TableStorage",
      "Name" : "syslogMessages"
    },
    {
      "ConnectionString": "",
      "Enabled": true,
      "ConnectionType": "LocalFile",
      "Name" :  "log.txt"
    }
  ]
}
```

- Run ```dotnet Syslog.Server.dll``` on the command line
