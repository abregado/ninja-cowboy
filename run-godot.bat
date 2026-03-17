@echo off
setlocal

set GODOT=D:\Programs\Godot_v4.6.1-stable_mono_win64\Godot_v4.6.1-stable_mono_win64_console.exe
set PROJECT_DIR=%~dp0
set PROJECT_DIR_NOSLASH=%PROJECT_DIR:~0,-1%

echo [BUILD] Building C# project...
dotnet build "%PROJECT_DIR%Ninja Cowboy.sln" || ( echo BUILD FAILED & exit /b 1 )

echo [BUILD] Success. Launching game...
"%GODOT%" --path "%PROJECT_DIR_NOSLASH%"
endlocal
