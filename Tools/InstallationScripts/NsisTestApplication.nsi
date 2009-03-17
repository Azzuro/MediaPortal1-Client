#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion


# DEFINES
!define svn_ROOT "..\.."
!define svn_MP "${svn_ROOT}\mediaportal"
!define svn_TVServer "${svn_ROOT}\TvEngine3\TVLibrary"
!define svn_DeployTool "${svn_ROOT}\Tools\MediaPortal.DeployTool"
!define svn_InstallScripts "${svn_ROOT}\Tools\InstallationScripts"




; TEST APP FOR MP STUFF

; The name of the installer
Name "test-helpmacros"

; The file to write
OutFile "${__FILE__}.exe"

; The default installation directory
InstallDir $DESKTOP\Example1

; Request application privileges for Windows Vista
RequestExecutionLevel user
ShowInstDetails show

;--------------------------------

Page components
Page instfiles

;--------------------------------

#!define INSTALL_LOG_FILE "$DESKTOP\install_$(^Name).log"

#!include "x64.nsh"
#!include Sections.nsh
#!include LogicLib.nsh
#!include Library.nsh
#!include FileFunc.nsh
#!include Memento.nsh


!define USE_READ_MP_DIRS ; defines if MediaPortal's special directories needs to be read from config
#!define USE_INSTALL_LOG  ; enables logging during installation and uninstallation
!include "${svn_InstallScripts}\include-CommonMPMacros.nsh"

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important
  
SectionEnd ; end the section

;--------------------------------

Section /o "MediaPortal Product information"

    DetailPrint ""
    DetailPrint "--------------------------------------"
    DetailPrint "- MediaPortal Installation"
    DetailPrint "--------------------------------------"

  ${If} ${MP023IsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    DetailPrint "X  MP 0.2.3.0 InstDir: $R0"
  ${Else}
    DetailPrint "!  MP 0.2.3.0 is not installed"
  ${EndIf}

  ${If} ${MPIsInstalled}
    !insertmacro MP_GET_INSTALL_DIR $R0
    DetailPrint "X  MediaPortal InstDir: $R0"
  ${Else}
    DetailPrint "!  MediaPortal is not installed"
  ${EndIf}

  ${If} ${TVServerIsInstalled}
    !insertmacro TVSERVER_GET_INSTALL_DIR $R0
    DetailPrint "X  TVServer InstDir: $R0"
  ${Else}
    DetailPrint "!  TVServer is not installed"
  ${EndIf}


  ${If} ${MSI_TVServerIsInstalled}
    DetailPrint "X  old MSI-based TVServer is installed"
  ${Else}
    DetailPrint "!  old MSI-based TVServer is not installed"
  ${EndIf}

  ${If} ${MSI_TVClientIsInstalled}
    DetailPrint "X  old MSI-based TVClient is installed"
  ${Else}
    DetailPrint "!  old MSI-based TVClient is not installed"
  ${EndIf}

SectionEnd

Section /o "Read MediaPortal directories"

  DetailPrint ""
  DetailPrint "--------------------------------------"
  DetailPrint "- Read MediaPortal directories"
  DetailPrint "--------------------------------------"

  ${IfNot} ${MP023IsInstalled}
  ${AndIfNot} ${MPIsInstalled}
    DetailPrint "no MPIsInstalled"
  ${else}
    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base
  ${EndIf}

  DetailPrint "Found the following Entries:"
  DetailPrint "    Base:  $MPdir.Base"
  DetailPrint "    Config:  $MPdir.Config"
  DetailPrint "    Plugins: $MPdir.Plugins"
  DetailPrint "    Log: $MPdir.Log"
  DetailPrint "    CustomInputDevice: $MPdir.CustomInputDevice"
  DetailPrint "    CustomInputDefault: $MPdir.CustomInputDefault"
  DetailPrint "    Skin: $MPdir.Skin"
  DetailPrint "    Language: $MPdir.Language"
  DetailPrint "    Database: $MPdir.Database"
  DetailPrint "    Thumbs: $MPdir.Thumbs"
  DetailPrint "    Weather: $MPdir.Weather"
  DetailPrint "    Cache: $MPdir.Cache"
  DetailPrint "    BurnerSupport: $MPdir.BurnerSupport"

SectionEnd

Section /o "OS, .Net and VCRedist Tests"

  !insertmacro MediaPortalOperatingSystemCheck 0
  !insertmacro MediaPortalAdminCheck 0
  !insertmacro MediaPortalVCRedistCheck 0
  !insertmacro MediaPortalNetFrameworkCheck 0

SectionEnd

Section /o "MediaPortal CleanUp: 1.0 for 1.0.1 Update"

  !include "${svn_MP}\Setup\update-1.0.1.nsh"

SectionEnd

Section /o "Rename MP dir"

    !insertmacro MP_GET_INSTALL_DIR $MPdir.Base
    ${ReadMediaPortalDirs} $MPdir.Base
    
  !insertmacro GET_BACKUP_POSTFIX $R0
  
  ${If} ${FileExists} "$MPdir.Base\*.*"
    ${LOG_TEXT} "INFO" "Installation dir already exists. ($MPdir.Base)"
    !insertmacro RenameDirectory "$MPdir.Base" "$MPdir.Base_$R0"    
  ${EndIf}

SectionEnd

Section /o "other tests"
  
  MessageBox MB_ICONINFORMATION|MB_YESNO "Do kill process test?" IDNO noKillProcess

  ${KILLPROCESS} "MPInstaller.exe"
  ${KILLPROCESS} "makensisw.exe"
  ${KILLPROCESS} "Input Service Configuration.exe"

  DetailPrint "KillProcess FINISHED"

  noKillProcess:
  
  
  
  MessageBox MB_ICONINFORMATION|MB_YESNO "ShutdownTVService?" IDNO noTVShutdown
  
  ${LOG_TEXT} "INFO" "Stopping TV Service..."
  nsExec::ExecToLog 'net stop TVservice'
  Pop $0
  DetailPrint "net stop TVservice result: $0"  
  
  DetailPrint "TVShutdown FINISHED"

  noTVShutdown:
  
  
  MessageBox MB_ICONINFORMATION|MB_YESNO "TVStart?" IDNO noTVStart
  
  ${LOG_TEXT} "INFO" "starting TV Service..."
  nsExec::ExecToLog 'net start TVservice'
  Pop $0
  DetailPrint "net start TVservice result: $0"  
  
  DetailPrint "TVStart FINISHED"

  noTVStart:


  
  

SectionEnd