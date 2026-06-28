# OrganizeME — V1 (MVP)

OrganizeME watches your Downloads folder and automatically sorts new
files into category folders (Documents, Pictures, Videos, etc.) based on
their file extension. It runs quietly in the system tray, keeps a
history of everything it moves, and shows a Windows notification each
time it organizes a file.

This is **Version 1**. AI-based smart categorization (via Ollama) is
planned for Version 2 — the code is structured so that feature can be
added later without rewriting anything.

---

## 1. What you need before you start

| Tool | Why | Link |
|---|---|---|
| Windows 10/11 | WPF only runs on Windows | — |
| .NET 8 SDK | Compiles and runs the app | https://dotnet.microsoft.com/download/dotnet/8.0 |
| VS Code | Editor | https://code.visualstudio.com |
| C# Dev Kit (VS Code extension) | C# language support, build/debug | Install from inside VS Code |

### Install the .NET 8 SDK
1. Go to the link above, download **.NET 8.0 SDK** for **Windows x64**.
2. Run the installer (defaults are fine).
3. Open a terminal (PowerShell) and check it worked:
   ```
   dotnet --version
   ```
   You should see `8.0.x`.

### Install VS Code extensions
1. Open VS Code → Extensions tab (the squares icon on the left).
2. Search for **"C# Dev Kit"** (publisher: Microsoft) → Install.
3. Restart VS Code.

That's the entire toolchain — no need for full Visual Studio.

---

## 2. Open the project

1. Unzip the OrganizeME project folder somewhere, e.g. `C:\Dev\OrganizeME`.
2. Open VS Code → **File → Open Folder** → select that `OrganizeME` folder
   (the one containing `OrganizeME.sln`).
3. VS Code will detect the C# project and may ask to add required assets
   for building/debugging — click **Yes**.

## 3. Restore packages (download the libraries the project needs)

Open a terminal in VS Code (`` Ctrl+` ``) and run:

```
dotnet restore
```

This downloads SQLite, Serilog, the tray-icon library, etc. — everything
listed in `src/OrganizeME/OrganizeME.csproj`. You only need to do this
once (and again later if you add new packages).

## 4. Build the project

```
dotnet build
```

If this finishes with `Build succeeded`, you're good. If you see red
errors, scroll up — the first error is usually the real one; everything
after it is often a side effect.

## 5. Run the app

```
dotnet run --project src/OrganizeME/OrganizeME.csproj
```

Or, in VS Code, just press **F5** (this uses the `.vscode/launch.json`
already included in the project).

**What you should see:** a window titled "OrganizeME" opens, showing a
status card and an empty "Recent Activity" list. It's already watching
your real Downloads folder. A tray icon also appears near your clock.

### Try it out
1. Download literally any file (a PDF, an image, anything) into your
   Downloads folder, or just copy/paste a file into it.
2. Within a couple of seconds, OrganizeME should move it into a new
   subfolder like `Downloads\Documents\` or `Downloads\Pictures\`, show
   a notification, and add a row to "Recent Activity".

### Closing vs. quitting
- Clicking the window's **X** just hides the window — OrganizeME keeps
  running in the tray (so it can keep organizing files in the
  background).
- To fully quit, **right-click the tray icon → Exit**.

---

## 6. Project structure (where everything lives)

```
OrganizeME/
├── OrganizeME.sln                  ← open this if you ever install full Visual Studio
├── .vscode/                        ← F5 debug + build task config for VS Code
└── src/OrganizeME/
    ├── App.xaml / App.xaml.cs      ← startup: wires up DI, logging, tray icon
    ├── OrganizeME.csproj           ← project file: target framework + NuGet packages
    ├── Models/                     ← plain data classes (FileHistoryEntry, FolderRule, AppSettings)
    ├── Services/                   ← the actual logic (watching, moving, db, settings, notifications)
    │   └── Interfaces/             ← contracts the ViewModels depend on (not the concrete classes)
    ├── ViewModels/                 ← MVVM "brains" behind each window
    ├── Views/                      ← the actual windows (XAML) and their code-behind
    ├── Data/                       ← SQLite connection/schema setup
    ├── Helpers/                    ← RelayCommand, value converters
    └── Resources/                  ← icon, color/style theme
```

### The pipeline, in one sentence
`FileWatcherService` notices a new file → raises an event →
`MainViewModel` hands it to `FileOrganizerOrchestrator` → which asks
`ExtensionFileCategorizer` what category it is → asks `FileMoverService`
to move it → records the result via `SqliteHistoryRepository` → shows a
toast via `ToastNotificationService`.

Every one of those pieces is hidden behind an interface
(`IFileWatcherService`, `IFileCategorizer`, etc.) and wired together in
`App.xaml.cs` → `ConfigureServices`. That's "Dependency Injection" — it
means each piece can be tested or swapped independently. For instance,
Version 2's AI categorizer will be a new class implementing
`IFileCategorizer` that you register *instead of*
`ExtensionFileCategorizer` — nothing else in the app needs to change.

---

## 7. Where things are saved

- **Settings**: `%AppData%\OrganizeME\settings.json`
- **History database**: `%AppData%\OrganizeME\organizeme.db` (SQLite)
- **Logs**: `%AppData%\OrganizeME\logs\organizeme-<date>.log`

(`%AppData%` is usually `C:\Users\<you>\AppData\Roaming`.) If something
isn't behaving as expected, the log file is the first place to look —
every action OrganizeME takes is recorded there via Serilog.

---

## 8. Common beginner issues

**"dotnet: command not found" / not recognized**
→ The SDK installer adds `dotnet` to your PATH automatically, but you
need to **open a new terminal** after installing (old terminals don't
pick up PATH changes).

**Build fails mentioning a missing package**
→ Run `dotnet restore` again — sometimes a fresh clone/unzip needs it
before the first build.

**The app starts but nothing gets organized**
→ Check Settings (gear icon) to confirm the monitored folder is correct,
and that "Automatically move files" is checked. Also check the log file
above — it logs exactly what it saw and why it did (or didn't) move a
file.

**Windows Defender / SmartScreen warning when running the .exe directly**
→ Normal for unsigned hobby apps. This only matters once you `publish`
a standalone .exe to share with others — for development via
`dotnet run`/F5 it won't appear.

---

## 9. Packaging it as a real installable app (later, once V1 feels solid)

When you're ready to install this for real (not just `dotnet run` from
source), publish a self-contained build:

```
dotnet publish src/OrganizeME/OrganizeME.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

This produces a single `OrganizeME.exe` under
`src/OrganizeME/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/`
that runs on any Windows 10/11 PC without needing .NET installed. Inno
Setup (mentioned in your original spec) can then wrap that `.exe` into a
proper installer with a Start Menu shortcut — that's a good V1.1 task
once the app itself works the way you want.

---

## 10. What's next (Version 2 — already scaffolded for)

The "Enable AI-assisted categorization" checkbox in Settings is visible
but disabled — `AppSettings.AiEnabled` already exists in the data model
and gets saved/loaded, it's just not wired to anything yet. To add
Ollama-based categorization later:

1. Create `OllamaFileCategorizer : IFileCategorizer` in `Services/`.
2. In `App.xaml.cs`, swap which categorizer is registered based on
   `settings.AiEnabled` (or have `OllamaFileCategorizer` wrap
   `ExtensionFileCategorizer` as a fallback when AI is off/uncertain).
3. Enable the checkbox in `SettingsWindow.xaml` by removing
   `IsEnabled="False"`.

No other file needs to change — that's the point of the interface-based
design.
