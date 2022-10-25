using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using Poly2Tri;
using UnityModManagerNet;

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
            _configValues.Add("nuclear_GDP_damage_to_target_nation_factor", 1.15f); // 15-30% increase
            _configValues.Add("nuclear_GDP_damage_to_all_nations_if_above_certain_million_deaths_factor", 0.85f); // 8-20% decrease

            // nation related configurations
            _configValues.Add("nation_related_configurations_enabled", false);

            // faction related configurations
            _configValues.Add("faction_related_configurations_enabled", true);
            _configValues.Add("faction_hab_mission_point_generation_multiplier", 4.0f);

            // agent related success&failure chance modifiers
            _configValues.Add("agent_follow_up_success_failure_modifiers_enabled", true);
            _configValues.Add("agent_follow_up_successes_decrease_chances_of_success_max_count", 3); // stop decreasing at 60% (BASE GAME does not support this feature yet, added by mod)
            _configValues.Add("agent_follow_up_successes_decrease_chances_of_success_factor", 0.10f); // 20% increase (BASE GAME does not support this feature yet, added by mod)
            _configValues.Add("agent_follow_up_failures_decrease_chances_of_success_max_count", 5); // stop decreasing at 100% (BASE GAME does not support this feature yet, added by mod)
            _configValues.Add("agent_follow_up_failures_decrease_chances_of_success_factor", 0.15f); // 20% increase (BASE GAME does not support this feature yet, added by mod)

            // mission related slider additions
            _configValues.Add("mission_related_slider_additions_enabled", true);
            _configValues.Add("mission_related_slider_operations_boost_modifier", 0.33f);

            // DEFAULT MODIFICATIONS END

            _configValues.Add("extended_installation_completed", false);

            // load JSON configurations
            using (StreamReader r = new StreamReader(currentAssemblyFullPathDirectory+"\\TI_General_Adjustments_Alterations_Config.txt")) {
                string json = r.ReadToEnd();
                var jsonConfigValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (jsonConfigValues == null)
                {
                    throw new Exception("TI_General_Adjustments_Alterations.json must be set in mod folder");
                }
                AddJsonDataToConfigurationsList(jsonConfigValues);
            }

            if (File.Exists(currentAssemblyFullPathDirectory + "\\Extended_Install_Configurations.txt"))
            {
                using (StreamReader r = new StreamReader(currentAssemblyFullPathDirectory + "\\Extended_Install_Configurations.txt"))
                {
                    string json = r.ReadToEnd();
                    var jsonConfigValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    if (jsonConfigValues != null)
                    {
                        AddJsonDataToConfigurationsList(jsonConfigValues);
                    }
                }
            }
        }

        private static void AddJsonDataToConfigurationsList(Dictionary<String, object> jsonData)
        {
            foreach (KeyValuePair<string, object> entry in jsonData)
            {
                Console.WriteLine("set json configuration param: key - " + entry.Key + " value " + entry.Value + " valuetype " + entry.Value.GetType());
                short newShortVal;
                int newIntVal;
                float newFloatVal;
                DateTime newDateVal;
                if (DateTime.TryParse(entry.Value.ToString(), out newDateVal))
                {
                    Console.WriteLine("json configuration param added as DateTime");
                    _configValues[entry.Key] = newDateVal;
                }
                else if (short.TryParse(entry.Value.ToString(), out newShortVal) && !entry.Value.ToString().Contains("."))
                {
                    Console.WriteLine("json configuration param added as short");
                    _configValues[entry.Key] = newShortVal;
                }
                else if (Int32.TryParse(entry.Value.ToString(), out newIntVal) && !entry.Value.ToString().Contains("."))
                {
                    Console.WriteLine("json configuration param added as int");
                    _configValues[entry.Key] = newIntVal;
                } 
                else if (float.TryParse(entry.Value.ToString(), out newFloatVal))
                {
                    Console.WriteLine("json configuration param added as float");
                    _configValues[entry.Key] = newFloatVal;
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

        public static T GetValue<T>(String jsonConfigKeyName)
        {
            return (T) getConfigValues().GetValueSafe(jsonConfigKeyName);
        }
        
        public static float GetValueAsFloat(String jsonConfigKeyName)
        {
            return float.Parse(getConfigValues().GetValueSafe(jsonConfigKeyName).ToString());
        }

        public static short GetValueAsShort(String jsonConfigKeyName)
        {
            return short.Parse(getConfigValues().GetValueSafe(jsonConfigKeyName).ToString());
        }

        public static bool IsDebugModeActive()
        {
            return (bool)getConfigValues().GetValueSafe("enable_debug_mode");
        }
        
    }
}