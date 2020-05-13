; Variables:
#define MyAppName "RedEye"
#define MyAppVersion "1.4.1"
#define MyAppPublisher "Swerik"
#define MyAppURL "https://github.com/TheSwerik/RedEye"   
#define MyAppExeName "RedEye.exe"
#define MyAppIconName "RedEye.ico"

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
SetupIconFile=assets\{#MyAppIconName}
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked  
 
[Dirs]
Name: "{app}\bin\assets"; Permissions: users-modify

[Files]
Source: "Publish\bin\*"; DestDir: "{app}\bin"; Excludes:"*.pdb;Publish\bin\config.csv;Publish\bin\assets\examples\*"; Check: Is64BitInstallMode; Flags: ignoreversion recursesubdirs   
Source: "Publish\bin\config.csv"; DestDir: "{userdocs}\{#MyAppName}"; Check: Is64BitInstallMode; Flags: ignoreversion recursesubdirs   
Source: "Publish\bin\assets\examples\*"; DestDir: "{userdocs}\..\Pictures\{#MyAppName}\Sources\examples"; Check: Is64BitInstallMode; Flags: ignoreversion recursesubdirs   

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Flags: createonlyiffileexists;
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Tasks: desktopicon; Flags: createonlyiffileexists;
Name: "{app}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Flags: createonlyiffileexists;

[Run] 
Filename: "{app}\bin\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Check: IsWin64; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{userdocs}\RedEye\config.csv"