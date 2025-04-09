cd ExtendedInstaller
copy "..\..\..\..\TerraInvicta_Data\Managed\Assembly-CSharp.dll" "Assembly-CSharp.dll"
ExtendedInstaller.exe ../../../../TerraInvicta_Data/Managed/Assembly-CSharp.dll GetAllModifiers,StartMissionPhase,RandomizeStats,RandomizeSiteMiningData targetID,councilorID ../TI_Augmenter.dll
del "Assembly-CSharp.dll"
cd ..
cd ..
cd ..
cd ..
timeout /t 2
start "" TerraInvicta.exe
