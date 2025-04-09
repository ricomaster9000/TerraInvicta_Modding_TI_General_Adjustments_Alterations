using System;
using System.Collections.Generic;
using AssetBundles;
using PavonisInteractive.TerraInvicta;
using TI_Augmenter.augmentations.harmonypatches.habsitestate;

namespace TI_Augmenter.augmentations.harmonypatches
{
    public class LoadGameOrStartGame_Patch
    {

        public static bool MustInitializeResourcePoolData = false;
        public static bool ClearGameData_Prefix()
        {
            Config.GameStartedOrLoaded = true;
            Console.WriteLine("Game Loaded or New Game started");
            // load resource totals
            
            return true;
        }

        public static bool AssetLoad_Initialize_Prefix()
        {   
            Log.Time("AssetBundleManager Initialize", new Action(AssetBundleManager.Initialize));
            Config.GameAssetsLoaded = true;
            Console.WriteLine("Game Assets fully loaded");
            Main.ApplyAdditionalPatchesAffectingGameAssets();
            return false;
        }

        public static bool SaveAllGameStatesPrefix(string filepath, bool doNotOpenSaveMenu = false)
        {
            if (Config.GetValueAsBool("resource_depletion_enabled"))
            {
                Main.logDebug("Resource Depletion -> saving resource pool data");
                Main.SaveResourcePoolData(filepath, TIHabSiteStateRandomizeSiteMiningDataPatch.HabSiteToTotalResources);
            }
            return true;
        }
        
        public static bool LoadAllGameStatesPrefix(string filepath)
        {
            if (Config.GetValueAsBool("resource_depletion_enabled"))
            {
                Main.logDebug("Resource Depletion -> loading resource pool data");
                IDictionary<string, ResourceSiteTotalInfo> resourcePoolData = Main.LoadResourcePoolData(filepath);
                if (resourcePoolData == null)
                {
                    Main.MustInitializeResourcePoolData = true;
                }
                else
                {
                    TIHabSiteStateRandomizeSiteMiningDataPatch.HabSiteToTotalResources = resourcePoolData;
                }
            }
            return true;
        }
    }
}