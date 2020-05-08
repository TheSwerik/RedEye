; Variables:
#define MyAppName "RedEye"
#define MyAppVersion "1.3.2"
#define MyAppPublisher "Swerik"
#define MyAppURL "https://github.com/TheSwerik/RedEye"   
#define MyAppExeName "RedEye.exe"
;#define MyAppIconName "RedEye.ico"

[Setup]
AppId={{0F48D403-E6A3-4037-8B6D-1F3D032D473A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL} 
AppVerName={#MyAppName}
DefaultDirName={autopf}\{#MyAppName}   
DefaultGroupName={#MyAppName} 
Compression=lzma2   
SolidCompression=yes   
WizardStyle=modern
OutputBaseFilename={#MyAppName}
OutputDir=Installer
ArchitecturesInstallIn64BitMode=x64  
AllowNoIcons=yes  
ShowLanguageDialog=auto
CloseApplications=yes
CloseApplicationsFilter=*.*

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked   

[Dirs]
Name: "{autodocs}\RedEye"

[Files]
Source: "Publish\bin\*"; DestDir: "{app}\bin"; Excludes:"*.pdb;Publish\bin\config.csv"; Check: Is64BitInstallMode; Flags: ignoreversion recursesubdirs   
Source: "Publish\bin\config.csv"; DestDir: "{autodocs}\RedEye\config.csv"; Check: Is64BitInstallMode; Flags: ignoreversion recursesubdirs   

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Flags: createonlyiffileexists;
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Tasks: desktopicon; Flags: createonlyiffileexists;
Name: "{app}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Flags: createonlyiffileexists;

[Run] 
Filename: "{app}\bin\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Check: IsWin64; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{autodocs}\RedEye\config.csv"