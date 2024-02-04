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

namespace SpawnableItems
{
    [HarmonyPatch]
    internal static class TerminalPatch
    {
        private static readonly ManualLogSource LoggerInstance = SpawnableItemsBase.LoggerInstance;

        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        private static void GetItems(Terminal __instance)
        {
            // my method
            LoggerInstance.LogInfo($"Getting buyable items...");
            List<Item> buyableItems = __instance.buyableItemsList.ToList();
            LoggerInstance.LogDebug($"Got {buyableItems.Count} buyable items...");

            foreach (Item item in buyableItems)
            {
                LoggerInstance.LogDebug(item.itemName);
            }

            // other method
            LoggerInstance.LogInfo($"Getting all non scrap items...");
            List<SpawnableItemWithRarity> nonScrapItems = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => !item.isScrap && (bool)item.spawnPrefab).Select(SetSpawnableItemWithRarity).ToList();
            LoggerInstance.LogDebug($"Got {nonScrapItems.Count} non scrap items...");
            foreach (SpawnableItemWithRarity item in nonScrapItems)
            {
                LoggerInstance.LogDebug(item.spawnableItem.itemName);
            }

            LoggerInstance.LogInfo($"Getting all items...");
            List<SpawnableItemWithRarity> allItems = StartOfRound.Instance.allItemsList.itemsList.Select(SetSpawnableItemWithRarity).ToList();
            LoggerInstance.LogDebug($"Got {allItems.Count} items...");
            foreach (SpawnableItemWithRarity item in allItems)
            {
                LoggerInstance.LogDebug(item.spawnableItem.itemName + " " + item.rarity);
            }

            /*for (int k = 0; k < Terminal.terminalNodes.allKeywords.Length; k++)
            {
                TerminalKeyword keyword = Terminal.terminalNodes.allKeywords[k];
                if (!(keyword != null) || keyword.compatibleNouns == null)
                {
                    continue;
                }
                for (int m = 0; m < keyword.compatibleNouns.Length; m++)
                {
                    CompatibleNoun noun = keyword.compatibleNouns[m];
                    if (!(noun.result != null))
                    {
                        continue;
                    }
                    if (noun.result.buyItemIndex >= 0)
                    {
                        if (noun.result.buyItemIndex < Terminal.buyableItemsList.Length)
                        {
                            Item item2 = Terminal.buyableItemsList[noun.result.buyItemIndex];
                            if (item2 == null)
                            {
                                Plugin.Log.LogWarning((object)("The item " + noun.noun.word + "(" + noun.result.name + ") is null? This is unexpected behaviour. Item wont be added."));
                                continue;
                            }
                            if (!BuyableItems.Contains(item2))
                            {
                                Plugin.Log.LogMessage((object)("Found buyable item " + item2.itemName));
                                BuyableItems.Add(item2);
                            }
                            if (!AllItems.Contains(item2))
                            {
                                AllItems.Add(item2);
                            }
                        }
                        else
                        {
                            Plugin.Log.LogWarning((object)("The item " + noun.noun.word + "(" + noun.result.name + ") wasn't added to buyableItemsList. This is unexpected behaviour. Item wont be added."));
                        }
                    }
                    else
                    {
                        if (noun.result.shipUnlockableID < 0 || StartOfRound.Instance.unlockablesList.unlockables.Count <= noun.result.shipUnlockableID)
                        {
                            continue;
                        }
                        UnlockableItem unlockable2 = StartOfRound.Instance.unlockablesList.unlockables[noun.result.shipUnlockableID];
                        UnlockablePrices[unlockable2] = noun.result.itemCost;
                        if (unlockable2.alwaysInStock)
                        {
                            if (!ShipUpgrades.Contains(unlockable2))
                            {
                                Plugin.Log.LogMessage((object)("Found unlockable ship upgrade " + unlockable2.unlockableName));
                                ShipUpgrades.Add(unlockable2);
                            }
                        }
                        else if (!Decorations.Contains(unlockable2))
                        {
                            Plugin.Log.LogMessage((object)("Found unlockable ship decoration " + unlockable2.unlockableName));
                            Decorations.Add(unlockable2);
                        }
                    }
                }
            }*/
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        public static void SpawnStoreItemsInsideFactory(RoundManager __instance)
        {
            return; // temporary
            SpawnableItemWithRarity[] array = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => !item.isScrap && (bool)item.spawnPrefab).Select(SetSpawnableItemWithRarity).ToArray();
            int[] weights = array.Select((SpawnableItemWithRarity f) => f.rarity).ToArray();
            List<RandomScrapSpawn> list = (from s in UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>()
                                           where !s.spawnUsed
                                           select s).ToList();
            System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed - 7);
            int num = random.Next(SpawnableItemsBase.configMaxItemsToSpawn.Value / 2, SpawnableItemsBase.configMaxItemsToSpawn.Value);
            for (int i = 0; i < num; i++)
            {
                if (list.Count <= 0)
                {
                    break;
                }
                int index = random.Next(0, list.Count);
                RandomScrapSpawn randomScrapSpawn = list[index];
                Vector3 vector = randomScrapSpawn.transform.position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    list.RemoveAt(index);
                }
                else
                {
                    vector = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit);
                }
                int randomWeightedIndex = RoundManager.Instance.GetRandomWeightedIndex(weights, random);
                GameObject gameObject = UnityEngine.Object.Instantiate(array[randomWeightedIndex].spawnableItem.spawnPrefab, vector + Vector3.up * 0.5f, Quaternion.identity, StartOfRound.Instance.propsContainer);
                gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
            }
        }

        private static SpawnableItemWithRarity SetSpawnableItemWithRarity(Item item)
        {
            // temporary test
            SpawnableItemWithRarity spawnableItemWithRarity = new SpawnableItemWithRarity
            {
                rarity = 1,
                spawnableItem = item
            };
            return spawnableItemWithRarity;
            // should check if it has a price or value and set rarity accordingly, otherwise set to average of all item rarities in that level

        }
    }
}
