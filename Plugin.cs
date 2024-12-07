using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks.Ugc;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SpawnableItems
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static Plugin PluginInstance;
        public static ManualLogSource LoggerInstance { get; private set; }
        public static bool IsServerOrHost { get { return NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost; } }

        public static List<SpawnableItemWithRarity> SpawnableItemsList = [];

        public static ConfigEntry<bool> configShouldScrapSpawn;
        public static ConfigEntry<bool> configUseVanillaSpawning;
        public static ConfigEntry<bool> configIncludeDefensiveItems;
        public static ConfigEntry<int> configMinItemsToSpawn;
        public static ConfigEntry<int> configMaxItemsToSpawn;

        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;

            harmony.PatchAll();

            configShouldScrapSpawn = Config.Bind("General", "Should Scrap Spawn", true, "Should items spawn when scrapping?\nIf set to false, ItemSpawnSequence will default to 'WithScrap' and items will just be added to the loot table");
            configUseVanillaSpawning = Config.Bind("General", "Use Vanilla Spawning", true, "If true, will spawn items based on the levels min/max amount. If false, will use the min/max amount in custom spawning.");
            configIncludeDefensiveItems = Config.Bind("General", "Include Defensive Items", true, "Should defensive items be included in the item spawning? (Yield Sign, Shotgun, Shells, etc.)");
            configMinItemsToSpawn = Config.Bind("Custom Spawning", "Min Items To Spawn", 0, "Minimum number of items to spawn.\nThis is only used when 'Use Vanilla Spawning' is set to false.");
            configMaxItemsToSpawn = Config.Bind("Custom Spawning", "Max Items To Spawn", 25, "Maximum number of items to spawn.\nThis is only used when 'Use Vanilla Spawning' is set to false.");

            LoggerInstance.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} loaded successfully.");
        }
    }
}
