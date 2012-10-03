; Script generated by the HM NIS Edit Script Wizard.
!include WordFunc.nsh
!insertmacro VersionCompare
;!include LogicLib.nsh

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "TickZoom MB Trading Service"
!define PRODUCT_VERSION "Beta5"
!define PRODUCT_PUBLISHER "TickZOOM"
!define PRODUCT_WEB_SITE "http://www.tickzoom.org"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\MBTradingService.exe ${PRODUCT_VERSION}"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME} ${PRODUCT_VERSION}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"
!define NSIS

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!insertmacro MUI_PAGE_LICENSE "license.rtf"
; Directory page
;!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page

!define MUI_FINISHPAGE_RUN "NotePad.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Configure ${PRODUCT_NAME}"
!define MUI_FINISHPAGE_RUN_PARAMETERS "$INSTDIR\MBTradingService.exe.config"

!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "MBTServiceSetup${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES\TickZOOM\${PRODUCT_VERSION}\MB Trading Service"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section 'NET Framework 3.5 SP1' SEC01
    SetOutPath '$INSTDIR'
    LogSet on
    SetOverwrite on
    Pop $1
    
    ;registry
    ReadRegDWORD $0 HKLM 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5' Install

    LogText "NET Framework Registry Version $0"
    ${If} $0 == 1
          DetailPrint '..NET Framework 3.5 already installed.'
    ${Else}

        ;options
      inetc::get /RESUME "Your internet connection seems to have dropped out!\nPlease reconnect and click Retry to resume downloading..." /caption "Downloading .NET Framework 3.5" /canceltext "Cancel" "http://www.tickzoom.org/chrome/site/dotnetfx35.exe" "$INSTDIR\dotnetfx35.exe" /end
      ${If} $1 != "OK"
        DetailPrint "Download of .NET Framework 3.5 failed: $1"
        Abort "Installation Canceled."
      ${EndIf}

      ;file work
      ExecWait "$INSTDIR\dotnetfx35.exe /quiet /norestart" $0
      DetailPrint "..NET Framework 3.5 SP1 exit code = $0"

    ${EndIf}

SectionEnd

Section "TickZOOM MB Trading Service" SEC02
  SetOutPath "$INSTDIR"
  LogSet on
  SetOverwrite try
  File "..\bin\Debug\MBTradingService.exe"
  File "..\bin\Debug\MBTradingService.exe.config"
  File "..\bin\Debug\QuotesUI.exe"
  File "..\bin\Debug\QuotesUI.exe.config"
  File "..\bin\Debug\MBTradingService.exe.config"
  File "..\bin\Debug\Interop.LICDLLLib.dll"
  File "..\bin\Debug\Interop.MBTCOMLib.dll"
  File "..\bin\Debug\Interop.MBTHISTLib.dll"
  File "..\bin\Debug\Interop.MBTNAVLib.dll"
  File "..\bin\Debug\Interop.MBTORDERSLib.dll"
  File "..\bin\Debug\Interop.MBTQUOTELib.dll"
  File "..\bin\Debug\TickZoomAPI1.0.dll"
  File "..\bin\Debug\MBTrading.dll"
  File "..\bin\Debug\TickZoomCommunication.dll"
  File "..\bin\Debug\TickZoomLogging.dll"
  File "..\bin\Debug\TickZoomTickUtil.dll"
  File "Start.cmd"
  File "Stop.cmd"
  File "Restart.cmd"
  CreateDirectory "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}"
  CreateDirectory "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service"
  CreateShortCut "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Start.lnk" $INSTDIR\Start.cmd
  CreateShortCut "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Stop.lnk" $INSTDIR\Stop.cmd
  CreateShortCut "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Restart.lnk" $INSTDIR\Restart.cmd
  CreateShortCut "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Configure.lnk" NotePad.exe $INSTDIR\MBTradingService.exe.config
  SimpleSC::InstallService "MBTradingService" "MB Trading Service" "16" "2" "$INSTDIR\MBTradingService.exe" "" "" ""
  SimpleSC::SetServiceDescription "MBTradingService" "Processes orders, captures, and serves tick data to clients in TickZOOM format from MB Trading Broker."
SectionEnd

