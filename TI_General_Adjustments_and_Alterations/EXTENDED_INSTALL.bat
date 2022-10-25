cd ExtendedInstaller
copy "..\..\..\..\TerraInvicta_Data\Managed\Assembly-CSharp.dll" "Assembly-CSharp.dll"
ExtendedInstaller.exe ../../../../TerraInvicta_Data/Managed/Assembly-CSharp.dll GetAllModifiers,StartMissionPhase targetID,councilorID ../TI_General_Adjustments_Alterations.dll
del "Assembly-CSharp.dll"
cd ..
cd ..
cd ..
cd ..
timeout /t 2
start "" TerraInvicta.exe
