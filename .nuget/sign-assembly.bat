@echo off
if "%1"=="" goto end
if not exist %1 goto end
if exist %1.signed goto end
set ildasm="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\ildasm.exe"
set sn="C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\sn.exe"
if not exist "C:\Program Files (x86)" set ildasm="C:\Program Files\Microsoft SDKs\Windows\v7.0A\Bin\ildasm.exe" end
if not exist "C:\Program Files (x86)" set sn="C:\Program Files\Microsoft SDKs\Windows\v7.0A\Bin\sn.exe" end
set ilasm="%windir%\Microsoft.NET\Framework\v4.0.30319\ilasm.exe"
echo Renaming original DLL...
move %1 %1.unsigned
echo Disassembling renamed original DLL...
pushd %~dp0
%ildasm% %1.unsigned /out:%~dpn1.il
echo Generating new strong name key...
%sn% -k %~dpn1.snk > nul:
echo Reassembling and signing with strong name key...
%ilasm% %~dpn1.il /dll /key=%~dpn1.snk /quiet
popd
echo Deleting intermediate disassembly and strong name key...
del %~dpn1.il
del %~dpn1.snk
echo Done!
:end