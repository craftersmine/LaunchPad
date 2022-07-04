This folder must contain all files `craftersmine.AppLauncher.LauncherBridgeClient` after build such as
  - Main executable `craftersmine.AppLauncher.LauncherBridgeClient.exe`
  - Executable configuration `craftersmine.AppLauncher.LauncherBridgeClient.exe.config`
  - PDB Symbols (not required, but recommended) `craftersmine.AppLauncher.LauncherBridgeClient.pdb`
  - IPC library `SharmIpc.dll`
  - IPC library XML documentaion file (not required, but recommended) `SharmIpc.xml`


You can link this files from build folder of `craftersmine.AppLauncher.LauncherBridgeClient` project with Visual Studio (`Add Item...` -> `Existing item...` -> `Add as Link` button), or manually update them in this folder

Otherwise, application will not work properly (will fail to launch applications and even crash)