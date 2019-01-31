# Syslog.Server

A simple .NET Core multi threaded syslog server, which saves syslog entries to Azure

## PreRequisite 

### Build
- Visual Studio
- DotNet Core 2.0
- Open UDP Port 514 (syslog)

### Run
- DotNet Core 2.0
- Open UDP Port 514 (syslog)

## How to build
- Clone the Repo
- Use the Visual Studio Publish function (right click on the project -> publish)

## How to run
- Create Azure Storage Account. See [Microsoft Docs](https://docs.microsoft.com/en-us/azure/storage/common/storage-quickstart-create-account?tabs=azure-portal)
- Retrieve the Azure Storage Connection String from the Azure Resource
- Add your Azure Storage Connection String to the appsettings.json
```json
{
  "AzureStorage": {
    "StorageConnectionString": ""
  }
}
```
- dotnet run [Name of the compiled dll]
