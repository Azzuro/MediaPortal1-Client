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


!define MIN_FRA_MAJOR "2"
!define MIN_FRA_MINOR "0"
!define MIN_FRA_BUILD "*"


# INCLUDE
!include "include-DotNetFramework.nsh"


# BUILD sources  , comment to disable the preBuild
!define BUILD_MediaPortal
!define BUILD_TVServer
!define BUILD_DeployTool
!define BUILD_Installer

!system '"$%ProgramFiles%\TortoiseSVN\bin\SubWCRev.exe" "${svn_ROOT}" RevisionInfoTemplate.nsh version.txt' = 0
;!define SVN_REVISION "$WCREV$"    ; that's the string in version txt, after SubWCRev has been launched
!include "version.txt"

!ifdef BUILD_MediaPortal
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release "${svn_MP}\DeployVersionSVN\DeployVersionSVN.sln"' = 0

!system '"${svn_MP}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_MP}"' = 0
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${svn_MP}\MediaPortal.sln"' = 0
!system '"${svn_MP}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_MP}"  /revert' = 0
!endif

!ifdef BUILD_TVServer
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild Release "${svn_TVServer}\DeployVersionSVN\DeployVersionSVN.sln"' = 0

!system '"${svn_MP}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_TVServer}"' = 0
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${svn_TVServer}\TvLibrary.sln"' = 0
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" "${svn_TVServer}\TvPlugin\TvPlugin.sln"' = 0
!system '"${svn_MP}\DeployVersionSVN\DeployVersionSVN\bin\Release\DeployVersionSVN.exe" /svn="${svn_TVServer}"  /revert' = 0
!endif

!ifdef BUILD_DeployTool
!system '"$%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release" "${svn_DeployTool}\MediaPortal.DeployTool.sln"' = 0
!endif

!ifdef BUILD_Installer
!system '"${NSISDIR}\makensis.exe" "${svn_MP}\Setup\setup.nsi"' = 0
!system '"${NSISDIR}\makensis.exe" "${svn_TVServer}\Setup\setup.nsi"' = 0
!endif


# UNPACKER script
Name "MediaPortal Unpacker"
;SetCompressor /SOLID lzma
Icon "${svn_DeployTool}\Install.ico"

OutFile "MediaPortal Setup 1.0preRC2 (SVN${SVN_REVISION}).exe"
InstallDir "$TEMP\MediaPortal Installation"

Page directory
Page instfiles

CRCCheck on
XPStyle on
RequestExecutionLevel admin
ShowInstDetails show
AutoCloseWindow true

Section
  SetOutPath $INSTDIR
  File /r /x .svn /x *.pdb /x *.vshost.exe "${svn_DeployTool}\bin\Release\*"

  SetOutPath $INSTDIR\deploy
  File "${svn_MP}\Setup\Release\package-mediaportal.exe"
  File "${svn_TVServer}\Setup\Release\package-tvengine.exe"

SectionEnd

Function .onInit
  Call AbortIfBadFramework
FunctionEnd

Function .onInstSuccess
  Exec "$INSTDIR\MediaPortal.DeployTool.exe"
FunctionEnd