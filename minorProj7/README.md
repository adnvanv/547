# Minor Project 7: Cross-Platform (Avalonia)

**Stay Planner** is an Avalonia UI desktop app (.NET 10) for quoting and booking hotel rooms. It was written and built on Windows 11, published for Linux (`linux-x64`) from that Windows machine, and run on Debian GNU/Linux 12.

- **Client** (`StayPlanner.Desktop`): Avalonia 11.3 window with MVVM (CommunityToolkit.Mvvm). Pick dates, guests and a room, see an itemized quote, book or cancel. The header shows the OS the app is running on.
- **Business layer** (`StayPlanner.Business`): `QuoteManager` (nightly rate, 20% Friday/Saturday surcharge, 10% weekly discount, $15 extra-guest fee, 8.75% tax) and `ReservationManager` (validation, availability, double-booking check).
- **Data layer** (`StayPlanner.Data`): rooms from an embedded `rooms.json`; reservations saved as JSON in the user's app-data folder (`%APPDATA%\StayPlanner` on Windows, `~/.config/StayPlanner` on Linux).

## Layout

```
StayPlanner/
  Code/StayPlanner.sln
  Code/StayPlanner.Data/        data layer (entities, repositories, rooms.json)
  Code/StayPlanner.Business/    business layer (quotes, bookings, rules)
  Code/StayPlanner.Desktop/     Avalonia client (views, view models)
  Scripts/linux/                Dockerfile + scripts that run the Linux build on a virtual X11 desktop
Documentation/
  MinorProject7_CrossPlatform.docx
  screenshots/
```

## Run on Windows (dev machine)

Needs the .NET 10 SDK. NuGet restores Avalonia on the first build.

```
cd StayPlanner/Code
dotnet run --project StayPlanner.Desktop
```

Or open `StayPlanner/Code/StayPlanner.sln` in Visual Studio 2022/2026 and press F5.

## Build for Linux

On any OS with the .NET 10 SDK:

```
cd StayPlanner/Code
dotnet publish StayPlanner.Desktop -c Release -r linux-x64 -o ../publish/linux-x64
```

The output is one self-contained executable, `StayPlanner` (about 95 MB). The target machine does not need .NET installed. Copy it to a Linux desktop with X11 (Ubuntu, Debian, Fedora and so on), then:

```
chmod +x StayPlanner
./StayPlanner
```

A bare Linux image may need `libx11-6 libice6 libsm6 libfontconfig1` and a font package. Desktop distributions already have them. Use `-r osx-arm64` or `-r win-x64` to build for macOS or Windows.

### Linux run used for the screenshots

`StayPlanner/Scripts/linux/run-linux.ps1` (needs Docker Desktop) publishes the `linux-x64` build, builds a Debian 12 image with a virtual X11 display (Xvfb), the fluxbox window manager and xterm, and starts the app in it. No .NET is installed in the image.

```
powershell -ExecutionPolicy Bypass -File StayPlanner/Scripts/linux/run-linux.ps1 -Screenshot linux.png
docker rm -f stayplanner-linux
```
