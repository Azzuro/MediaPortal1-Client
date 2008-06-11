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

!include /nonFatal "XML.nsh"
!ifndef xml::SetCondenseWhiteSpace
  !error "$\r$\n$\r$\nYou need the xml plugin to compile this script. Look at$\r$\n$\r$\n     http://nsis.sourceforge.net/XML_plug-in$\r$\n$\r$\ndownload and install it!$\r$\n$\r$\n"
!endif

!include FileFunc.nsh
!insertmacro GetRoot
!insertmacro un.GetRoot

!include WordFunc.nsh
!insertmacro WordReplace
!insertmacro un.WordReplace



#**********************************************************************************************************#
#
# code for file association was taken from:
#                         http://nsis.sourceforge.net/File_Association
#
#**********************************************************************************************************#
!define registerExtension "!insertmacro registerExtension"
!define unregisterExtension "!insertmacro unregisterExtension"
 
!macro registerExtension executable extension description
       Push "${executable}"  ; "full path to my.exe"
       Push "${extension}"   ;  ".mkv"
       Push "${description}" ;  "MKV File"
       Call registerExtension
!macroend
 
; back up old value of .opt
Function registerExtension
!define Index "Line${__LINE__}"
  pop $R0 ; ext name
  pop $R1
  pop $R2
  push $1
  push $0
  ReadRegStr $1 HKCR $R1 ""
  StrCmp $1 "" "${Index}-NoBackup"
    StrCmp $1 "OptionsFile" "${Index}-NoBackup"
    WriteRegStr HKCR $R1 "backup_val" $1
"${Index}-NoBackup:"
  WriteRegStr HKCR $R1 "" $R0
  ReadRegStr $0 HKCR $R0 ""
  StrCmp $0 "" 0 "${Index}-Skip"
	WriteRegStr HKCR $R0 "" $R0
	WriteRegStr HKCR "$R0\shell" "" "open"
	WriteRegStr HKCR "$R0\DefaultIcon" "" "$R2,0"
"${Index}-Skip:"
  WriteRegStr HKCR "$R0\shell\open\command" "" '$R2 "%1"'
  WriteRegStr HKCR "$R0\shell\edit" "" "Edit $R0"
  WriteRegStr HKCR "$R0\shell\edit\command" "" '$R2 "%1"'
  pop $0
  pop $1
!undef Index
FunctionEnd
 
!macro unregisterExtension extension description
       Push "${extension}"   ;  ".mkv"
       Push "${description}"   ;  "MKV File"
       Call un.unregisterExtension
!macroend
 
Function un.unregisterExtension
  pop $R1 ; description
  pop $R0 ; extension
!define Index "Line${__LINE__}"
  ReadRegStr $1 HKCR $R0 ""
  StrCmp $1 $R1 0 "${Index}-NoOwn" ; only do this if we own it
  ReadRegStr $1 HKCR $R0 "backup_val"
  StrCmp $1 "" 0 "${Index}-Restore" ; if backup="" then delete the whole key
  DeleteRegKey HKCR $R0
  Goto "${Index}-NoOwn"
"${Index}-Restore:"
  WriteRegStr HKCR $R0 "" $1
  DeleteRegValue HKCR $R0 "backup_val"
  DeleteRegKey HKCR $R1 ;Delete key with association name settings
"${Index}-NoOwn:"
!undef Index
FunctionEnd



#**********************************************************************************************************#
#
# logging system
#
#**********************************************************************************************************#
!ifdef INSTALL_LOG
!ifndef INSTALL_LOG_FILE
  !ifndef COMMON_APPDATA
    !error "$\r$\n$\r$\nCOMMON_APPDATA is not defined!$\r$\n$\r$\n"
  !endif

  !define INSTALL_LOG_FILE "${COMMON_APPDATA}\log\install_${VER_MAJOR}.${VER_MINOR}.${VER_REVISION}.${VER_BUILD}.log"
!endif

Var LogFile

!define prefixERROR "[ERROR     !!!]   "
!define prefixDEBUG "[    DEBUG    ]   "
!define prefixINFO  "[         INFO]   "

!define LOG_OPEN `!insertmacro LOG_OPEN`
!macro LOG_OPEN

  FileOpen $LogFile "$TEMP\install_$(^Name).log" w

!macroend

!define LOG_CLOSE `!insertmacro LOG_CLOSE`
!macro LOG_CLOSE

  FileClose $LogFile

  CopyFiles "$TEMP\install_$(^Name).log" "${INSTALL_LOG_FILE}"

