# BetterExceptions

A simple mod for Mount & Blade 2: Bannerlord.<br>
It catches unhandled exceptions thrown by the game and reformats them into html files with all infos given by the exception.<br>
Custom formatting isn't supported yet, however a sample template thruthfully showing the default formating is being provided with the compiled binaries.<br>
<br>
CrashReports will be stored in BetterExceptions/ModuleData/CrashReports after successfull installation.

## Building from source

BetterExecutions depends upon <a href="https://www.github.com/KaySteinhoff/MBEasyMod">MBEasyMod</a>, a collection of helper classes that wrap frequently used functionality.<br>
MBEasyMod is not available as a NuGet package meaning you'll have to pull it too and, depening on where you store them relative to one another, may need to adjust the reference path. After that you can just compile as normal.

## Installation

1. Download the latest release
2. Extract files
3. Copy "BetterExceptions" folder into the "Modules" folder located in the game directory
    <br>
    <br>3.1. If you don't know where your game installation is located you can open the game folder by: going into Steam -> Right click MB2: Bannerlord -> Installed Files -> Browse
    <br>
    <br>
4. Enable BetterExceptions in the mod tab of the game launcher
5. Drag it to the top of the list

It is important that BetterExceptions is at the top of the mod list as only then will it be loaded first and be able to catch any errors that may otherwise be thrown before.
