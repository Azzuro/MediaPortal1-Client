rem Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

cd
mkdir plugins
mkdir plugins\windows
mkdir plugins\TagReaders
mkdir plugins\subtitle
mkdir plugins\ExternalPlayers
mkdir plugins\process
mkdir Wizards

del /F /Q plugins\windows\*.*
del /F /Q plugins\tagreaders\*.*
del /F /Q plugins\subtitle\*.*
del /F /Q plugins\ExternalPlayers\*.*
del /F /Q plugins\process\*.*
del *.dll

copy ..\..\..\core\directshowhelper\directshowhelper\release\directshowhelper.dll .
regsvr32 /s directshowhelper.dll
copy ..\..\..\core\fontengine\fontengine\release\fontengine.dll .
copy ..\..\..\Interop.DirectShowHelperLib.dll .
copy ..\..\..\mfc71.dll .
copy ..\..\..\msvcp71.dll .
copy ..\..\..\msvcr71.dll .
copy ..\..\..\Microsoft.ApplicationBlocks*.dll .
copy ..\..\..\d3dx9_26.dll .
copy ..\..\..\Microsoft.DirectX.Direct3D.dll .
copy ..\..\..\Microsoft.DirectX.Direct3DX.dll .
copy ..\..\..\Microsoft.DirectX.DirectDraw.dll .
copy ..\..\..\Microsoft.DirectX.dll .
rem ExternalDisplay plugin LCD driver DLLs
copy ..\..\..\FTD2XX.DLL .
copy ..\..\..\SG_VFD.dll .
if not exist LUI\. mkdir LUI
copy ..\..\..\LUI.dll LUI\.
copy ..\..\..\Communications.dll .
copy ..\..\..\Interop.GIRDERLib.dll .
copy ..\..\..\MediaPadLayer.dll .
rem 
copy ..\..\..\KCS.Utilities.dll .
copy ..\..\..\X10Plugin.* .
copy ..\..\..\X10Unified.* .
copy ..\..\..\xAPMessage.dll .
copy ..\..\..\xAPTransport.dll .
copy ..\..\..\mbm5.dll .
copy ..\..\..\madlldlib.dll .
copy ..\..\..\ECP2Assembly.dll .
copy ..\..\..\edtftpnet-1.1.3.dll .
copy ..\..\..\dvblib.dll .
copy ..\..\..\Interop.WMEncoderLib.dll .
copy ..\..\..\Interop.TunerLib.dll .
copy ..\..\..\Interop.iTunesLib.dll .
copy ..\..\..\Microsoft.Office.Interop.Outlook.dll .
copy ..\..\..\XPBurnComponent.dll .

copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\Configuration\bin\Release\Configuration.exe .
copy ..\..\..\TVGuideScheduler\bin\Release\TVGuideScheduler.exe .

copy ..\..\..\core\bin\Release\Core.dll .
copy ..\..\..\tvcapture\bin\release\tvcapture.dll .
copy ..\..\..\databases\bin\release\databases.dll .
copy ..\..\..\SubtitlePlugins\bin\release\SubtitlePlugins.dll plugins\subtitle
copy ..\..\..\TagReaderPlugins\bin\release\TagReaderPlugins.dll plugins\TagReaders
copy ..\..\..\ExternalPlayers\bin\release\ExternalPlayers.dll plugins\ExternalPlayers
copy ..\..\..\WindowPlugins\bin\release\WindowPlugins.dll plugins\Windows
copy ..\..\..\WindowPlugins\GUIMSNPlugin\DotMSN.dll plugins\Windows
copy ..\..\..\ProcessPlugins\bin\release\ProcessPlugins.dll plugins\process\
copy ..\..\..\Dialogs\bin\release\Dialogs.dll plugins\Windows
copy ..\..\..\RemotePlugins\bin\release\RemotePlugins.dll .

copy ..\..\..\ProcessPlugins\MCEDisplay\bin\release\MCEDisplay.dll plugins\process\
copy ..\..\..\ProcessPlugins\MCEDisplay\bin\release\MCEDisplay.pdb plugins\process\
rem copy ..\..\..\ProcessPlugins\MCEDisplay\bin\release\Interop.FILEWRITERLib.dll .
copy ..\..\..\MSASState\bin\release\MSASState.dll .
copy ..\..\..\MSASState\bin\release\MSASState.pdb .
copy ..\..\..\MSASState\bin\release\MemMapFile.dll .
 
copy ..\..\..\sqlite.dll .
copy ..\..\..\SQLiteClient.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .
copy ..\..\..\TaskScheduler.dll .
copy ..\..\..\AxInterop.WMPLib.dll .
copy ..\..\..\FireDTVKeyMap.XML .
copy ..\..\..\FireDTVKeyMap.XML.Schema .