!macroend

!define LOG_TEXT `!insertmacro LOG_TEXT`
!macro LOG_TEXT LEVEL TEXT

!if     "${LEVEL}" != "DEBUG"
  !if   "${LEVEL}" != "ERROR"
    !if "${LEVEL}" != "INFO"
      !error "$\r$\n$\r$\nYou call macro LOG_TEXT with wrong LogLevel. Only 'DEBUG', 'ERROR' and 'INFO' are valid!$\r$\n$\r$\n"
    !endif
  !endif
!endif

  FileWrite $LogFile "${prefix${LEVEL}}${TEXT}$\r$\n"

!macroend

!else

!define LOG_OPEN `!insertmacro LOG_OPEN`
!macro LOG_OPEN
!macroend

!define LOG_CLOSE `!insertmacro LOG_CLOSE`
!macro LOG_CLOSE
!macroend

!define LOG_TEXT `!insertmacro LOG_TEXT`
!macro LOG_TEXT LEVEL TEXT
!macroend

!endif



#**********************************************************************************************************#
#
# killing a process
#
#**********************************************************************************************************#
!define KILLPROCESS `!insertmacro KILLPROCESS`
!macro KILLPROCESS PROCESS
!if ${KILLMODE} == "1"
  ExecShell "" "Cmd.exe" '/C "taskkill /F /IM "${PROCESS}""' SW_HIDE
  Sleep 300
!else if ${KILLMODE} == "2"
  ExecWait '"taskkill" /F /IM "${PROCESS}"'
!else if ${KILLMODE} == "3"
  nsExec::ExecToLog '"taskkill" /F /IM "${PROCESS}"'
!else

  nsExec::ExecToLog '"taskkill" /F /IM "${PROCESS}"'

!endif
!macroend











#Var AR_SecFlags
#Var AR_RegFlags

# registry
# ${MEMENTO_REGISTRY_ROOT}
# ${MEMENTO_REGISTRY_KEY}
# ${MEMENTO_REGISTRY_KEY}
#ReadRegDWORD $AR_RegFlags ${MEMENTO_REGISTRY_ROOT} `${MEMENTO_REGISTRY_KEY}` `MementoSection_${__MementoSectionLastSectionId}`

 /*   not needed anymore ----- done by MementoSectionRestore
!macro InitSection SecName
    ;This macro reads component installed flag from the registry and
    ;changes checked state of the section on the components page.
    ;Input: section index constant name specified in Section command.

    ClearErrors
    ;Reading component status from registry
    ReadRegDWORD $AR_RegFlags "${MEMENTO_REGISTRY_ROOT}" "${MEMENTO_REGISTRY_KEY}" "${SecName}"
    IfErrors "default_${SecName}"
    
    ;Status will stay default if registry value not found
    ;(component was never installed)
    IntOp $AR_RegFlags $AR_RegFlags & 0x0001  ;Turn off all other bits
    SectionGetFlags ${${SecName}} $AR_SecFlags  ;Reading default section flags
    IntOp $AR_SecFlags $AR_SecFlags & 0xFFFE  ;Turn lowest (enabled) bit off
    IntOp $AR_SecFlags $AR_RegFlags | $AR_SecFlags      ;Change lowest bit

    ;Writing modified flags
    SectionSetFlags ${${SecName}} $AR_SecFlags

  "default_${SecName}:"
!macroend
*/

!macro FinishSection SecName
  ;This macro reads section flag set by user and removes the section
  ;if it is not selected.
  ;Then it writes component installed flag to registry
  ;Input: section index constant name specified in Section command.

  ${IfNot} ${SectionIsSelected} "${${SecName}}"
    ClearErrors
    ReadRegDWORD $R0 ${MEMENTO_REGISTRY_ROOT} '${MEMENTO_REGISTRY_KEY}' 'MementoSection_${SecName}'

    ${If} $R0 = 1
      !insertmacro "Remove_${${SecName}}"
    ${EndIf}
  ${EndIf}
!macroend

!macro RemoveSection SecName
  ;This macro is used to call section's Remove_... macro
  ;from the uninstaller.
  ;Input: section index constant name specified in Section command.

  !insertmacro "Remove_${${SecName}}"
!macroend

!macro DisableComponent SectionName AddText
  !insertmacro UnselectSection "${SectionName}"
  ; Make the unselected section read only
  !insertmacro SetSectionFlag "${SectionName}" 16
  SectionGetText ${SectionName} $R0
  SectionSetText ${SectionName} "$R0${AddText}"
!macroend



