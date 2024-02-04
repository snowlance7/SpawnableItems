using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SpawnableItems;
using Unity.Netcode;

namespace SpawnableItems.Patches
{
    [HarmonyPatch]
    internal class RoundManagerPatch
    {
        private static readonly ManualLogSource LoggerInstance = SpawnableItemsBase.LoggerInstance;

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        private static void SpawnScrapInLevelPreFix(RoundManager __instance)
        {
            return; // remove this line when implementing the patch
            if (!SpawnableItemsBase.configShouldScrapSpawn.Value)
            {
                __instance.currentLevel.spawnableScrap.Clear();
            }

            if (SpawnableItemsBase.configItemSpawnSequence.Value == "BeforeScrap")
            {
                // spawn number of items before scrap is spawned
                
            }
            else if (SpawnableItemsBase.configItemSpawnSequence.Value == "WithScrap")
            {
                // add items to spawnablescrap

            }

            //__instance.currentLevel.spawnableScrap.Add(); // use this to add items to spawnablescrap in foreach loop
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        private static void SpawnScrapInLevelPostFix(RoundManager __instance)
        {
            return; // remove this line when implementing the patch
            // spawn number of items after scrap is spawned only if configItemSpawnSequence is "AfterScrap"
        }
    }
}
