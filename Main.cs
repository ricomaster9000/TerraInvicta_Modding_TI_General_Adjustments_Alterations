using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using TI_Balancer.adjustments_alterations.harmonypatches.missionrelated;
using TI_General_Adjustments_Alterations.adjustment_alterations.core.missionrelated;
using TI_General_Adjustments_Alterations.adjustment_alterations.core.regionstate;
using TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches;
using TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches.councilorstate;
using TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches.factionstate;
using TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches.habsitestate;
using TI_General_Adjustments_Alterations.adjustments_alterations.harmonypatches.missionrelated;
using UnityEngine;
using UnityModManagerNet;
// ReSharper disable All

namespace TI_General_Adjustments_Alterations
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        private static Harmony harmony = null;

        public static bool Load(UnityModManager.ModEntry modEntry) {
            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            Config.LoadValues();
            if (Config.IsDebugModeActive())
            {
                PrintOutAllGameAssemblyMethods();
            }

            bool gameHasBeenUpdatedRerunExtendedInstall = false;
            if (Config.GetValueAsBool("extended_installation_completed"))
            {
                long originalSize = new System.IO.FileInfo(Path.GetFullPath("TerraInvicta_Data\\Managed\\Assembly-CSharp.dll")).Length;
                int extendSize = Config.GetValueAsInt("extended_installation_main_dll_file_size");
                gameHasBeenUpdatedRerunExtendedInstall = originalSize != extendSize;
            }
            if (!Config.GetValueAsBool("extended_installation_completed") || gameHasBeenUpdatedRerunExtendedInstall)
            {
                var psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.FileName = Path.GetFullPath(modEntry.Path+"\\EXTENDED_INSTALL.bat");
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.WorkingDirectory = modEntry.Path;
                var process = new Process();
                process.StartInfo = psi;
                process.Start();
                Application.Quit(0);
            }
            harmony = new Harmony(modEntry.Info.Id);
            applyGameStateListenerPatches(harmony);
            if (Config.GetValueAsBool("nuclear_barrage_related_configurations_enabled"))
            {
                var original = typeof(TIRegionState).GetMethod("ApplyDamageToRegion");
                var prefix = typeof(ApplyDamageToRegionPatch).GetMethod("Prefix");
                var postfix = typeof(ApplyDamageToRegionPatch).GetMethod("Postfix");
                harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
            }
            
            if (Config.GetValueAsBool("faction_related_configurations_enabled"))
            {
                var original = typeof(TIFactionState).GetMethod("GetMissionControlContributionFromHabs");
                var postfix = typeof(MissionControlContributionFromHabs_Patch).GetMethod("GetMissionControlContributionFromHabs_Postfix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));

                var original2 = typeof(TIHabModuleTemplate).GetMethod("MonthlyResourceIncome");
                var prefix2 = typeof(MissionControlContributionFromHabs_Patch).GetMethod("MonthlyResourceIncome_Prefix");
                harmony.Patch(original2, new HarmonyMethod(prefix2));
            }

            if (Config.GetValueAsBool("agent_follow_up_success_failure_modifiers_enabled"))
            {
                TIMissionResolutionPatch.setConfigVariables();
                var original2 = typeof(TIMissionResolution_Contested).GetMethod("GetAllModifiers");
                var postfix2 = typeof(TIMissionResolutionPatch).GetMethod("GetAllModifiers_Postfix");
                harmony.Patch(original2, null, new HarmonyMethod(postfix2));

                TIMissionStatePatch.setConfigVariables();
                var original6 = typeof(TIMissionState).GetMethod("ResolveMission");
                var postfix6 = typeof(TIMissionStatePatch).GetMethod("ResolveMission_Postfix");
                harmony.Patch(original6, null, new HarmonyMethod(postfix6));
            }

            if (Config.GetValueAsBool("mission_related_slider_additions_enabled"))
            {
                var original = typeof(AssignCouncilorToMission).GetMethod("Execute");
                var postfix = typeof(AssignCouncilorToMission_Patch).GetMethod("Execute_MissionSliderAdjustments_PostFix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));
    
                var original2 = typeof(TICouncilorState).GetMethod("GetPossibleMissionList");
                var postfix2 = typeof(GetPossibleMissionList_Patch).GetMethod("GetPossibleMissionList_MissionSliderAdjustments_Postfix");
                harmony.Patch(original2, null, new HarmonyMethod(postfix2));
            }

            if (Config.GetValueAsBool("agent_attributes_alterations_enabled"))
            {
                TICouncilorState_RandomizeStats_Patch.setConfigVariables();
                var original = typeof(TICouncilorState).GetMethod("RandomizeStats");
                var prefix = typeof(TICouncilorState_RandomizeStats_Patch).GetMethod("Prefix");
                harmony.Patch(original, new HarmonyMethod(prefix));

                TICouncilorState_HireRecruitCost_Patch.setConfigVariables();
                var original2 = typeof(TICouncilorState).GetMethod("HireRecruitCost");
                var prefix2 = typeof(TICouncilorState_HireRecruitCost_Patch).GetMethod("Prefix");
                harmony.Patch(original2, new HarmonyMethod(prefix2));
            }
            
            /*if (Config.GetValueAsBool("debt_feature"))
            {
                var original = typeof(TIFactionState).GetMethod("GetMissionControlContributionFromHabs");
                var postfix = typeof(MissionControlContributionFromHabs_Patch).GetMethod("GetMissionControlContributionFromHabs_Postfix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));

                var original2 = typeof(TIHabModuleTemplate).GetMethod("MonthlyResourceIncome");
                var prefix2 = typeof(MissionControlContributionFromHabs_Patch).GetMethod("MonthlyResourceIncome_Prefix");
                harmony.Patch(original2, new HarmonyMethod(prefix2));
            }*/
            
            if (Config.GetValueAsBool("resource_depletion_enabled"))
            {
                var original = typeof(TIHabSiteState).GetMethod("RandomizeSiteMiningData");
                var postfix = typeof(TIHabSiteStateRandomizeSiteMiningDataPatch).GetMethod("RandomizeSiteMiningData_Postfix");
                harmony.Patch(original, null, new HarmonyMethod(postfix));

                var original2 = typeof(TIFactionState).GetMethod("AddToCurrentResource");
                var postfix2 = typeof(AddToCurrentResource_Patch).GetMethod("AddToCurrentResourcePostfix");
                harmony.Patch(original2, null,new HarmonyMethod(postfix2));
            }
            
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        public static void ApplyAdditionalPatchesAffectingGameAssets()
        {
            //var original5 = typeof(MarkerController).GetMethod("SetCPImages");
            //var prefix5 = typeof(MarketControllerSetCPImages_Replace).GetMethod("SetCPImages_MarkerController");
            //Main._harmony.Patch(original5, new HarmonyMethod(prefix5));
        }

        private static void PrintOutAllGameAssemblyMethods()
        {
            foreach (var method in typeof(TIFactionState).GetMethods())
            {
                try
                {
                    var parameters = method.GetParameters();
                    var parameterDescriptions = string.Join(", ", method.GetParameters()
                        .Select(x => x.ParameterType + " " + x.Name)
                        .ToArray());

                    Console.WriteLine("{0} {1} ({2})",
                        method.ReturnType,
                        method.Name,
                        parameterDescriptions);
                } catch(Exception e) {}
            }
        }

        private static void applyGameStateListenerPatches(Harmony harmony)
        {
            var clearGameDataOriginal = typeof(ViewControl).GetMethod("ClearGameData");
            var clearGameDataPrefix = typeof(LoadGameOrStartGame_Patch).GetMethod("ClearGameData_Prefix");
            harmony.Patch(clearGameDataOriginal, new HarmonyMethod(clearGameDataPrefix));
            
            var assetLoaderInitializeOriginal = typeof(AssetLoader).GetMethod("Initialize");
            var assetLoaderInitializePrefix = typeof(LoadGameOrStartGame_Patch).GetMethod("AssetLoad_Initialize_Prefix");
            harmony.Patch(assetLoaderInitializeOriginal, new HarmonyMethod(assetLoaderInitializePrefix));

            var startMissionPhaseOriginal = typeof(CouncilorMissionCanvasController).GetMethod("StartMissionPhase");
            var startMissionPhasePrefix = typeof(StartMissionPhase_Patch).GetMethod("Prefix");
            harmony.Patch(startMissionPhaseOriginal, new HarmonyMethod(startMissionPhasePrefix));
        }

        public static void logDebug(string log)
        {
            if (Config.IsDebugModeActive())
            {
                Console.WriteLine("TI_General_Adjustments_Alterations: " + log);
            }
        }
        
        
    }
}