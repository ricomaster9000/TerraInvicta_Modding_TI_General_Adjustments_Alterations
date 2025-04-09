using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;

namespace TI_General_Adjustments_Alterations
{
    public static class Config
    {
        private static Dictionary<String, Object> _configValues = new Dictionary<string, object>();
        public static bool GameStartedOrLoaded = false;
        public static bool GameAssetsLoaded = false;
        public static void LoadValues() {
            // DEFAULT MODIFICATIONS START
            String currentAssemblyFullPathDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            _configValues.Add("enable_debug_mode", false);

            // nuclear barrage related configurations
            _configValues.Add("nuclear_barrage_related_configurations_enabled", true); // 15-30% increase
            _configValues.Add("nuclear_GDP_damage_to_target_nation_multiplier", 2.00f); // 15-30% increase
            _configValues.Add("nuclear_population_damage_to_target_nation_multiplier", 2.00f); // 15-30% increase
            _configValues.Add("nuclear_GDP_damage_global_multiplier", 1.50f); // 8-20% decrease
            _configValues.Add("nuclear_GDP_damage_global_because_of_core_region_multiplier", 1.50f);

            // nation related configurations
            _configValues.Add("nation_related_configurations_enabled", false);

            // faction related configurations
            _configValues.Add("faction_related_configurations_enabled", true);
            _configValues.Add("faction_hab_mission_point_generation_multiplier", 4.0f);

            // agent related success&failure chance modifiers
            _configValues.Add("agent_follow_up_success_failure_modifiers_enabled", true);
            _configValues.Add("agent_follow_up_successes_decrease_chances_of_success_max_count", 3); // stop decreasing at 60% (BASE GAME does not support this feature yet, added by mod)
            _configValues.Add("agent_follow_up_successes_decrease_chances_of_success_factor", 0.05f); // 20% increase (BASE GAME does not support this feature yet, added by mod)
            _configValues.Add("agent_follow_up_failures_decrease_chances_of_success_max_count", 5); // stop decreasing at 100% (BASE GAME does not support this feature yet, added by mod)
            _configValues.Add("agent_follow_up_failures_decrease_chances_of_success_factor", 0.10f); // 20% increase (BASE GAME does not support this feature yet, added by mod)

            // mission related slider additions
            _configValues.Add("mission_related_slider_additions_enabled", true);
            _configValues.Add("mission_related_slider_operations_boost_modifier", 0.33f);
            
            // agent traits&stats enhancers and max random adjustments
            _configValues.Add("agent_attributes_alterations_enabled", true);
            _configValues.Add("agent_attributes_all_range_recruit_pool_modifier", 3.00f);
            _configValues.Add("agent_attributes_investigation_range_recruit_pool_modifier", 1.50f);
            _configValues.Add("agent_attributes_cost_factor_multiplier", 2.00f);
            
            // resource depletion
            _configValues.Add("resource_depletion_enabled", true);
            
            // Remove-Control-Points-On-Abandon-Nation
            _configValues.Add("remove_control_point_permanently_on_abandon_nation", true);

            // DEFAULT MODIFICATIONS END

            _configValues.Add("extended_installation_completed", false);

            // load configurations
            Dictionary<string,string> configFileData = File.ReadAllLines(currentAssemblyFullPathDirectory+"\\TI_General_Adjustments_Alterations_Config.txt")
                .Select(x => x.Split('='))
                .ToDictionary(x => x[0], x => x[1]);
            
            if (configFileData == null)
            {
                throw new Exception("TI_General_Adjustments_Alterations.txt must be set in mod folder");
            }
            AddDataToConfigurationsList(configFileData);

            if (File.Exists(currentAssemblyFullPathDirectory + "\\Extended_Install_Configurations.txt"))
            {
                using (StreamReader r = new StreamReader(currentAssemblyFullPathDirectory + "\\Extended_Install_Configurations.txt"))
                {
                    string json = r.ReadToEnd();
                    var jsonConfigValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (jsonConfigValues != null)
                    {
                        AddDataToConfigurationsList(jsonConfigValues);
                    }
                }
            }
        }

        private static void AddDataToConfigurationsList(Dictionary<String, string> configFileData)
        {
            foreach (KeyValuePair<string, string> entry in configFileData)
            {
                Console.WriteLine("set configuration param: key - " + entry.Key + " value " + entry.Value + " valuetype " + entry.Value.GetType());
                int newIntVal;
                float newFloatVal;
                Boolean newBoolVal;
                if (int.TryParse(entry.Value, out newIntVal) && !entry.Value.Contains("."))
                {
                    Console.WriteLine("configuration param added as int");
                    _configValues[entry.Key] = newIntVal;
                }
                else if (float.TryParse(entry.Value,NumberStyles.Float, CultureInfo.InvariantCulture, out newFloatVal))
                {
                    Console.WriteLine("configuration param added as float");
                    _configValues[entry.Key] = newFloatVal;
                }
                if (Boolean.TryParse(entry.Value, out newBoolVal))
                {
                    Console.WriteLine("configuration param added as boolean");
                    _configValues[entry.Key] = newBoolVal;
                }
                else
                {
                    _configValues[entry.Key] = entry.Value;
                }
            }
        }

        private static Dictionary<string, object> getConfigValues()
        {
            return _configValues;
        }

        public static float GetValueAsFloat(String jsonConfigKeyName)
        {
            float result;
            if (!float.TryParse(getConfigValues().GetValueSafe(jsonConfigKeyName).ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                throw new InvalidCastException("could not cast value to float");
            }
            return result;
        }

        public static int GetValueAsInt(String jsonConfigKeyName)
        {
            return Int32.Parse(getConfigValues().GetValueSafe(jsonConfigKeyName).ToString());
        }
        
        public static bool GetValueAsBool(String jsonConfigKeyName)
        {
            return Boolean.Parse(getConfigValues().GetValueSafe(jsonConfigKeyName).ToString());
        }

        public static bool IsDebugModeActive()
        {
            return (bool)getConfigValues().GetValueSafe("enable_debug_mode");
        }
        
        public static bool isKeySet(string key)
        {
            return getConfigValues().ContainsKey(key);
        }
        
    }
}