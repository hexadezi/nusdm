# nusdm
Download Wii U games from the Nintendo Update Server.

The keys will be generated. They are not in the json file.

## Screenshot
![](https://github.com/"")

## Prerequisites
[.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)

## Usage
Download the package. Put the Common Key in the config.json file and start the executable.

## Settings

Setting | Explanation
------------ | -------------
CommonKey | Hier ist nicht viel zu erklären.
TitleFile | The file that contains the titles in JSON format. <br/> The following properties are read: **titleID, titleKey, name, region** <br/> If no key is specified, one is generated.
SavePath | The path should have the following format: C:\\\Users\\\Dave\\\Desktop\\\
NintendoBaseUrl | Address of the Content Delivery Network
DownloadParallel | If true, the files will be downloaded parallel, which should result in a higher download speed.
Decrypt | If true, the downloaded files will be decrypted after download. <br/> CDecrypt is downloaded on startup and stored in the tmp directory.
DeleteAppFiles | Files will be deleted after decryption.
SkipExistingFiles | It will skip download, if files exist. Sometimes files are downloaded even though they exist. <br/> This is due to incorrect information in the metadata.
DownloadH3Files | If true, h3 files will be downloaded.

## Download
Download here: https://github.com/labo89/nusdm/releases

## Dependencies
The following dependencies are included in the executable.

[Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)

## Acknowledgments
[damysteryman/FunKiiUNETThingy](https://github.com/damysteryman/FunKiiUNETThingy)

[FoxMcloud5655/NEW-NUSGrabberGUI](https://github.com/FoxMcloud5655/NEW-NUSGrabberGUI)

[V10lator/NUSspli](https://github.com/V10lator/NUSspli)

[crediar/cdecrypt](https://code.google.com/archive/p/cdecrypt)

[Pixel perfect](https://www.flaticon.com/de/autoren/pixel-perfect)

## License
This project is licensed under the MIT License - see the LICENSE file for details
