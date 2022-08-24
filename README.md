# SmartInstaller

![Smart Installer](https://raw.githubusercontent.com/eclipium/SmartInstaller/master/smart_installer.png)


A software installer made under WPF in modern C# with a Windows 11 style interface and easy to use.
It also includes a dark theme that matches the windows theme.

| ![Screenshot - Light](https://raw.githubusercontent.com/eclipium/SmartInstaller/master/SmartInstaller-White.png) | ![ Screenshot - Dark](https://raw.githubusercontent.com/eclipium/SmartInstaller/master/SmartInstaller-Dark.png) |
|----------------------------------------------------------------------------------------------------------------|--------------------------------------------------------|
| Light Mode                                                                                                     | Dark Mode                                              |


## Requirements for the compilation

- .NET SDK 6 / .NET Framework Targeting Pack 4.7.2

## User requirements

- .NET Desktop Runtime 6 / .NET Framework 4.x

- Windows 7 or later

## Generating the installer

Just replace the package.json data and the installation package (package.zip)

## Installation package

Must be a zip file containing: a bin folder containing the application and a package.json file in this form:

```json
{
  "Name": "{ApplicationName}",
  "MainExe": "{ExecutableName(exemple: app.exe)}",
  "VersionName": "{VersionName}",
  "VersionCode": {VersionCode}, 
  "Date": "{PublicationDate}"
}
```

## Example :

```json
{
  "Name": "TetraSwap",
  "MainExe": "TetraSwap.exe",
  "VersionName": "1.3",
  "VersionCode": 10, 
  "Date": "25/04/2022"
}
```
