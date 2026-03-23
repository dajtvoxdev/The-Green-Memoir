; ============================================================
; Moonlit Garden - Inno Setup Script
; The Green Memoir - A cozy farming game
; ============================================================
; How to use:
;   1. Install Inno Setup from https://jrsoftware.org/isinfo.php
;   2. Open this file in Inno Setup Compiler
;   3. Click "Compile" (Ctrl+F9)
;   4. Output: web/public/game/Setup_MoonlitGarden.exe
; ============================================================

#define MyAppName "Moonlit Garden"
#define MyAppVersion "0.03"
#define MyAppPublisher "Moonlit Garden Studio"
#define MyAppURL "https://github.com/YourRepo/Moonlit-Garden"
#define MyAppExeName "MoonlitGarden.exe"
#define BuildDir "..\Builds"

[Setup]
; App identity
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Install location
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
DisableProgramGroupPage=yes

; Output
OutputDir=..\web\public\game
OutputBaseFilename=Setup_MoonlitGarden_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
LZMANumBlockThreads=4

; Appearance
WizardStyle=modern
SetupIconFile=..\Assets\icon_game.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Requirements
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0

; Privileges
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "Create a Start Menu shortcut"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Main executable
Source: "{#BuildDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Unity runtime DLLs
Source: "{#BuildDir}\UnityPlayer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\UnityCrashHandler64.exe"; DestDir: "{app}"; Flags: ignoreversion

; Game data folder
Source: "{#BuildDir}\MoonlitGarden_Data\*"; DestDir: "{app}\MoonlitGarden_Data"; Flags: ignoreversion recursesubdirs createallsubdirs

; Mono runtime
Source: "{#BuildDir}\MonoBleedingEdge\*"; DestDir: "{app}\MonoBleedingEdge"; Flags: ignoreversion recursesubdirs createallsubdirs

; D3D12 support
Source: "{#BuildDir}\D3D12\*"; DestDir: "{app}\D3D12"; Flags: ignoreversion recursesubdirs createallsubdirs

; Exclude debug info (not needed for players)
; The BurstDebugInformation folder is intentionally excluded

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Clean up save data and logs on uninstall (optional — user can keep saves)
; Type: filesandordirs; Name: "{localappdata}\{#MyAppPublisher}\{#MyAppName}"
