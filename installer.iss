; ─────────────────────────────────────────────────────────────────────────────
; OrganizeME — Inno Setup script
; Called by GitHub Actions with:  ISCC.exe /DAppVersion=1.1.0 installer.iss
; ─────────────────────────────────────────────────────────────────────────────

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName      "OrganizeME"
#define AppPublisher "Adeepa Wedage"
#define AppExeName   "OrganizeME.exe"

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/ANWedage/OrganizeME
AppSupportURL=https://github.com/ANWedage/OrganizeME/issues
AppUpdatesURL=https://github.com/ANWedage/OrganizeME/releases
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir=installer-output
OutputBaseFilename={#AppName}-v{#AppVersion}-Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#AppExeName}
; Use Windows 10+ style wizard
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
; All published files from the dotnet publish step
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}";           Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#AppName}";   Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Kill the running process before uninstalling so files are not locked
Filename: "taskkill.exe"; Parameters: "/F /IM {#AppExeName}"; Flags: runhidden skipifdoesntexist; RunOnceId: "KillApp"

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDataFolder: String;
  MsgResult: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    AppDataFolder := ExpandConstant('{userappdata}\OrganizeME');
    if DirExists(AppDataFolder) then
    begin
      MsgResult := MsgBox(
        'Do you want to remove all OrganizeME data?' + #13#10 +
        'This includes your settings, file history, and logs.' + #13#10#13#10 +
        AppDataFolder,
        mbConfirmation, MB_YESNO);
      if MsgResult = IDYES then
        DelTree(AppDataFolder, True, True, True);
    end;
  end;
end;