Section -AdditionalIcons
  SetOutPath $INSTDIR
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\MBTradingService.exe ${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\MBTradingService.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  SimpleSC::StopService MBTradingService
  SimpleSC::RemoveService MBTradingService
  Delete "$INSTDIR\${PRODUCT_NAME}.url"  
  Delete "$INSTDIR\install.log"
  Delete "$INSTDIR\Interop.LICDLLLib.dll"
  Delete "$INSTDIR\Interop.MBTCOMLib.dll"
  Delete "$INSTDIR\Interop.MBTHISTLib.dll"
  Delete "$INSTDIR\Interop.MBTNAVLib.dll"
  Delete "$INSTDIR\Interop.MBTORDERSLib.dll"
  Delete "$INSTDIR\Interop.MBTQUOTELib.dll"
  Delete "$INSTDIR\MBTrading.dll"
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\MBTradingService.exe"
  Delete "$INSTDIR\MBTradingService.exe.config"
  Delete "$INSTDIR\QuotesUI.exe"
  Delete "$INSTDIR\QuotesUI.exe.config"
  Delete "$INSTDIR\Start.cmd"
  Delete "$INSTDIR\Stop.cmd"
  Delete "$INSTDIR\Restart.cmd"
  Delete "$INSTDIR\TickZoomAPI1.0.dll"
  Delete "$INSTDIR\TickZoomCommunication.dll"
  Delete "$INSTDIR\TickZoomLogging.dll"
  Delete "$INSTDIR\TickZoomTickUtil.dll"
  Delete "$INSTDIR\dotnetfx35.exe"
  
  RMDir "$INSTDIR"

  ; Shortcuts to Start and Stop the Service
  Delete "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Start.lnk"
  Delete "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Stop.lnk"
  Delete "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Restart.lnk"
  Delete "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Configure.lnk"
  Delete "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Website.lnk"
  Delete "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service\Uninstall.lnk"

  RMDir "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}\MB Trading Service"
  RMDir "$SMPROGRAMS\TickZOOM ${PRODUCT_VERSION}"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd

Function AdvReplaceInFile
Exch $0 ;file to replace in
Exch
Exch $1 ;number to replace after
Exch
Exch 2
Exch $2 ;replace and onwards
Exch 2
Exch 3
Exch $3 ;replace with
Exch 3
Exch 4
Exch $4 ;to replace
Exch 4
Push $5 ;minus count
Push $6 ;universal
Push $7 ;end string
Push $8 ;left string
Push $9 ;right string
Push $R0 ;file1
Push $R1 ;file2
Push $R2 ;read
Push $R3 ;universal
Push $R4 ;count (onwards)
Push $R5 ;count (after)
Push $R6 ;temp file name

  GetTempFileName $R6
  FileOpen $R1 $0 r ;file to search in
  FileOpen $R0 $R6 w ;temp file
   StrLen $R3 $4
   StrCpy $R4 -1
   StrCpy $R5 -1

loop_read:
 ClearErrors
 FileRead $R1 $R2 ;read line
 IfErrors exit

   StrCpy $5 0
   StrCpy $7 $R2

loop_filter:
   IntOp $5 $5 - 1
   StrCpy $6 $7 $R3 $5 ;search
   StrCmp $6 "" file_write2
   StrCmp $6 $4 0 loop_filter

StrCpy $8 $7 $5 ;left part
IntOp $6 $5 + $R3
IntCmp $6 0 is0 not0
is0:
StrCpy $9 ""
Goto done
not0:
StrCpy $9 $7 "" $6 ;right part
done:
StrCpy $7 $8$3$9 ;re-join

IntOp $R4 $R4 + 1
StrCmp $2 all file_write1
StrCmp $R4 $2 0 file_write2
IntOp $R4 $R4 - 1

IntOp $R5 $R5 + 1
StrCmp $1 all file_write1
StrCmp $R5 $1 0 file_write1
IntOp $R5 $R5 - 1
Goto file_write2

file_write1:
 FileWrite $R0 $7 ;write modified line
Goto loop_read

file_write2:
 FileWrite $R0 $R2 ;write unmodified line
Goto loop_read

exit:
  FileClose $R0
  FileClose $R1

   SetDetailsPrint none
  Delete $0
  Rename $R6 $0
  Delete $R6
   SetDetailsPrint both

Pop $R6
Pop $R5
Pop $R4
Pop $R3
Pop $R2
Pop $R1
Pop $R0
Pop $9
Pop $8
Pop $7
Pop $6
Pop $5
Pop $0
Pop $1
Pop $2
Pop $3
Pop $4
FunctionEnd