#region Copyright (C) 2005-2010 Team MediaPortal
/*
// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#**********************************************************************************************************#
#
#   For building the installer on your own you need:
#       1. Latest NSIS version from http://nsis.sourceforge.net/Download
#
#**********************************************************************************************************#

#---------------------------------------------------------------------------
# SPECIAL BUILDS
#---------------------------------------------------------------------------
##### SVN_BUILD
# This build will be created by svn bot only.
# Creating such a build, will only include the changed and new files since latest stable release to the installer.

##### HEISE_BUILD
# Uncomment the following line to create a setup for "Heise Verlag" / ct' magazine  (without MPC-HC/Gabest Filters)
#!define HEISE_BUILD
# parameter for command line execution: /DHEISE_BUILD


#---------------------------------------------------------------------------
# DEVELOPMENT ENVIRONMENT
#---------------------------------------------------------------------------
# SKRIPT_NAME is needed to diff between the install scripts in imported headers
!define SKRIPT_NAME "MediaPortal"
# path definitions, all others are done in MediaPortalScriptInit
!define svn_ROOT "..\.."
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"
# common script init
!include "${svn_InstallScripts}\include\MediaPortalScriptInit.nsh"

# additional path definitions
!ifdef SVN_BUILD
  !define MEDIAPORTAL.BASE "C:\compile\compare_mp1_test"
!else
  !define MEDIAPORTAL.BASE "${svn_MP}\MediaPortal.Base"
!endif
!define MEDIAPORTAL.XBMCBIN "${svn_MP}\MediaPortal.Application\bin\${BUILD_TYPE}"


#---------------------------------------------------------------------------
# pre build commands
#---------------------------------------------------------------------------
!include "${svn_MP}\Setup\setup-preBuild.nsh"


#---------------------------------------------------------------------------
# DEFINES
#---------------------------------------------------------------------------
!define PRODUCT_NAME          "MediaPortal"
!define PRODUCT_PUBLISHER     "Team MediaPortal"
!define PRODUCT_WEB_SITE      "www.team-mediaportal.com"

!define REG_UNINSTALL         "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!define MEMENTO_REGISTRY_ROOT HKLM
!define MEMENTO_REGISTRY_KEY  "${REG_UNINSTALL}"
!define COMMON_APPDATA        "$APPDATA\Team MediaPortal\MediaPortal"
!define STARTMENU_GROUP       "$SMPROGRAMS\Team MediaPortal\MediaPortal"

; import version from shared file
!include "${svn_InstallScripts}\include\MediaPortalCurrentVersion.nsh"

SetCompressor /SOLID lzma


#---------------------------------------------------------------------------
# VARIABLES
#---------------------------------------------------------------------------
;Var StartMenuGroup  ; Holds the Startmenu\Programs folder
!ifndef HEISE_BUILD
Var noGabest
!endif
Var noDesktopSC
;Var noStartMenuSC
Var DeployMode
Var UpdateMode

Var PREVIOUS_INSTALLDIR
Var PREVIOUS_VERSION
Var PREVIOUS_VERSION_STATE
Var EXPRESS_UPDATE

Var frominstall

Var MPTray_Running

#---------------------------------------------------------------------------
# INCLUDE FILES
#---------------------------------------------------------------------------
!include MUI2.nsh
!include Sections.nsh
!include Library.nsh
!include FileFunc.nsh
!include Memento.nsh

!include "${svn_InstallScripts}\include\FileAssociation.nsh"
!include "${svn_InstallScripts}\include\LanguageMacros.nsh"
!include "${svn_InstallScripts}\include\LoggingMacros.nsh"
!include "${svn_InstallScripts}\include\MediaPortalDirectories.nsh"
!include "${svn_InstallScripts}\include\MediaPortalMacros.nsh"
!include "${svn_InstallScripts}\include\ProcessMacros.nsh"
!include "${svn_InstallScripts}\include\WinVerEx.nsh"

!ifndef SVN_BUILD
!include "${svn_InstallScripts}\pages\AddRemovePage.nsh"
!endif
!include "${svn_InstallScripts}\pages\UninstallModePage.nsh"


#---------------------------------------------------------------------------
# INSTALLER INTERFACE settings
#---------------------------------------------------------------------------
!define MUI_ABORTWARNING
!define MUI_ICON    "${svn_InstallScripts}\Resources\install.ico"
!define MUI_UNICON  "${svn_InstallScripts}\Resources\install.ico"

!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP              "${svn_InstallScripts}\Resources\header.bmp"
!if ${VER_BUILD} == 0       # it's an official release
  !define MUI_WELCOMEFINISHPAGE_BITMAP      "${svn_InstallScripts}\Resources\wizard-mp.bmp"
!else                       # it's a svn release
  !define MUI_WELCOMEFINISHPAGE_BITMAP      "${svn_InstallScripts}\Resources\wizard-mp-svn.bmp"
!endif
!define MUI_UNWELCOMEFINISHPAGE_BITMAP      "${svn_InstallScripts}\Resources\wizard-mp.bmp"
!define MUI_HEADERIMAGE_RIGHT

!define MUI_COMPONENTSPAGE_SMALLDESC
;!define MUI_STARTMENUPAGE_NODISABLE
;!define MUI_STARTMENUPAGE_DEFAULTFOLDER       "Team MediaPortal\MediaPortal"
;!define MUI_STARTMENUPAGE_REGISTRY_ROOT       HKLM
;!define MUI_STARTMENUPAGE_REGISTRY_KEY        "${REG_UNINSTALL}"
;!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME  StartMenuGroup
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN            "$MPdir.Base\Configuration.exe"
!define MUI_FINISHPAGE_RUN_TEXT       "Run MediaPortal Configuration"
!define MUI_FINISHPAGE_RUN_PARAMETERS "/avoidVersionCheck"
#!define MUI_FINISHPAGE_SHOWREADME $INSTDIR\readme.txt
#!define MUI_FINISHPAGE_SHOWREADME_TEXT "View Readme"
#!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_LINK          "Donate to MediaPortal"
!define MUI_FINISHPAGE_LINK_LOCATION "http://www.team-mediaportal.com/donate.html"

!define MUI_UNFINISHPAGE_NOAUTOCLOSE

#---------------------------------------------------------------------------
# INSTALLER INTERFACE
#---------------------------------------------------------------------------
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "${svn_MP}\Docs\MediaPortal License.rtf"
!insertmacro MUI_PAGE_LICENSE "${svn_MP}\Docs\BASS License.txt"

!ifndef SVN_BUILD
Page custom PageReinstallMode PageLeaveReinstallMode
!endif

!define MUI_PAGE_CUSTOMFUNCTION_PRE PageComponentsPre
!insertmacro MUI_PAGE_COMPONENTS

!define MUI_PAGE_CUSTOMFUNCTION_PRE PageDirectoryPre
!insertmacro MUI_PAGE_DIRECTORY

;!define MUI_PAGE_CUSTOMFUNCTION_PRE PageStartmenuPre
;!insertmacro MUI_PAGE_STARTMENU Application $StartMenuGroup

!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH


; UnInstaller Interface
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.WelcomePagePre
!insertmacro MUI_UNPAGE_WELCOME
UninstPage custom un.UninstallModePage un.UninstallModePageLeave
;!define MUI_PAGE_CUSTOMFUNCTION_PRE un.ConfirmPagePre
;!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_PRE un.FinishPagePre
!insertmacro MUI_UNPAGE_FINISH


#---------------------------------------------------------------------------
# INSTALLER LANGUAGES
#---------------------------------------------------------------------------
!insertmacro LANG_LOAD "English"


#---------------------------------------------------------------------------
# INSTALLER ATTRIBUTES
#---------------------------------------------------------------------------
Name          "${PRODUCT_NAME}"
BrandingText  "${PRODUCT_NAME} ${VERSION} by ${PRODUCT_PUBLISHER}"
!if ${VER_BUILD} == 0       # it's an official release
  OutFile "${svn_OUT}\package-mediaportal.exe"
!else                       # it's a svn release
  OutFile "${svn_OUT}\Setup-MediaPortal-svn-${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}.exe"
!endif
InstallDir ""
CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
VIProductVersion "${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductName       "${PRODUCT_NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} ProductVersion    "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyName       "${PRODUCT_PUBLISHER}"
VIAddVersionKey /LANG=${LANG_ENGLISH} CompanyWebsite    "${PRODUCT_WEB_SITE}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileVersion       "${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} FileDescription   "${PRODUCT_NAME} installation ${VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} LegalCopyright    "Copyright � 2005-2009 ${PRODUCT_PUBLISHER}"
ShowUninstDetails show


#---------------------------------------------------------------------------
# USEFUL MACROS
#---------------------------------------------------------------------------
!macro SectionList MacroName
  ${LOG_TEXT} "DEBUG" "MACRO SectionList ${MacroName}"
  ; This macro used to perform operation on multiple sections.
  ; List all of your components in following manner here.
!ifndef HEISE_BUILD
  !insertmacro "${MacroName}" "SecGabest"
!endif
  !insertmacro "${MacroName}" "SecPowerScheduler"
  !insertmacro "${MacroName}" "SecMpeInstaller"
!macroend

!macro ShutdownRunningMediaPortalApplications
  ${LOG_TEXT} "INFO" "Terminating processes..."

  ${KillProcess} "MediaPortal.exe"
  ${KillProcess} "configuration.exe"

  ${KillProcess} "MpeInstaller.exe"
  ${KillProcess} "MpeMaker.exe"

  ${KillProcess} "WatchDog.exe"
  ${KillProcess} "MusicShareWatcher.exe"

  ; MPTray
  ${KillProcess} "MPTray.exe"
  StrCpy $MPTray_Running $R0
  
  ; MovieThumbnailer
  ${KillProcess} "mtn.exe"
!macroend

!macro RenameInstallDirectory
  ${LOG_TEXT} "DEBUG" "MACRO::RenameInstallDirectory"

  !insertmacro GET_BACKUP_POSTFIX $R0

  !insertmacro RenameDirectory "$MPdir.Base" "$MPdir.Base_$R0"
  !insertmacro RenameDirectory "$MPdir.Config" "$MPdir.Config_$R0"

  ${If} ${FileExists} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml"
    ${LOG_TEXT} "INFO" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml already exists. It will be renamed."
    Rename "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml_$R0"
  ${EndIf}
!macroend

Function RunUninstaller

!ifndef SVN_BUILD
  ${VersionCompare} 1.0.2.22779 $PREVIOUS_VERSION $R0
  ; old (un)installers should be called silently
  ${If} $R0 == 2 ;previous is higher than 22780
    !insertmacro RunUninstaller "NoSilent"
  ${Else}
    !insertmacro RunUninstaller "silent"
  ${EndIf}
!endif

FunctionEnd


#---------------------------------------------------------------------------
# SECTIONS and REMOVEMACROS
#---------------------------------------------------------------------------
Section "-prepare" SecPrepare
  ${LOG_TEXT} "INFO" "Prepare installation..."
  ${ReadMediaPortalDirs} "$INSTDIR"

  !insertmacro ShutdownRunningMediaPortalApplications

  ${LOG_TEXT} "INFO" "Deleting SkinCache..."
  RMDir /r "$MPdir.Cache"

  # if it is an update include a file with last update/cleanup instructions
  ;future note: if silent, uninstall old, if not, do nothing.
  ${If} $DeployMode == 1
  ${AndIf} $UpdateMode == 1
    ${LOG_TEXT} "DEBUG" "SecPrepare: DeployMode = 1 | UpdateMode = 1"

    ${If} $PREVIOUS_VERSION == ""
      ${LOG_TEXT} "INFO" "It seems MP is not installed, no update procedure will be done"
    ${ElseIf} $R3 != 0
      ${LOG_TEXT} "INFO" "An SVN version ($0) of MP is installed. Update is not supported."
    ${Else}
      ${LOG_TEXT} "INFO" "MediaPortal $0 is installed."
    ${EndIf}

  ${ElseIf} $DeployMode == 1
    ${LOG_TEXT} "DEBUG" "SecPrepare: DeployMode = 1 | UpdateMode = 0"

    !insertmacro RenameInstallDirectory
    ${ReadMediaPortalDirs} "$INSTDIR"

  ${Else}
    ${LOG_TEXT} "DEBUG" "SecPrepare: DeployMode = 0 | UpdateMode = 0"

    ${LOG_TEXT} "INFO" "Uninstalling old version ..."
    ${If} ${Silent}
      !insertmacro RunUninstaller "silent"
    ${ElseIf} $EXPRESS_UPDATE != ""
      Call RunUninstaller
      BringToFront
    ${EndIf}

    Call BackupInstallDirectory

  ${EndIf}

SectionEnd

Section "MediaPortal core files (required)" SecCore
  SectionIn RO
  ${LOG_TEXT} "INFO" "Installing MediaPortal core files..."

  SetOverwrite on

  #CONFIG FILES ARE ALWAYS INSTALLED by SVN and FINAL releases, BECAUSE of the config dir location
  #MediaPortal Paths should not be overwritten
  !define EXCLUDED_CONFIG_FILES "\
    /x 'eHome Infrared Transceiver List XP.xml' \
    /x keymap.xml \
    /x MediaPortalDirs.xml \
    /x wikipedia.xml \
    /x mtn.c \
    "
	
  #Special build for Heise needs some files excluded.
  #At the moment this is only lame_enc.dll
  !define EXCLUDED_FILES_FOR_HEISE_BUILD "\
    /x lame_enc.dll \
	"

### AUTO-GENERATED   UNINSTALLATION CODE ###
  # Files which were diffed before including in installer
  # means all of them are in full installer, but only the changed and new ones are in svn installer 
  #We can not use the complete mediaportal.base dir recoursivly , because the plugins, thumbs need to be extracted to their special MPdir location
  # exluding only the folders does not work because /x plugins won't extract the \plugins AND musicplayer\plugins directory
  SetOutPath "$MPdir.Base"
  !ifdef HEISE_BUILD
	File /nonfatal /x .svn ${EXCLUDED_CONFIG_FILES} ${EXCLUDED_FILES_FOR_HEISE_BUILD} "${MEDIAPORTAL.BASE}\*"
  !else
	File /nonfatal /x .svn ${EXCLUDED_CONFIG_FILES}  "${MEDIAPORTAL.BASE}\*"
  !endif
  SetOutPath "$MPdir.Base\defaults"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\defaults\*"
  SetOutPath "$MPdir.Base\MovieThumbnailer"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\MovieThumbnailer\*"
  SetOutPath "$MPdir.Base\MusicPlayer"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\MusicPlayer\*"
  SetOutPath "$MPdir.Base\Profiles"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Profiles\*"
  SetOutPath "$MPdir.Base\Wizards"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\Wizards\*"

  # special MP directories
  SetOutPath "$MPdir.Language"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\language\*"
  SetOutPath "$MPdir.Plugins"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\plugins\*"
  SetOutPath "$MPdir.Skin"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\skin\*"
  SetOutPath "$MPdir.Thumbs"
  File /nonfatal /r /x .svn "${MEDIAPORTAL.BASE}\thumbs\*"
### AUTO-GENERATED   UNINSTALLATION CODE   END ###

  ; create empty folders
  SetOutPath "$MPdir.Config"
  CreateDirectory "$MPdir.Config"
  SetOutPath "$MPdir.Database"
  CreateDirectory "$MPdir.Database"
  SetOutPath "$MPdir.Log"
  CreateDirectory "$MPdir.Log"

  ; Config Files
  SetOutPath "$MPdir.Config"
  File /nonfatal "${MEDIAPORTAL.BASE}\eHome Infrared Transceiver List XP.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\keymap.xml"
  File /nonfatal "${MEDIAPORTAL.BASE}\wikipedia.xml"

  File /nonfatal "${MEDIAPORTAL.BASE}\log4net.config"
  
  SetOutPath "$MPdir.Config\scripts\MovieInfo"
  File /nonfatal "${MEDIAPORTAL.BASE}\scripts\MovieInfo\IMDB.csscript"


  SetOutPath "$MPdir.Base"
  File "${svn_MP}\MediaPortal.Base\MediaPortalDirs.xml"
  File "${svn_MP}\MediaPortal.Base\BuiltInPlugins.xml"
  ; MediaPortal.exe
  File "${svn_MP}\MediaPortal.Application\bin\${BUILD_TYPE}\MediaPortal.exe"
  File "${svn_MP}\MediaPortal.Application\bin\${BUILD_TYPE}\MediaPortal.exe.config"
  ; Configuration
  File "${svn_MP}\Configuration\bin\${BUILD_TYPE}\Configuration.exe"
  File "${svn_MP}\Configuration\bin\${BUILD_TYPE}\Configuration.exe.config"
  ; Core
  File "${svn_MP}\core\bin\${BUILD_TYPE}\Core.dll"
  File "${svn_Common_MP_TVE3}\DirectShowLib\bin\${BUILD_TYPE}\DirectShowLib.dll"
  File "${svn_MP}\core.cpp\fontEngine\bin\${BUILD_TYPE}\fontengine.dll"
  File "${svn_MP}\core.cpp\DirectShowHelper\bin\${BUILD_TYPE}\dshowhelper.dll"
  File "${svn_MP}\core.cpp\Win7RefreshRateHelper\bin\${BUILD_TYPE}\Win7RefreshRateHelper.dll"
  File "${svn_MP}\core.cpp\DxUtil\bin\${BUILD_TYPE}\dxutil.dll"
  File "${svn_MP}\core.cpp\mpc-hc_subs\bin\${BUILD_TYPE}\mpcSubs.dll"
  File "${svn_DirectShowFilters}\DXErr9\bin\${BUILD_TYPE}\Dxerr9.dll"
  File "${svn_MP}\MiniDisplayLibrary\bin\${BUILD_TYPE}\MiniDisplayLibrary.dll"
  ; Utils
  File "${svn_MP}\Utils\bin\${BUILD_TYPE}\Utils.dll"
  ; Common Utils
  File "${svn_Common_MP_TVE3}\Common.Utils\bin\${BUILD_TYPE}\Common.Utils.dll"
  ; Support
  File "${svn_MP}\MediaPortal.Support\bin\${BUILD_TYPE}\MediaPortal.Support.dll"
  ; Databases
  File "${svn_MP}\databases\bin\${BUILD_TYPE}\Databases.dll"
  ; MusicShareWatcher
  File "${svn_MP}\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\${BUILD_TYPE}\MusicShareWatcher.exe"
  File "${svn_MP}\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\${BUILD_TYPE}\MusicShareWatcherHelper.dll"
  ; WatchDog
  File "${svn_MP}\WatchDog\bin\${BUILD_TYPE}\WatchDog.exe"
  File "${svn_MP}\WatchDog\bin\${BUILD_TYPE}\DaggerLib.dll"
  File "${svn_MP}\WatchDog\bin\${BUILD_TYPE}\DaggerLib.DSGraphEdit.dll"
  File "${svn_MP}\WatchDog\bin\${BUILD_TYPE}\DirectShowLib-2005.dll"
  File "${svn_MP}\WatchDog\bin\${BUILD_TYPE}\MediaFoundation.dll"
  ; MP Tray
  File "${svn_MP}\MPTray\bin\${BUILD_TYPE}\MPTray.exe"
  ; Plugins
  File "${svn_MP}\RemotePlugins\bin\${BUILD_TYPE}\RemotePlugins.dll"
  File "${svn_MP}\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\${BUILD_TYPE}\HcwHelper.exe"
  File "${svn_MP}\RemotePlugins\Remotes\X10Remote\Interop.X10.dll"
  SetOutPath "$MPdir.Plugins\ExternalPlayers"
  File "${svn_MP}\ExternalPlayers\bin\${BUILD_TYPE}\ExternalPlayers.dll"
  SetOutPath "$MPdir.Plugins\process"
  File "${svn_MP}\ProcessPlugins\bin\${BUILD_TYPE}\ProcessPlugins.dll"
  SetOutPath "$MPdir.Plugins\subtitle"
  File "${svn_MP}\SubtitlePlugins\bin\${BUILD_TYPE}\SubtitlePlugins.dll"
  SetOutPath "$MPdir.Plugins\Windows"
  File "${svn_MP}\Dialogs\bin\${BUILD_TYPE}\Dialogs.dll"
  File "${svn_MP}\WindowPlugins\bin\${BUILD_TYPE}\WindowPlugins.dll"
  ; Doc
  SetOutPath "$MPdir.Base\Docs"
  File "${svn_MP}\Docs\BASS License.txt"
  File "${svn_MP}\Docs\MediaPortal License.rtf"

  #---------------------------------------------------------------------------
  # FILTER REGISTRATION
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  SetOutPath "$MPdir.Base"
  ;filter used for SVCD and VCD playback
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\bin\Release\cdxareader.ax"                             "$MPdir.Base\cdxareader.ax"       "$MPdir.Base"
  ; used for channels with two mono languages in one stereo streams
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\MPAudioswitcher\bin\${BUILD_TYPE}\MPAudioSwitcher.ax"  "$MPdir.Base\MPAudioSwitcher.ax"  "$MPdir.Base"
  ; used for digital tv
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\TsReader\bin\${BUILD_TYPE}\TsReader.ax"                "$MPdir.Base\TsReader.ax"         "$MPdir.Base"
  WriteRegStr HKCR "Media Type\Extensions\.ts"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  WriteRegStr HKCR "Media Type\Extensions\.tp"        "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  WriteRegStr HKCR "Media Type\Extensions\.tsbuffer"  "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"
  WriteRegStr HKCR "Media Type\Extensions\.rtsp"      "Source Filter" "{b9559486-e1bb-45d3-a2a2-9a7afe49b23f}"

SectionEnd
!macro Remove_${SecCore}
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal core files..."
  
  !insertmacro ShutdownRunningMediaPortalApplications

  #---------------------------------------------------------------------------
  # FILTER UNREGISTRATION     for TVClient
  #               for more information see:           http://nsis.sourceforge.net/Docs/AppendixB.html
  #---------------------------------------------------------------------------
  ;filter used for SVCD and VCD playback
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\cdxareader.ax"
  ##### MAYBE used by VideoEditor
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\CLDump.ax"
  ;filter for analog tv and videoeditor
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\PDMpgMux.ax"
  ; used for shoutcast
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\shoutcastsource.ax"
  ; used for channels with two mono languages in one stereo streams
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MPAudioSwitcher.ax"
  ; used for digital tv
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\TsReader.ax"


### AUTO-GENERATED   UNINSTALLATION CODE ###
  !include "${svn_MP}\Setup\uninstall.nsh"
### AUTO-GENERATED   UNINSTALLATION CODE   END ###


  ; Remove the Folders
  RMDir /r "$MPdir.Cache"

  ; Config Files
  Delete "$MPdir.Config\CaptureCardDefinitions.xml"
  Delete "$MPdir.Config\eHome Infrared Transceiver List XP.xml"
  Delete "$MPdir.Config\keymap.xml"
  Delete "$MPdir.Config\wikipedia.xml"

  Delete "$MPdir.Config\Installer\cleanup.xml"
  RMDir "$MPdir.Config\Installer"
  Delete "$MPdir.Config\scripts\MovieInfo\IMDB.csscript"
  RMDir "$MPdir.Config\scripts\MovieInfo"
  RMDir "$MPdir.Config\scripts"


  ; MediaPortal.exe
  Delete "$MPdir.Base\MediaPortal.exe"
  Delete "$MPdir.Base\MediaPortal.exe.config"
  ; Configuration
  Delete "$MPdir.Base\Configuration.exe"
  Delete "$MPdir.Base\Configuration.exe.config"
  ; Core
  Delete "$MPdir.Base\Core.dll"
  Delete "$MPdir.Base\DirectShowLib.dll"
  Delete "$MPdir.Base\fontengine.dll"
  Delete "$MPdir.Base\dshowhelper.dll"
  Delete "$MPdir.Base\Win7RefreshRateHelper.dll"
  Delete "$MPdir.Base\dxutil.dll"
  Delete "$MPdir.Base\Dxerr9.dll"
  Delete "$MPdir.Base\mpcSubs.dll"
  Delete "$MPdir.Base\MiniDisplayLibrary.dll"
  ; Utils
  Delete "$MPdir.Base\Utils.dll"
  ; Common Utils
  Delete "$MPdir.Base\Common.Utils.dll"
  ; Support
  Delete "$MPdir.Base\MediaPortal.Support.dll"
  ; Databases
  Delete "$MPdir.Base\Databases.dll"
  ; MusicShareWatcher
  Delete "$MPdir.Base\MusicShareWatcher.exe"
  Delete "$MPdir.Base\MusicShareWatcherHelper.dll"
  ; WatchDog
  Delete "$MPdir.Base\WatchDog.exe"
  Delete "$MPdir.Base\DaggerLib.dll"
  Delete "$MPdir.Base\DaggerLib.DSGraphEdit.dll"
  Delete "$MPdir.Base\DirectShowLib-2005.dll"
  Delete "$MPdir.Base\MediaFoundation.dll"
  ; MP Tray
  Delete "$MPdir.Base\MPTray.exe"
  ; Plugins
  Delete "$MPdir.Base\RemotePlugins.dll"
  Delete "$MPdir.Base\HcwHelper.exe"
  Delete "$MPdir.Base\Interop.X10.dll"
  Delete "$MPdir.Plugins\ExternalPlayers\ExternalPlayers.dll"
  RMDir "$MPdir.Plugins\ExternalPlayers"
  Delete "$MPdir.Plugins\process\ProcessPlugins.dll"
  RMDir "$MPdir.Plugins\process"
  Delete "$MPdir.Plugins\subtitle\SubtitlePlugins.dll"
  RMDir "$MPdir.Plugins\subtitle"
  Delete "$MPdir.Plugins\Windows\Dialogs.dll"
  Delete "$MPdir.Plugins\Windows\WindowPlugins.dll"
  RMDir "$MPdir.Plugins\Windows"
  RMDir "$MPdir.Plugins"
  ; Doc
  Delete "$MPdir.Base\Docs\BASS License.txt"
  Delete "$MPdir.Base\Docs\MediaPortal License.rtf"
  RMDir "$MPdir.Base\Docs"
  ; Wizards
  RMDir /r "$MPdir.Base\Wizards"
!macroend

!ifndef HEISE_BUILD
Section "-MPC-HC audio/video decoders" SecGabest
  ${LOG_TEXT} "INFO" "Installing MPC-HC audio/video decoders..."

  SetOutPath "$MPdir.Base"
  ; register the default video and audio codecs from the MediaPlayer Classic Home Cinema Project
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\bin\Release\MpaDecFilter.ax"   "$MPdir.Base\MpaDecFilter.ax"   "$MPdir.Base"
  !insertmacro InstallLib REGDLL NOTSHARED NOREBOOT_NOTPROTECTED "${svn_DirectShowFilters}\bin\Release\Mpeg2DecFilter.ax" "$MPdir.Base\Mpeg2DecFilter.ax" "$MPdir.Base"
  
  ; adjust the merit of this directshow filter
  SetOutPath "$MPdir.Base"
  File "${svn_ROOT}\Tools\Script & Batch tools\SetMerit\bin\Release\SetMerit.exe"

  ${LOG_TEXT} "INFO" "set merit for MPA"
  nsExec::ExecToLog '"$MPdir.Base\SetMerit.exe" {3D446B6F-71DE-4437-BE15-8CE47174340F} 00600000'
  ${LOG_TEXT} "INFO" "set merit for MPV"
  nsExec::ExecToLog '"$MPdir.Base\SetMerit.exe" {39F498AF-1A09-4275-B193-673B0BA3D478} 00600000'
SectionEnd
!macro Remove_${SecGabest}
  ${LOG_TEXT} "INFO" "Uninstalling MPC-HC audio/video decoders..."

  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\MpaDecFilter.ax"
  !insertmacro UnInstallLib REGDLL NOTSHARED REBOOT_NOTPROTECTED "$MPdir.Base\Mpeg2DecFilter.ax"

  ; remove the tool to adjust the merit
  Delete "$MPdir.Base\SetMerit.exe"
!macroend
!endif

Section "-Powerscheduler Client plugin" SecPowerScheduler
  ${LOG_TEXT} "INFO" "Installing Powerscheduler client plugin..."

  SetOutPath "$MPdir.Base"
  File "${svn_Common_MP_TVE3}\PowerScheduler.Interfaces\bin\${BUILD_TYPE}\PowerScheduler.Interfaces.dll"

  SetOutPath "$MPdir.Plugins\Process"
  File "${svn_MP}\PowerSchedulerClientPlugin\bin\${BUILD_TYPE}\PowerSchedulerClientPlugin.dll"
SectionEnd
!macro Remove_${SecPowerScheduler}
  ${LOG_TEXT} "INFO" "Uninstalling Powerscheduler client plugin..."

  Delete "$MPdir.Base\PowerScheduler.Interfaces.dll"

  Delete "$MPdir.Plugins\Process\PowerSchedulerClientPlugin.dll"
!macroend

Section "-MediaPortal Extension Installer" SecMpeInstaller
  ${LOG_TEXT} "INFO" "MediaPortal Extension Installer..."

  ; install files
  SetOutPath "$MPdir.Base"
  File "${svn_MP}\MPE\MpeCore\bin\${BUILD_TYPE}\MpeCore.dll"
  File "${svn_MP}\MPE\MpeInstaller\bin\${BUILD_TYPE}\MpeInstaller.exe"
  File "${svn_MP}\MPE\MpeMaker\bin\${BUILD_TYPE}\MpeMaker.exe"

  ; create startmenu shortcuts
  ${If} $noDesktopSC != 1
    CreateShortCut "$DESKTOP\MediaPortal Extension Installer.lnk" "$MPdir.Base\MpeInstaller.exe"  ""  "$MPdir.Base\MpeInstaller.exe"  0 "" "" "MediaPortal Extension Installer"
  ${EndIf}
  CreateDirectory "${STARTMENU_GROUP}"
  CreateShortCut "${STARTMENU_GROUP}\MediaPortal Extension Installer.lnk" "$MPdir.Base\MpeInstaller.exe"  ""  "$MPdir.Base\MpeInstaller.exe"  0 "" "" "MediaPortal Extension Installer"
  CreateShortCut "${STARTMENU_GROUP}\MediaPortal Extension Maker.lnk"     "$MPdir.Base\MpeMaker.exe"      ""  "$MPdir.Base\MpeMaker.exe"      0 "" "" "MediaPortal Extension Maker"

  ; associate file extensions
  ${RegisterExtension} "$MPdir.Base\MpeInstaller.exe" ".mpe1" "MediaPortal extension"
  ${RegisterExtension} "$MPdir.Base\MpeMaker.exe"     ".xmp2" "MediaPortal extension project"

  ${RefreshShellIcons}
SectionEnd
!macro Remove_${SecMpeInstaller}
  ${LOG_TEXT} "INFO" "Uninstalling MediaPortal Extension Installer..."

  ; remove files
  Delete "$MPdir.Base\MpeCore.dll"
  Delete "$MPdir.Base\MpeInstaller.exe"
  Delete "$MPdir.Base\MpeMaker.exe"

  ; remove startmenu shortcuts
  Delete "$DESKTOP\MediaPortal Extension Installer.lnk"
  Delete "${STARTMENU_GROUP}\MediaPortal Extension Installer.lnk"
  Delete "${STARTMENU_GROUP}\MediaPortal Extension Maker.lnk"

  ; unassociate file extensions
  ${UnRegisterExtension} ".mpe1" "MediaPortal extension"
  ${UnRegisterExtension} ".xmp2"  "MediaPortal extension project"

  ${RefreshShellIcons}
!macroend

SectionGroup /e "Backup" SecBackup
  ${MementoUnselectedSection} "Installation directory" SecBackupInstDir
  ${MementoSectionEnd}
  ${MementoSection} "Configuration directory" SecBackupConfig
  ${MementoSectionEnd}
  ${MementoUnselectedSection} "Thumbs directory" SecBackupThumbs
  ${MementoSectionEnd}
SectionGroupEnd

${MementoSectionDone}

#---------------------------------------------------------------------------
# This Section is executed after the Main secxtion has finished and writes Uninstall information into the registry
Section -Post
  ${LOG_TEXT} "INFO" "Doing post installation stuff..."

  ; Removes unselected components
  !insertmacro SectionList "FinishSection"

  ; writes component status to registry
  ${MementoSectionSave}

  SetOverwrite on
  SetOutPath "$MPdir.Base"

  ; cleaning/renaming log dir - requested by chemelli
  RMDir /r "$MPdir.Log\OldLogs"
  CreateDirectory "$MPdir.Log\OldLogs"
  CopyFiles /SILENT /FILESONLY "$MPdir.Log\*" "$MPdir.Log\OldLogs"
  Delete "$MPdir.Log\*"

  ; removing old externaldisplay files - requested by chemelli
  ${LOG_TEXT} "INFO" "Removing obsolete (External/Mini/Cybr)Display files"
  Delete "$MPdir.Plugins\process\ExternalDisplayPlugin.dll"
  Delete "$MPdir.Plugins\process\MiniDisplayPlugin.dll"
  Delete "$MPdir.Plugins\process\CybrDisplayPlugin.dll"
  Delete "$MPdir.Plugins\windows\CybrDisplayPlugin.dll"

  ; BASS 2.3  to   2.4   Update - requested by hwahrmann (2009-01-26)
  ${LOG_TEXT} "INFO" "Removing obsolete BASS 2.3 files"
  Delete "$MPdir.Base\MusicPlayer\plugins\audio decoders\bass_wv.dll"

  ; removing old shortcut
  ${LOG_TEXT} "INFO" "Removing obsolete startmenu shortcuts"
  Delete "${STARTMENU_GROUP}\MediaPortal Logs Collector.lnk"
  
  ; create desktop shortcuts
  ${If} $noDesktopSC != 1
    CreateShortCut "$DESKTOP\MediaPortal.lnk"               "$MPdir.Base\MediaPortal.exe"      "" "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
    CreateShortCut "$DESKTOP\MediaPortal Configuration.lnk" "$MPdir.Base\Configuration.exe"    "" "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
  ${EndIf}

  ; create startmenu shortcuts
  ;${If} $noStartMenuSC != 1
      ; We need to create the StartMenu Dir. Otherwise the CreateShortCut fails
      CreateDirectory "${STARTMENU_GROUP}"
      CreateShortCut "${STARTMENU_GROUP}\MediaPortal.lnk"                            "$MPdir.Base\MediaPortal.exe"   ""      "$MPdir.Base\MediaPortal.exe"   0 "" "" "MediaPortal"
      CreateShortCut "${STARTMENU_GROUP}\MediaPortal Configuration.lnk"              "$MPdir.Base\Configuration.exe" ""      "$MPdir.Base\Configuration.exe" 0 "" "" "MediaPortal Configuration"
      CreateShortCut "${STARTMENU_GROUP}\MediaPortal Debug-Mode.lnk"                 "$MPdir.Base\WatchDog.exe"      ""      "$MPdir.Base\WatchDog.exe"   0 "" "" "MediaPortal Debug-Mode"
      CreateShortCut "${STARTMENU_GROUP}\uninstall MediaPortal.lnk"                  "$MPdir.Base\uninstall-mp.exe"
      CreateShortCut "${STARTMENU_GROUP}\User Files.lnk"                             "$MPdir.Config"                 ""      "$MPdir.Config"                 0 "" "" "Browse you config files, databases, thumbs, logs, ..."

      WriteINIStr "${STARTMENU_GROUP}\Quick Setup Guide.url"  "InternetShortcut" "URL" "http://wiki.team-mediaportal.com/TeamMediaPortal/MP1QuickSetupGuide"
      WriteINIStr "${STARTMENU_GROUP}\Help.url"               "InternetShortcut" "URL" "http://wiki.team-mediaportal.com/"
      WriteINIStr "${STARTMENU_GROUP}\web site.url"           "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  ;${EndIf}

  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMajor"    "${VER_MAJOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionMinor"    "${VER_MINOR}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionRevision" "${VER_REVISION}"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" "VersionBuild"    "${VER_BUILD}"

  ; Write Uninstall Information
  WriteRegStr HKLM "${REG_UNINSTALL}" InstallPath        "$MPdir.Base"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayName        "${PRODUCT_NAME}"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayVersion     "${VERSION}"
  WriteRegStr HKLM "${REG_UNINSTALL}" Publisher          "${PRODUCT_PUBLISHER}"
  WriteRegStr HKLM "${REG_UNINSTALL}" URLInfoAbout       "${PRODUCT_WEB_SITE}"
  WriteRegStr HKLM "${REG_UNINSTALL}" DisplayIcon        "$MPdir.Base\MediaPortal.exe,0"
  WriteRegStr HKLM "${REG_UNINSTALL}" UninstallString    "$MPdir.Base\uninstall-mp.exe"
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoModify 1
  WriteRegDWORD HKLM "${REG_UNINSTALL}" NoRepair 1

  WriteUninstaller "$MPdir.Base\uninstall-mp.exe"

  ; set rights to programmdata directory and reg keys
  !insertmacro SetRights

  ; start configuration.exe for MediaPortal.xml upgrading
  StrCpy $R0 "--DeployMode"
  ${LOG_TEXT} "INFO" "Starting Configuration.exe $R0..."
  ExecWait '"$INSTDIR\Configuration.exe" $R0'
	  
  ; start MpTray if it was running before
  ${If} $MPTray_Running == 0
    ${LOG_TEXT} "INFO" "Starting MPTray..."
    Exec '"$MPdir.Base\MPTray.exe"'
  ${EndIf}
SectionEnd

#---------------------------------------------------------------------------
# This section is called on uninstall and removes all components
Section Uninstall
  ${LOG_TEXT} "DEBUG" "SECTION Uninstall"
  ;First removes all optional components
  !insertmacro SectionList "RemoveSection"
  ;now also remove core component
  !insertmacro Remove_${SecCore}

  ; remove registry key
  DeleteRegValue HKLM "${REG_UNINSTALL}" "UninstallString"

  ; remove Start Menu shortcuts
  ; $StartMenuGroup (default): "Team MediaPortal\MediaPortal"
  Delete "${STARTMENU_GROUP}\MediaPortal.lnk"
  Delete "${STARTMENU_GROUP}\MediaPortal Configuration.lnk"
  Delete "${STARTMENU_GROUP}\MediaPortal Debug-Mode.lnk"
  Delete "${STARTMENU_GROUP}\MediaPortal Log-Files.lnk"
  Delete "${STARTMENU_GROUP}\MediaPortal TestTool.lnk"
  Delete "${STARTMENU_GROUP}\MediaPortal Logs Collector.lnk"
  Delete "${STARTMENU_GROUP}\uninstall MediaPortal.lnk"
  Delete "${STARTMENU_GROUP}\User Files.lnk"

  Delete "${STARTMENU_GROUP}\Quick Setup Guide.url"
  Delete "${STARTMENU_GROUP}\Help.url"
  Delete "${STARTMENU_GROUP}\web site.url"
  RMDir "${STARTMENU_GROUP}"
  RMDir "$SMPROGRAMS\Team MediaPortal"

  ; remove Desktop shortcuts
  Delete "$DESKTOP\MediaPortal.lnk"
  Delete "$DESKTOP\MediaPortal Configuration.lnk"

  ; remove last files and instdir
  Delete "$MPdir.Base\uninstall-mp.exe"
  RMDir "$MPdir.Base"


  ${If} $UnInstallMode == 1

    ${LOG_TEXT} "INFO" "Removing User Settings"
    DeleteRegKey HKLM "${REG_UNINSTALL}"
    RMDir /r "$MPdir.Config"
    RMDir /r "$MPdir.Database"
    RMDir /r "$MPdir.Language"
    RMDir /r "$MPdir.Plugins"
    RMDir /r "$MPdir.Skin"
    RMDir /r "$MPdir.Base"

    RMDir /r "$LOCALAPPDATA\VirtualStore\ProgramData\Team MediaPortal\MediaPortal"
    RMDir /r "$LOCALAPPDATA\VirtualStore\Program Files\Team MediaPortal\MediaPortal"
    RMDir "$LOCALAPPDATA\VirtualStore\ProgramData\Team MediaPortal"
    RMDir "$LOCALAPPDATA\VirtualStore\Program Files\Team MediaPortal"

  ${ElseIf} $UnInstallMode == 2

    !insertmacro CompleteMediaPortalCleanup

  ${EndIf}


  ${If} $frominstall == 1
    Quit
  ${EndIf}
SectionEnd


#---------------------------------------------------------------------------
# SOME MACROS AND FUNCTIONS
#---------------------------------------------------------------------------
Function ReadPreviousSettings
  ; read and analyze previous version
  !insertmacro ReadPreviousVersion

  ; read previous used directories
  !insertmacro MP_GET_INSTALL_DIR $PREVIOUS_INSTALLDIR
FunctionEnd

Function LoadPreviousSettings
  ; reset INSTDIR
  ${If} "$PREVIOUS_INSTALLDIR" != ""
    StrCpy $INSTDIR "$PREVIOUS_INSTALLDIR"
  ${ElseIf} "$INSTDIR" == ""
    StrCpy $INSTDIR "$PROGRAMFILES\Team MediaPortal\MediaPortal"
  ${EndIf}

  ; reset previous component selection from registry
  ${MementoSectionRestore}

  !insertmacro UpdateBackupSections

!ifndef HEISE_BUILD
  ; update the component status -> commandline parameters have higher priority than registry values
  ${If} $noGabest = 1
    !insertmacro UnselectSection ${SecGabest}
  ${EndIf}
!endif

  ; update component selection, according to possible selections
  ;Call .onSelChange

FunctionEnd


#---------------------------------------------------------------------------
# INSTALLER CALLBACKS
#---------------------------------------------------------------------------
Function .onInit
  ${LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInit"

  StrCpy $MPTray_Running 0

  #### check and parse cmdline parameter
  ; set default values for parameters ........
!ifndef HEISE_BUILD
  StrCpy $noGabest 0
!endif
  StrCpy $noDesktopSC 0
  ;StrCpy $noStartMenuSC 0
  StrCpy $DeployMode 0
  StrCpy $UpdateMode 0

  ${InitCommandlineParameter}
!ifndef HEISE_BUILD
  ${ReadCommandlineParameter} "noGabest"
!endif
  ${ReadCommandlineParameter} "noDesktopSC"
  ;${ReadCommandlineParameter} "noStartMenuSC"
  ${ReadCommandlineParameter} "DeployMode"
  ${ReadCommandlineParameter} "UpdateMode"
  #### END of check and parse cmdline parameter


  ${If} $DeployMode != 1
    !insertmacro DoPreInstallChecks
  ${EndIf}


  Call ReadPreviousSettings
  Call LoadPreviousSettings


  SetShellVarContext all
FunctionEnd

Function .onInstFailed
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInstFailed"
  ${LOG_CLOSE}
FunctionEnd

Function .onInstSuccess
  ${LOG_TEXT} "DEBUG" "FUNCTION .onInstSuccess"
  ${LOG_CLOSE}
FunctionEnd

;======================================

Function PageComponentsPre
  ; skip page if previous settings are used for update
  ${If} $EXPRESS_UPDATE == 1
    Abort
  ${EndIf}
FunctionEnd

Function PageDirectoryPre
  ; skip page if previous settings are used for update
  ${If} $EXPRESS_UPDATE == 1
    Abort
  ${EndIf}
FunctionEnd


#---------------------------------------------------------------------------
# UNINSTALLER CALLBACKS
#---------------------------------------------------------------------------
Function un.onInit
  ${un.LOG_OPEN}
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onInit"


  #### check and parse cmdline parameter
  ; set default values for parameters ........
  StrCpy $UnInstallMode 0

  ${un.InitCommandlineParameter}
  ${un.ReadCommandlineParameter} "frominstall"

  ; check for special parameter and set the their variables
  ClearErrors
  ${un.GetOptions} $R0 "/RemoveAll" $R1
  IfErrors +2
  StrCpy $UnInstallMode 1
  #### END of check and parse cmdline parameter


  ReadRegStr $INSTDIR HKLM "${REG_UNINSTALL}" "InstallPath"
  ${un.ReadMediaPortalDirs} "$INSTDIR"
  ;!insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuGroup

  SetShellVarContext all
FunctionEnd

Function un.onUninstFailed
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onUninstFailed"
  ${un.LOG_CLOSE}
FunctionEnd

Function un.onUninstSuccess
  ${LOG_TEXT} "DEBUG" "FUNCTION un.onUninstSuccess"

  ; write a reboot flag, if reboot is needed, so the installer won't continue until reboot is done
  ${If} ${RebootFlag}
    ${LOG_TEXT} "INFO" "!!! Some files were not able to uninstall. To finish uninstallation completly a REBOOT is needed."
    FileOpen $0 "$MPdir.Base\rebootflag" w
    Delete "$MPdir.Base\rebootflag" ; this will not be deleted until the reboot because it is currently opened
    RMDir "$MPdir.Base"
    FileClose $0
  ${EndIf}

  ${un.LOG_CLOSE}
FunctionEnd

;======================================

Function un.WelcomePagePre

  ${If} $frominstall == 1
    Abort
  ${EndIf}

FunctionEnd

/*
Function un.ConfirmPagePre

  ${If} $frominstall == 1
    Abort
  ${EndIf}

FunctionEnd
*/
Function un.FinishPagePre

  ${If} $frominstall == 1
    SetRebootFlag false
    Abort
  ${EndIf}

FunctionEnd


Function BackupInstallDirectory
  ${LOG_TEXT} "DEBUG" "MACRO::BackupInstallDirectory"

  !insertmacro GET_BACKUP_POSTFIX $R0

  ${If} ${SectionIsSelected} ${SecBackupInstDir}
    ${LOG_TEXT} "INFO" "Creating backup of installation dir, this might take some minutes."
    CreateDirectory "$MPdir.Base_$R0"
    CopyFiles /SILENT "$MPdir.Base\*.*" "$MPdir.Base_$R0"
  ${EndIf}

  ${If} ${SectionIsSelected} ${SecBackupConfig}
    !insertmacro BackupConfigDir
  ${EndIf}

  ${If} ${SectionIsSelected} ${SecBackupThumbs}
    !insertmacro BackupThumbsDir
  ${EndIf}

FunctionEnd


#---------------------------------------------------------------------------
# SECTION DESCRIPTIONS
#---------------------------------------------------------------------------
!ifndef HEISE_BUILD
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecGabest}  $(DESC_SecGabest)
!insertmacro MUI_FUNCTION_DESCRIPTION_END
!endif
