; Variables:
#define MyAppName "RedEye"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Swerik"
#define MyAppURL "https://github.com/TheSwerik/RedEye"   
#define MyAppExeName "RedEye.exe"
;#define MyAppIconName "RedEye.ico"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)   
; IMPORTANT: This is only an example how the GUID should look like, please generate a new one! 
AppId={{0F48D403-E6A3-4037-8B6D-1F3D032D473A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL} 
;AppComments=
; The Name displayed in the "add or remove programs" page (default is "{#MyAppName} version {#MyAppVersion}")
AppVerName={#MyAppName}
DefaultDirName={autopf}\{#MyAppName}   
DefaultGroupName={#MyAppName} 
Compression=lzma2   
SolidCompression=yes   
; other option is "classic"
WizardStyle=modern
; Filename of the Setup exe
OutputBaseFilename={#MyAppName}
; Changes the Folder where the Setup exe is created
OutputDir=Installer
; Moves the relative Path to the Sources up
;SourceDir=..\       
; will use x64 folder on 64bit PC
ArchitecturesInstallIn64BitMode=x64  
; will show a checkbox for making star menu folder optional
AllowNoIcons=yes  
; Skips the Language Selection if the current PCs language is listed in [Languages]
ShowLanguageDialog=auto
; Makes The Setup Close any Program that uses the Files that the Setup wants zu change (other option: force)
CloseApplications=yes
; Filters which Files are checked by "CloseApplications"  (default: *.exe,*.dll,*.chm)
CloseApplicationsFilter=*.*
; Disables automatic Restart after "CloseApplications" finsihed
;RestartApplications=no
; set the name of a License Agreement File (.txt or .rtf) which is displayed before the user selects the destination directory for the program
; gets ignored if Specified in [Language]
;LicenseFile=
; Makes Explorer Refresh File Associations at the End of the Un-/Installation
;ChangesAssociations=yes  
; Makes Explorer (and other Running Programs) Refresh Environment Variables at the End of the Un-/Installation
;ChangesEnvironment=yes
; Changes Dialog Font
;DefaultDialogFontName=
; Disables the Last "finish Setup" page
;DisableFinishedPage=yes   
; set the name of an optional .txt or .rtf file that gets displayesd after a successful install  
; gets ignored if Specified in [Language]
;InfoAfterFile=   
; set the name of an optional .txt or .rtf file that gets displayesd before the user selects the destination directory for the program  
; gets ignored if Specified in [Language]
;InfoBeforeFile=
; Minimum Windows Version required for installation (exists with error if not met)
;MinVersion=6.0 
; Maximum Windows Version supported for installation (exists with error if not met)
;OnlyBelowVersion=6.0
; Password required for installation (you should also usde Encryption if you set a password)
;Password=
; Encrypts the files (only makes sense with a Password)
;Encryption=yes
; Sets the Required Privileges to regular user (non-admin)
;PrivilegesRequired=lowest        
; Lets the user choose the Privileges (other option "console")
;PrivilegesRequiredOverridesAllowed=dialog
; Determines if uninstaller should be included
;Uninstallable=not IsTaskSelected('portablemode')
; Determines if "uninstall" should be Displayed under Programs section
;CreateUninstallRegKey=not IsTaskSelected('portablemode')
; specifies a different Icon for the Unsintaller (can be from .ico or .exe)
;UninstallDisplayIcon={app}\{#MyAppIconName}   
; specify Icon for Setup
;SetupIconFile={#MyAppIconName}

; Makes you select install-Types for [Components]
; is optional, default Types will be created if left empty
;[Types]
;Name: "full"; Description: "Full installation"
;Name: "compact"; Description: "Compact installation"
;Name: "custom"; Description: "Custom installation"; Flags: iscustom   

; Components to select (if at least one is specified, the Wizard will show a "Components" page)
; if no Types are Specified, default ones will be used
; check Flags at https://jrsoftware.org/ishelp/index.php?topic=componentssection
;[Components]
;Name: a; Description: a; Types: full compact

; creates empty Folders
;[Dirs]
;Name: "{app}\bin"

; writes data to INI file
;[INI]
;Filename: "MyProg.ini"; Section: "InstallSettings"; Key: "InstallPath"; String: "{app}"  

; setup Registry Keys, see https://jrsoftware.org/ishelp/index.php?topic=registrysection
;[Registry]

; Specifies a list of languages the Installer will support
; also allows to specify License-Files per language (also InfoBeforeFile and InfoAfterFile)
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

; Custom Checkboxes or Radio Buttons
; can be grouped with "GroupDescription" and assigned to components with "Components"
[Tasks]
; Creates a Desktop Shortcut if checked 
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked   
; activates Portable Mode (for example for "Uninstallable"
;Name: portablemode; Description: "Portable Mode"

; Specifies all files that will be installed, see https://jrsoftware.org/ishelp/index.php?topic=filessection
[Files]
Source: "Publish\bin\64bit\*"; DestDir: "{app}\bin\64bit"; Excludes:"*.pdb"; Check: Is64BitInstallMode;     Flags: ignoreversion recursesubdirs
Source: "Publish\bin\32bit\*"; DestDir: "{app}\bin\32bit"; Excludes:"*.pdb"; Check: not Is64BitInstallMode; Flags: ignoreversion recursesubdirs solidbreak    
; Place all common files here, first one should be marked 'solidbreak'
;Source: "Readme.txt"; DestDir: "{app}"; Flags: isreadme solidbreak

; Creates Shortcuts
; Parameters, HotKeys, WorkingDirectories etc can be specified, see https://jrsoftware.org/ishelp/index.php?topic=iconssection
; common Shortcut Folderconstants:  autoprograms (Prorgams Folder on Start Menu) | autoappdata | uninstallexe | autodocs (Documents Folder) | usersavedgames | autostartup | group | autodesktop
[Icons]
; creates Start Menu Shortcut (64 bit on x64 and 32bit on x86)
Name: "{group}\{#MyAppName}"; Filename: "{app}\bin\64bit\{#MyAppExeName}"; Flags: createonlyiffileexists;
Name: "{group}\{#MyAppName}"; Filename: "{app}\bin\32bit\{#MyAppExeName}"; Flags: createonlyiffileexists; 
; creates Desktop Shortcut (64 bit on x64 and 32bit on x86) 
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\bin\64bit\{#MyAppExeName}"; Tasks: desktopicon; Flags: createonlyiffileexists;
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\bin\32bit\{#MyAppExeName}"; Tasks: desktopicon; Flags: createonlyiffileexists; 
; creates Shortcut in Pragrams Root Folder (64 bit on x64 and 32bit on x86) 
Name: "{app}\{#MyAppName}"; Filename: "{app}\bin\64bit\{#MyAppExeName}"; Flags: createonlyiffileexists;
Name: "{app}\{#MyAppName}"; Filename: "{app}\bin\32bit\{#MyAppExeName}"; Flags: createonlyiffileexists; 

; Specifies Programs that are run after installation but before the final page of the Setup  
; use Flag "shellexec" when file is not directly runnable (for example .txt or a folder)
[Run] 
; in this example the main exe (dependent on Platform):
Filename: "{app}\bin\64bit\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Check: IsWin64; Flags: nowait postinstall skipifsilent
Filename: "{app}\bin\32bit\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Check: not IsWin64; Flags: nowait postinstall skipifsilent

; Specifies Programs that are run before uninstallation
; use Flag "shellexec" when file is not directly runnable (for example .txt or a folder)
;[UninstallRun] 

; Defines any Files or Folders that should be deleted when uninstalling
;[UninstallDelete]
;Type: filesandordirs; Name: "{autodocs}\savefile.sav"