#**********************************************************************************************************#
#
# Useful macros for MediaPortal and addtional Software which can be used like other LogicLib expressions.
#
#**********************************************************************************************************#

!ifndef MP_REG_UNINSTALL
  !define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
!endif
!ifndef TV3_REG_UNINSTALL
  !define TV3_REG_UNINSTALL     "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"
!endif

#**********************************************************************************************************#
# LOGICLIB EXPRESSIONS

;======================================   OLD MP INSTALLATION TESTs

# old installations < 0.2.3.0 RC 3
!macro _MP022IsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{87819CFA-1786-484D-B0DE-10B5FBF2625D}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MP022IsInstalled `"" MP022IsInstalled ""`

!macro _MP023RC3IsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal 0.2.3.0 RC3" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MP023RC3IsInstalled `"" MP023RC3IsInstalled ""`

!macro _MP023IsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal 0.2.3.0" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MP023IsInstalled `"" MP023IsInstalled ""`

;======================================   OLD TVServer/TVClient INSTALLATION TESTs

!macro _MSI_TVServerIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{4B738773-EE07-413D-AFB7-BB0AB04A5488}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MSI_TVServerIsInstalled `"" MSI_TVServerIsInstalled ""`

!macro _MSI_TVClientIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ClearErrors
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{F7444E89-5BC0-497E-9650-E50539860DE0}" "UninstallString"
  IfErrors 0 `${_t}`
  ReadRegStr $_LOGICLIB_TEMP HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{FD9FD453-1C0C-4EDA-AEE6-D7CF0E9951CA}" "UninstallString"
  IfErrors `${_f}` `${_t}`
!macroend
!define MSI_TVClientIsInstalled `"" MSI_TVClientIsInstalled ""`

;======================================

