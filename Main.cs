using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Actions;
using PavonisInteractive.TerraInvicta.Systems.Bootstrap;
using TI_Augmenter.augmentations.harmonypatches.missionrelated;
using TI_Augmenter.augmentations.harmonypatches.nationstate;
using TI_Augmenter.augmentations.harmonypatches.regionstate;
using TI_Augmenter.augmentations.harmonypatches;
using TI_Augmenter.augmentations.harmonypatches.councilorstate;
using TI_Augmenter.augmentations.harmonypatches.factionstate;
using TI_Augmenter.augmentations.harmonypatches.habsitestate;
using UnityEngine;
using UnityModManagerNet;
// ReSharper disable All

namespace TI_Augmenter
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        private static Harmony harmony = null;
        public static bool MustInitializeResourcePoolData = false;

        public static bool Load(UnityModManager.ModEntry modEntry) {
            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            Config.LoadValues();
            if (Config.IsDebugModeActive())
            {
                //PrintOutAllGameAssemblyMethods();
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
                psi.FileName = Path.GetFullPath( modEntry.Path+"\\EXTENDED_INSTALL.bat");
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.WorkingDirectory =  modEntry.Path;
                var process = new Process();
                process.StartInfo = psi;
                process.Start();
                Application.Quit(0);
            }
            harmony = new Harmony(modEntry.Info.Id);
            applyUniversalGameStateListenerPatches(harmony);
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
                
                var original3 = typeof(GameControl).GetMethod("Initialize");
                var postfix3 = typeof(TIHabSiteStateRandomizeSiteMiningDataPatch).GetMethod("InitializeGamePostfix");
                harmony.Patch(original3, null,new HarmonyMethod(postfix3));
            }

            if (Config.GetValueAsBool("remove_control_point_permanently_on_abandon_nation"))
            {
                var original = typeof(TINationState).GetMethod("SelfDisableControlPoints");
                var prefix = typeof(TINationStatePermanentlyRemoveControlPointPatch).GetMethod("SelfDisableControlPointsPrefix");
                harmony.Patch(original, new HarmonyMethod(prefix), null);
                
                var startMissionPhaseOriginal = typeof(CouncilorMissionCanvasController).GetMethod("StartMissionPhase");
                var startMissionPhasePostfix = typeof(StartMissionPhase_Patch).GetMethod("Postfix");
                harmony.Patch(startMissionPhaseOriginal, null, new HarmonyMethod(startMissionPhasePostfix));
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

        private static void applyUniversalGameStateListenerPatches(Harmony harmony)
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
            
            var saveAllGameStatesOriginal = typeof(GameStateManager).GetMethod("SaveAllGameStates");
            var saveAllGameStatesPrefix = typeof(LoadGameOrStartGame_Patch).GetMethod("SaveAllGameStatesPrefix");
            harmony.Patch(saveAllGameStatesOriginal, new HarmonyMethod(saveAllGameStatesPrefix));

            var loadAllGameStatesOriginal = typeof(GameStateManager).GetMethod("LoadAllGameStates");
            var loadAllGameStatesPrefix = typeof(LoadGameOrStartGame_Patch).GetMethod("LoadAllGameStatesPrefix");
            harmony.Patch(loadAllGameStatesOriginal, new HarmonyMethod(loadAllGameStatesPrefix));
        }

        public static void logDebug(string log)
        {
            if (Config.IsDebugModeActive())
            {
                Console.WriteLine("TI_Augmenter: " + log);
            }
        }
        
        private static float ValueAsFloat(String valueAsString)
        {
            float result;
            if (!float.TryParse(valueAsString, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                throw new InvalidCastException("could not cast value to float");
            }
            return result;
        }
        
        
        public static IDictionary<String, ResourceSiteTotalInfo> LoadResourcePoolData(string safeFilePath) {
            // DEFAULT MODIFICATIONS START
            String currentAssemblyFullPathDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // load configurations

            if (!File.Exists(safeFilePath + "_resource_pool_data.txt"))
            {
                return null;
            }
            
            Dictionary<string,ResourceSiteTotalInfo> resourcePoolInfo = File.ReadAllLines(safeFilePath+"_resource_pool_data.txt")
                .Select(x => x.Split('='))
                .ToDictionary(x => x[0], x =>
                {
                    ResourceSiteTotalInfo resourceSiteTotalInfo = new ResourceSiteTotalInfo(ValueAsFloat(x[1]));
                    return resourceSiteTotalInfo;
                });

            return resourcePoolInfo;
        }
        
        public static void SaveResourcePoolData(string safeFilePath, IDictionary<string, ResourceSiteTotalInfo> resourcePoolInfo)
        {
            if (resourcePoolInfo == null || resourcePoolInfo.Count == 0)
                return;

            string currentAssemblyFullPathDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            List<string> lines = new List<string>();
            foreach (var kvp in resourcePoolInfo)
            {
                string line = $"{kvp.Key}={kvp.Value.getTotalRemaining().ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                lines.Add(line);
            }

            File.WriteAllLines(safeFilePath + "_resource_pool_data.txt", lines);
        }
    }
}