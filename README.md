# Syslog.Server

A simple .NET Core multi threaded syslog server, which saves syslog entries to Azure

## PreRequisite 

### Build
- Visual Studio
- DotNet Core 2.1
- Open UDP Port 514 (syslog)

### Run
- DotNet Core 2.1
- Open UDP Port 514 (syslog)
- Configure the AppSettings with your Storage Connection String

## How to build
- Clone the Repo
- Use the Visual Studio Publish function (right click on the project -> publish)

## How to run
- Configure the AppSettings with your Storage Connection String
- dotnet run Syslog.Server.dll
