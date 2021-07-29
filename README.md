# SmartInstaller

Un installeur réalisé sous WPF en C# moderne épuré avec une interface style Windows 11 et simple d'utilisation.

![Capture d'écran](https://raw.githubusercontent.com/eclipium/SmartInstaller/master/screen.png)

## Configuration requise pour la compilation
- .NET Framework 4.8 ou supérieur

- Visual studio 2019 ou supérieur

## Configuration requise pour l'utilisateur
- .NET Framework 4.8 ou supérieur

- Windows 10 ou supérieur

## Génération de l'installeur

Remplacer {AppName} par le nom de l'application, {ImageUrl} par l'url du logo de l'application et {PackageUrl} par l'url du paquet d'installation.

## Paquet d'installation

Doit être un fichier zip contenant: un dossier bin contenant l'application et un fichier package.json sous cette forme:

```json
{
  "Name": "{NomDeApplication}",
  "MainExe": "{NomDeExecutable(exemple: app.exe)}",
  "VersionName": "{NomDeVersion}",
  "VersionCode": {NumeroDeVersion}, 
  "Date": "{DateDePublication}"
}
```

##Exemple :

```json
{
  "Name": "Hieroctive",
  "MainExe": "Hieroctive.exe",
  "VersionName": "1.2",
  "VersionCode": 3, 
  "Date": "02/07/2021"
}
```
