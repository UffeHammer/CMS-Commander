; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "CMS Commander"
#define MyAppVersion "1.0"
#define MyAppPublisher "Cabana"
#define MyAppURL "http://www.cabana.dk"
#define MyAppExeName "CMSCommander.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{F98B8C7E-475C-4BDA-B67F-0B5D63CD7470}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}           
DefaultGroupName={#MyAppName}
OutputBaseFilename=Setup CMS Commander
Compression=lzma
SolidCompression=yes
OutputDir=.

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\bin\Release\CMSCommander.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Debug\CMSCommander.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\CMSCommander.exe.config"; DestDir: "{app}"
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "..\..\ExtendedSitecoreAPI\Package installer\Sitecore webservice installer-1.0.zip"; DestDir: "{userdocs}\CMS Commander"; DestName: "Sitecore webservice installer.zip"
Source: "..\bin\Release\Core.dll"; DestDir: "{app}"
Source: "..\bin\Release\Core.pdb"; DestDir: "{app}"
Source: "..\bin\Release\Core.XmlSerializers.dll"; DestDir: "{app}"
Source: "..\bin\Release\FilestorageConversionPlugin.dll"; DestDir: "{app}"
Source: "..\bin\Release\FilestorageConversionPlugin.pdb"; DestDir: "{app}"
Source: "..\bin\Release\HtmlToXhtmlPlugin.dll"; DestDir: "{app}"
Source: "..\bin\Release\HtmlToXhtmlPlugin.pdb"; DestDir: "{app}"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{group}\CMS Commander package folder"; Filename: "{userdocs}\CMS Commander"; IconFilename: "{userdocs}\CMS Commander\Sitecore webservice installer.zip"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent