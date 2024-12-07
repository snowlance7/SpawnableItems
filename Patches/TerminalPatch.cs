using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using static SpawnableItems.Plugin;

namespace SpawnableItems.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Terminal.Awake))]
        private static void AwakePostfix(Terminal __instance)
        {
            if (IsServerOrHost)
            {
                if (SpawnableItemsList.Count != 0) { return; }
                LoggerInstance.LogDebug("Getting items...");
                foreach (var item in __instance.buyableItemsList)
                {
                    ConfigEntry<int> itemWeight = PluginInstance.Config.Bind<int>("Item Weights", item.itemName, 25, $"Spawn weight of {item.itemName} used in spawning item in dungeon");

                    SpawnableItemWithRarity spawnableItem = new();
                    spawnableItem.spawnableItem = item;
                    spawnableItem.rarity = itemWeight.Value;

                    SpawnableItemsList.Add(spawnableItem);
                }

                if (configIncludeDefensiveItems.Value)
                {
                    foreach (var item in StartOfRound.Instance.allItemsList.itemsList)
                    {
                        if (!item.isDefensiveWeapon) { continue; }
                        ConfigEntry<int> itemWeight = PluginInstance.Config.Bind<int>("Item Weights", item.itemName, 25, $"Spawn weight of {item.itemName} used in spawning item in dungeon. This is a defensive item.");

                        SpawnableItemWithRarity spawnableItem = new();
                        spawnableItem.spawnableItem = item;
                        spawnableItem.rarity = itemWeight.Value;

                        if (SpawnableItemsList.Contains(spawnableItem)) { continue; }
                        SpawnableItemsList.Add(spawnableItem);
                    }
                }
            }
        }
    }
}
