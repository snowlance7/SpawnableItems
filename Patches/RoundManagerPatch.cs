using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using static SpawnableItems.Plugin;

namespace SpawnableItems.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(RoundManager.SpawnScrapInLevel))]
        private static void SpawnScrapInLevelPrefix(RoundManager __instance)
        {
            if (IsServerOrHost)
            {
                if (!configShouldScrapSpawn.Value)
                {
                    __instance.currentLevel.spawnableScrap.Clear();
                }

                // Vanilla Spawning
                foreach (var spawnableItem in SpawnableItemsList)
                {
                    if (__instance.currentLevel.spawnableScrap.Contains(spawnableItem)) { continue; }
                    __instance.currentLevel.spawnableScrap.Add(spawnableItem);
                }

                if (configUseVanillaSpawning.Value) { return; }

                int min = Math.Min(configMinItemsToSpawn.Value, configMaxItemsToSpawn.Value);
                int max = Math.Max(configMinItemsToSpawn.Value, configMaxItemsToSpawn.Value);

                __instance.currentLevel.minScrap = min;
                __instance.currentLevel.maxScrap = max;
            }
        }
    }
}