!macro _MPIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${MP_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP `${_t}` `${_f}`
!macroend
!define MPIsInstalled `"" MPIsInstalled ""`

!macro _TVServerIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP 0 `${_f}`

  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "MementoSection_SecServer"
  StrCmp $_LOGICLIB_TEMP 1 `${_t}` `${_f}`
!macroend
!define TVServerIsInstalled `"" TVServerIsInstalled ""`

!macro _TVClientIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP
  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "UninstallString"

  IfFileExists $_LOGICLIB_TEMP 0 `${_f}`

  ReadRegStr $_LOGICLIB_TEMP HKLM "${TV3_REG_UNINSTALL}" "MementoSection_SecClient"
  StrCmp $_LOGICLIB_TEMP 1 `${_t}` `${_f}`
!macroend
!define TVClientIsInstalled `"" TVClientIsInstalled ""`

;======================================   3rd PARTY APPLICATION TESTs

!macro _VCRedistIsInstalled _a _b _t _f

  ClearErrors
  ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" CurrentVersion
  IfErrors `${_f}`

  StrCpy $R1 $R0 3
 
  StrCmp $R1 '5.1' lbl_winnt_XP
  StrCmp $R1 '5.2' lbl_winnt_2003
  StrCmp $R1 '6.0' lbl_winnt_vista `${_f}`


  lbl_winnt_vista:
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc80.crt_1fc8b3b9a1e18e3b_8.0.50727.762_none_10b2f55f9bffb8f8.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc80.mfc_1fc8b3b9a1e18e3b_8.0.50727.762_none_0c178a139ee2a7ed.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_microsoft.vc80.atl_1fc8b3b9a1e18e3b_8.0.50727.762_none_11ecb0ab9b2caf3c.manifest" 0 `${_f}`
  Goto `${_t}`

  lbl_winnt_2003:
  lbl_winnt_XP:
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC80.CRT_1fc8b3b9a1e18e3b_8.0.50727.762_x-ww_6b128700.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC80.MFC_1fc8b3b9a1e18e3b_8.0.50727.762_x-ww_3bf8fa05.manifest" 0 `${_f}`
  IfFileExists "$WINDIR\WinSxS\Manifests\x86_Microsoft.VC80.ATL_1fc8b3b9a1e18e3b_8.0.50727.762_x-ww_cbb27474.manifest" 0 `${_f}`
  Goto `${_t}`
!macroend
!define VCRedistIsInstalled `"" VCRedistIsInstalled ""`

!macro _dotNetIsInstalled _a _b _t _f
  SetRegView 32

  !insertmacro _LOGICLIB_TEMP

  ReadRegStr $4 HKLM "Software\Microsoft\.NETFramework" "InstallRoot"
  # remove trailing back slash
  Push $4
  Exch $EXEDIR
  Exch $EXEDIR
  Pop $4
  # if the root directory doesn't exist .NET is not installed
  IfFileExists $4 0 `${_f}`

  StrCpy $0 0

  EnumStart:

    EnumRegKey $2 HKLM "Software\Microsoft\.NETFramework\Policy"  $0
    IntOp $0 $0 + 1
    StrCmp $2 "" `${_f}`

    StrCpy $1 0

    EnumPolicy:

      EnumRegValue $3 HKLM "Software\Microsoft\.NETFramework\Policy\$2" $1
      IntOp $1 $1 + 1
       StrCmp $3 "" EnumStart
        IfFileExists "$4\$2.$3" `${_t}` EnumPolicy
!macroend
!define dotNetIsInstalled `"" dotNetIsInstalled ""`

#**********************************************************************************************************#
# Get MP infos
!macro MP_GET_INSTALL_DIR _var
  SetRegView 32

  ${If} ${MP023IsInstalled}
    ReadRegStr ${_var} HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"
  ${ElseIf} ${MPIsInstalled}
    ReadRegStr ${_var} HKLM "${MP_REG_UNINSTALL}" "InstallPath"
  ${Else}
    StrCpy ${_var} ""
  ${EndIf}

!macroend

!macro TVSERVER_GET_INSTALL_DIR _var
  SetRegView 32

  ${If} ${TVServerIsInstalled}
    ReadRegStr ${_var} HKLM "${TV3_REG_UNINSTALL}" "InstallPath"
  ${Else}
    StrCpy ${_var} ""
  ${EndIf}

!macroend

!insertmacro GetTime
!macro GET_BACKUP_POSTFIX _var

  ${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
  ; $0="01"      day
  ; $1="04"      month
  ; $2="2005"    year
  ; $3="Friday"  day of week name
  ; $4="16"      hour
  ; $5="05"      minute
  ; $6="50"      seconds
  
  StrCpy ${_var} "BACKUP_$1-$0_$4-$5"

!macroend



#**********************************************************************************************************#
#
# common language strings
#
#**********************************************************************************************************#
LangString TEXT_MP_NOT_INSTALLED                  ${LANG_ENGLISH} "MediaPortal not installed"
LangString TEXT_TVSERVER_NOT_INSTALLED            ${LANG_ENGLISH} "TVServer not installed"


LangString TEXT_MSGBOX_INSTALLATION_CANCELD       ${LANG_ENGLISH} "Installation will be canceled."
LangString TEXT_MSGBOX_MORE_INFO                  ${LANG_ENGLISH} "Do you want to get more information about it?"

LangString TEXT_MSGBOX_ERROR_WIN                  ${LANG_ENGLISH} "Your operating system is not supported by $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_WIN_NOT_RECOMMENDED  ${LANG_ENGLISH} "Your operating system is not recommended by $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_ADMIN                ${LANG_ENGLISH} "You need administration rights to install $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)"
LangString TEXT_MSGBOX_ERROR_VCREDIST             ${LANG_ENGLISH} "Microsoft Visual C++ 2005 SP1 Redistributable Package (x86) is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"
LangString TEXT_MSGBOX_ERROR_DOTNET               ${LANG_ENGLISH} "Microsoft .Net Framework 2 is not installed.$\r$\nIt is a requirement for $(^Name).$\r$\n$\r$\n$(TEXT_MSGBOX_INSTALLATION_CANCELD)$\r$\n$\r$\n$(TEXT_MSGBOX_MORE_INFO)"

LangString TEXT_MSGBOX_ERROR_IS_INSTALLED         ${LANG_ENGLISH} "$(^Name) is already installed. You need to uninstall it, before you continue with the installation.$\r$\nUninstall will be lunched when pressing OK."
LangString TEXT_MSGBOX_ERROR_ON_UNINSTALL         ${LANG_ENGLISH} "An error occured while trying to uninstall old version!$\r$\nDo you still want to continue the installation?"
LangString TEXT_MSGBOX_ERROR_REBOOT_REQUIRED      ${LANG_ENGLISH} "A reboot is required after a previous action. Reboot you system and try it again."



  /*
; Section flag test
!macro _MPIsInstalled _a _b _t _f
  !insertmacro _LOGICLIB_TEMP

  ReadRegStr $MPBaseDir HKLM "${MP_REG_UNINSTALL}" "UninstallString"
  ${If} $MPBaseDir == ""
    # this fallback should only be enabled until MediaPortal 1.0 is out
    ReadRegStr $MPBaseDir HKLM "SOFTWARE\Team MediaPortal\MediaPortal" "ApplicationDir"

#!define MP_REG_UNINSTALL      "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal"
#!define TV3_REG_UNINSTALL     "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal TV Server"

    ${If} $MPBaseDir == ""
        !insertmacro UnselectSection "${SecClient}"
        ; Make the unselected section read only
        !insertmacro SetSectionFlag "${SecClient}" 16
        SectionGetText ${SecClient} $R0
        SectionSetText ${SecClient} "$R0 ($(TEXT_MP_NOT_INSTALLED))"
    ${EndIf}
  ${EndIf}
    SectionGetFlags `${_b}` $_LOGICLIB_TEMP
    IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP & `${_a}`

    !insertmacro _= $_LOGICLIB_TEMP `${_a}` `${_t}` `${_f}`
  !macroend
  
  #!define MPIsInstalled `${SF_SELECTED} SectionFlagIsSet`
!define MPIsInstalled "!insertmacro _MPIsInstalled"
  */



#***************************
#***************************
  
Var MyDocs
Var UserAppData
Var CommonAppData

Var MPdir.Base

Var MPdir.Config
Var MPdir.Plugins
Var MPdir.Log
Var MPdir.CustomInputDevice
Var MPdir.CustomInputDefault
Var MPdir.Skin
Var MPdir.Language
Var MPdir.Database
Var MPdir.Thumbs
Var MPdir.Weather
Var MPdir.Cache
Var MPdir.BurnerSupport

#***************************
#***************************

!macro GET_PATH_TEXT

  Pop $R0

  ${xml::GotoPath} "/Config" $0
  ${If} $0 != 0
    ${LOG_TEXT} "ERROR" "xml::GotoPath /Config"
    Goto error
  ${EndIf}

  loop:

  ${xml::FindNextElement} "Dir" $0 $1
  ${If} $1 != 0
    ${LOG_TEXT} "ERROR" "xml::FindNextElement >/Dir< >$0<"
    Goto error
  ${EndIf}

  ${xml::ElementPath} $0
  ${xml::GetAttribute} "id" $0 $1
  ${If} $1 != 0
    ${LOG_TEXT} "ERROR" "xml::GetAttribute >id< >$0<"
    Goto error
  ${EndIf}
  ${IfThen} $0 == $R0  ${|} Goto foundDir ${|}

  Goto loop


  foundDir:
  ${xml::ElementPath} $0
  ${xml::GotoPath} "$0/Path" $1
  ${If} $1 != 0
    ${LOG_TEXT} "ERROR" "xml::GotoPath >$0/Path<"
    Goto error
  ${EndIf}

  ${xml::GetText} $0 $1
  ${If} $1 != 0
    ; maybe the path is only empty, which means MPdir.Base
    #MessageBox MB_OK "error: xml::GetText"
    #Goto error
    StrCpy $0 ""
  ${EndIf}

  Push $0
  Goto end

  error:
  Push "-1"

  end:

!macroend
Function GET_PATH_TEXT
  !insertmacro GET_PATH_TEXT
FunctionEnd
Function un.GET_PATH_TEXT
  !insertmacro GET_PATH_TEXT
FunctionEnd

#***************************
#***************************

!macro ReadMPdir UNINSTALL_PREFIX DIR
  ${LOG_TEXT} "DEBUG" "macro: ReadMPdir | DIR: ${DIR}"

  Push "${DIR}"
  Call ${UNINSTALL_PREFIX}GET_PATH_TEXT
  Pop $0
  ${IfThen} $0 == -1 ${|} Goto error ${|}

  ${LOG_TEXT} "DEBUG" "macro: ReadMPdir | text found in xml: '$0'"
  ${${UNINSTALL_PREFIX}WordReplace} "$0" "%APPDATA%" "$UserAppData" "+" $0
  ${${UNINSTALL_PREFIX}WordReplace} "$0" "%PROGRAMDATA%" "$CommonAppData" "+" $0

  ${${UNINSTALL_PREFIX}GetRoot} "$0" $1

  ${IfThen} $1 == "" ${|} StrCpy $0 "$MPdir.Base\$0" ${|}

  ; TRIM    \    AT THE END
  StrLen $1 "$0"
    #${DEBUG_MSG} "1 $1$\r$\n2 $2$\r$\n3 $3"
  IntOp $2 $1 - 1
    #${DEBUG_MSG} "1 $1$\r$\n2 $2$\r$\n3 $3"
  StrCpy $3 $0 1 $2
    #${DEBUG_MSG} "1 $1$\r$\n2 $2$\r$\n3 $3"

  ${If} $3 == "\"
    StrCpy $MPdir.${DIR} $0 $2
  ${Else}
    StrCpy $MPdir.${DIR} $0
  ${EndIf}

!macroend

#***************************
#***************************

!macro ReadConfig UNINSTALL_PREFIX PATH_TO_XML
  ${LOG_TEXT} "DEBUG" "macro: ReadConfig | UNINSTALL_PREFIX: ${UNINSTALL_PREFIX} | PATH_TO_XML: ${PATH_TO_XML}"

  IfFileExists "${PATH_TO_XML}\MediaPortalDirs.xml" 0 error

  
  #${xml::LoadFile} "$EXEDIR\MediaPortalDirsXP.xml" $0
  ${xml::LoadFile} "$0\MediaPortalDirs.xml" $0
  ${IfThen} $0 != 0 ${|} Goto error ${|}

  #</Dir>  Log CustomInputDevice CustomInputDefault Skin Language Database Thumbs Weather Cache BurnerSupport

  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Config
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Plugins
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Log
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" CustomInputDevice
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" CustomInputDefault
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Skin
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Language
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Database
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Thumbs
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Weather
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" Cache
  !insertmacro ReadMPdir "${UNINSTALL_PREFIX}" BurnerSupport


  Push "0"
  Goto end

  error:
  Push "-1"

  end:

!macroend
Function ReadConfig
  Pop $0

  !insertmacro ReadConfig "" "$0"
FunctionEnd
Function un.ReadConfig
  Pop $0

  !insertmacro ReadConfig "un." "$0"
FunctionEnd

#***************************
#***************************

!macro LoadDefaultDirs

  StrCpy $MPdir.Config              "$CommonAppData\Team MediaPortal\MediaPortal"

  StrCpy $MPdir.Plugins             "$MPdir.Base\plugins"
  StrCpy $MPdir.Log                 "$MPdir.Config\log"
  StrCpy $MPdir.CustomInputDevice   "$MPdir.Config\InputDeviceMappings"
  StrCpy $MPdir.CustomInputDefault  "$MPdir.Base\InputDeviceMappings\defaults"
  StrCpy $MPdir.Skin                "$MPdir.Base\skin"
  StrCpy $MPdir.Language            "$MPdir.Base\language"
  StrCpy $MPdir.Database            "$MPdir.Config\database"
  StrCpy $MPdir.Thumbs              "$MPdir.Config\thumbs"
  StrCpy $MPdir.Weather             "$MPdir.Base\weather"
  StrCpy $MPdir.Cache               "$MPdir.Config\cache"
  StrCpy $MPdir.BurnerSupport       "$MPdir.Base\Burner"

!macroend

#***************************
#***************************

!define ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirs ""`
!define un.ReadMediaPortalDirs `!insertmacro ReadMediaPortalDirs "un."`
!macro ReadMediaPortalDirs UNINSTALL_PREFIX INSTDIR
  ${LOG_TEXT} "DEBUG" "macro ReadMediaPortalDirs"

  StrCpy $MPdir.Base "${INSTDIR}"
  SetShellVarContext current
  StrCpy $MyDocs "$DOCUMENTS"
  StrCpy $UserAppData "$APPDATA"
  SetShellVarContext all
  StrCpy $CommonAppData "$APPDATA"


  !insertmacro LoadDefaultDirs

  Push "$MyDocs\Team MediaPortal"
  Call ${UNINSTALL_PREFIX}ReadConfig
  Pop $0
  ${If} $0 != 0   ; an error occured
    ${LOG_TEXT} "ERROR" "could not read '$MyDocs\Team MediaPortal\MediaPortalDirs.xml'"

    Push "$MPdir.Base"
    Call ${UNINSTALL_PREFIX}ReadConfig
    Pop $0
    ${If} $0 != 0   ; an error occured
      ${LOG_TEXT} "ERROR" "could not read '$MPdir.Base\MediaPortalDirs.xml'"

      ${LOG_TEXT} "INFO" "no MediaPortalDirs.xml read. using LoadDefaultDirs"
      !insertmacro LoadDefaultDirs

    ${Else}
      ${LOG_TEXT} "INFO" "read '$MPdir.Base\MediaPortalDirs.xml' successfully"
    ${EndIf}

  ${Else}
    ${LOG_TEXT} "INFO" "read '$MyDocs\Team MediaPortal\MediaPortalDirs.xml' successfully"
  ${EndIf}

  ${LOG_TEXT} "INFO" "Installer will use the following directories:$\r$\n"
  ${LOG_TEXT} "INFO" "          Base:  $MPdir.Base"
  ${LOG_TEXT} "INFO" "          Config:  $MPdir.Config"
  ${LOG_TEXT} "INFO" "          Plugins: $MPdir.Plugins"
  ${LOG_TEXT} "INFO" "          Log: $MPdir.Log"
  ${LOG_TEXT} "INFO" "          CustomInputDevice: $MPdir.CustomInputDevice"
  ${LOG_TEXT} "INFO" "          CustomInputDefault: $MPdir.CustomInputDefault"
  ${LOG_TEXT} "INFO" "          Skin: $MPdir.Skin"
  ${LOG_TEXT} "INFO" "          Language: $MPdir.Language"
  ${LOG_TEXT} "INFO" "          Database: $MPdir.Database"
  ${LOG_TEXT} "INFO" "          Thumbs: $MPdir.Thumbs"
  ${LOG_TEXT} "INFO" "          Weather: $MPdir.Weather"
  ${LOG_TEXT} "INFO" "          Cache: $MPdir.Cache"
  ${LOG_TEXT} "INFO" "          BurnerSupport: $MPdir.BurnerSupport"
!macroend










!ifdef WINVER++
; ADVANCED WinVer
; ---------------------
;      WinVer.nsh
; ---------------------
;
; LogicLib extensions for handling Windows versions.
;
; IsNT checks if the installer is running on Windows NT family (NT4, 2000, XP, etc.)
;
;   ${If} ${IsNT}
;     DetailPrint "Running on NT. Installing Unicode enabled application."
;   ${Else}
;     DetailPrint "Not running on NT. Installing ANSI application."
;   ${EndIf}
;
; AtLeastWin<version> checks if the installer is running on Windows version at least as specified.
; IsWin<version> checks if the installer is running on Windows version exactly as specified.
; AtMostWin<version> checks if the installer is running on Windows version at most as specified.
;
; <version> can be replaced with the following values:
;
;   95
;   98
;   ME
;
;   NT4
;   2000
;   2000Srv
;   XP
;   XP64
;   2003
;   Vista
;   2008
;
; Usage examples:
;
;   ${If} ${IsNT}
;   DetailPrint "Running on NT family."
;   DetailPrint "Surely not running on 95, 98 or ME."
;   ${AndIf} ${AtLeastWinNT4}
;     DetailPrint "Running on NT4 or better. Could even be 2003."
;   ${EndIf}
;
;   ${If} ${AtLeastWinXP}
;     DetailPrint "Running on XP or better."
;   ${EndIf}
;
;   ${If} ${IsWin2000}
;     DetailPrint "Running on 2000."
;   ${EndIf}
;
;   ${If} ${AtMostWinXP}
;     DetailPrint "Running on XP or older. Surely not running on Vista. Maybe 98, or even 95."
;   ${EndIf}
;
; Warning:
;
;   Windows 95 and NT both use the same version number. To avoid getting NT4 misidentified
;   as Windows 95 and vice-versa or 98 as a version higher than NT4, always use IsNT to
;   check if running on the NT family.
;
;     ${If} ${AtLeastWin95}
;     ${And} ${AtMostWinME}
;       DetailPrint "Running 95, 98 or ME."
;       DetailPrint "Actually, maybe it's NT4?"
;       ${If} ${IsNT}
;         DetailPrint "Yes, it's NT4! oops..."
;       ${Else}
;         DetailPrint "Nope, not NT4. phew..."
;       ${EndIf}
;     ${EndIf}

!verbose push
!verbose 3

!ifndef ___WINVER__NSH___
!define ___WINVER__NSH___

!include LogicLib.nsh

!define WINVER_95      0x4000
!define WINVER_98      0x40A0 ;4.10
!define WINVER_ME      0x45A0 ;4.90

!define WINVER_NT4     0x4000
!define WINVER_2000    0x5000
!define WINVER_2000Srv 0x5001
!define WINVER_XP      0x5010
!define WINVER_XP64    0x5020
!define WINVER_2003    0x5021
!define WINVER_VISTA   0x6000
!define WINVER_2008    0x6001


!macro __ParseWinVer
	!insertmacro _LOGICLIB_TEMP
	Push $0
	Push $1
	System::Call '*(i 148,i,i,i,i,&t128,&i2,&i2,&i2,&i1,&i1)i.r1' ;BUGBUG: no error handling for mem alloc failure!
	System::Call 'kernel32::GetVersionEx(i r1)'
	System::Call '*$1(i,i.r0)'
	!define _WINVER_PARSEVER_OLDSYS _WINVER${__LINE__}
	IntCmpU $0 5 0 ${_WINVER_PARSEVER_OLDSYS} ;OSVERSIONINFOEX can be used on NT4SP6 and later, but we only use it on NT5+
	System::Call '*$1(i 156)'
	System::Call 'kernel32::GetVersionEx(i r1)'
	${_WINVER_PARSEVER_OLDSYS}:
	!undef _WINVER_PARSEVER_OLDSYS
	IntOp $_LOGICLIB_TEMP $0 << 12 ;we already have the major version in r0
	System::Call '*$1(i,i,i.r0)'
	IntOp $0 $0 << 4
	IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP | $0
	System::Call '*$1(i,i,i,i,i,&t128,&i2,&i2,&i2,&i1.r0,&i1)'
	!define _WINVER_PARSEVER_NOTNTSERVER _WINVER${__LINE__}
	IntCmp $0 1 ${_WINVER_PARSEVER_NOTNTSERVER} ${_WINVER_PARSEVER_NOTNTSERVER} ;oviex.wProductType > VER_NT_WORKSTATION
	IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP | 1
	${_WINVER_PARSEVER_NOTNTSERVER}:
	!undef _WINVER_PARSEVER_NOTNTSERVER
	System::Free $1
	pop $1
	pop $0
!macroend

!macro __GetServicePack
  Push $0
  Push $1
  System::Call '*(i 148,i,i,i,i,&t128,&i2,&i2,&i2,&i1,&i1)i.r1' ;BUGBUG: no error handling for mem alloc failure!
  System::Call 'kernel32::GetVersionEx(i r1)'
  System::Call '*$1(i,i,i,i,i,i,i.r0)'

  MessageBox MB_OK|MB_ICONEXCLAMATION "Service Pack: $0"
  ;IntCmpU $0 5 0 ${_WINVER_PARSEVER_OLDSYS} ;OSVERSIONINFOEX can be used on NT4SP6 and later, but we only use it on NT5+

  System::Free $1
  pop $1
  pop $0
!macroend

!macro _IsNT _a _b _t _f
  !insertmacro _LOGICLIB_TEMP
  System::Call kernel32::GetVersion()i.s
  Pop $_LOGICLIB_TEMP
  IntOp $_LOGICLIB_TEMP $_LOGICLIB_TEMP & 0x80000000
  !insertmacro _== $_LOGICLIB_TEMP 0 `${_t}` `${_f}`
!macroend
!define IsNT `"" IsNT ""`

!macro __WinVer_DefineOSTest Test OS

  !define ${Test}Win${OS} `"" WinVer${Test} ${WINVER_${OS}}`

!macroend

!macro __WinVer_DefineOSTests Test

  !insertmacro __WinVer_DefineOSTest ${Test} 95
  !insertmacro __WinVer_DefineOSTest ${Test} 98
  !insertmacro __WinVer_DefineOSTest ${Test} ME
  !insertmacro __WinVer_DefineOSTest ${Test} NT4
  !insertmacro __WinVer_DefineOSTest ${Test} 2000
  !insertmacro __WinVer_DefineOSTest ${Test} 2000Srv
  !insertmacro __WinVer_DefineOSTest ${Test} XP
  !insertmacro __WinVer_DefineOSTest ${Test} XP64
  !insertmacro __WinVer_DefineOSTest ${Test} 2003
  !insertmacro __WinVer_DefineOSTest ${Test} VISTA
  !insertmacro __WinVer_DefineOSTest ${Test} 2008

!macroend

!macro _WinVerAtLeast _a _b _t _f
  !insertmacro __ParseWinVer
  !insertmacro _>= $_LOGICLIB_TEMP `${_b}` `${_t}` `${_f}`
!macroend

!macro _WinVerIs _a _b _t _f
  !insertmacro __ParseWinVer
  !insertmacro _= $_LOGICLIB_TEMP `${_b}` `${_t}` `${_f}`
!macroend

!macro _WinVerAtMost _a _b _t _f
  !insertmacro __ParseWinVer
  !insertmacro _<= $_LOGICLIB_TEMP `${_b}` `${_t}` `${_f}`
!macroend

!insertmacro __WinVer_DefineOSTests AtLeast
!insertmacro __WinVer_DefineOSTests Is
!insertmacro __WinVer_DefineOSTests AtMost

!endif # !___WINVER__NSH___

!verbose pop
!endif
