using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SpawnableItems
{
    [HarmonyPatch]
    internal static class TerminalPatch
    {
        private static readonly ManualLogSource LoggerInstance = SpawnableItemsBase.LoggerInstance;

        public static List<SpawnableItemWithRarity> itemsToSpawn;

        /*private static List<SpawnableItemWithRarity> GetDefaultSpawnableItems()
        {
            
        }*/ // might not be needed

        private static string GetSpawnableItemsAsString()
        {
            string result = "";

            foreach (SpawnableItemWithRarity item in itemsToSpawn)
            {
                if (item == itemsToSpawn.Last())
                {
                    result += $"{item.spawnableItem.itemName}:{item.rarity}";
                }
                else
                {
                    result += $"{item.spawnableItem.itemName}:{item.rarity},";
                }
            }
            return result;
        }

        private static List<SpawnableItemWithRarity> GetSpawnableItemsFromString()
        {
            throw new NotImplementedException();
            /*string itemString = SpawnableItemsBase.configItemsToSpawn.Value.Replace(" ", "");
            List<SpawnableItemWithRarity> result = itemString.Split(',')
                .Select(itemString =>
                {
                    var parts = itemString.Split(':');
                    return new Item;
                })
                .ToList();*/ // ERROR TO DO
            /*string itemsString = "apple:1,banana:2,cherry:3";
            List<Item> items = itemsString.Split(',')
                .Select(itemString =>
                {
                    var parts = itemString.Split(':');
                    return new Item { Name = parts[0], Value = int.Parse(parts[1]) };
                })
                .ToList();*/ // TO DO: use this to convert string to list
            // string itemsString = string.Join(",", items.Select(item => $"{item.Name}:{item.Value}")); // TO DO: Use this to convert list to string, may not need?
        }

        [HarmonyPatch(typeof(Terminal), "Awake")]
        [HarmonyPostfix]
        private static void TerminalAwakePostFix() // TO DO: modify to get shotgun and ammo objects as well
        {
            // if configItemsToSpawn is null, set to default
            if (SpawnableItemsBase.configItemsToSpawn == null)
            {
                itemsToSpawn = StartOfRound.Instance.allItemsList.itemsList
                    .Where((Item item) => (!item.isScrap && (bool)item.spawnPrefab) || (SpawnableItemsBase.configIncludeDefensiveItems.Value && item.isDefensiveWeapon))
                    .Select(SetDefaultRarities)
                    .Where(item => item != null) // Filter out null values
                    .ToList();
                LoggerInstance.LogWarning("configItemsToSpawn is null. Setting to default.");
                //SpawnableItemsBase.configItemsToSpawn = SpawnableItemsBase.Instance.Config.Bind("General", "ItemsToSpawn", itemsToSpawn, $"Items to spawn with their rarity.\n{GetSpawnableItemsAsString()}");
            }
            else
            {
                return; // remove this line when implementing the patch
                // TO DO: Set rarities based on configItemsToSpawn
            }
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        public static void SpawnStoreItemsInsideFactory(RoundManager __instance)
        {
            return; // remove this line when implementing the patch
            SpawnableItemWithRarity[] array = StartOfRound.Instance.allItemsList.itemsList.Where((Item item) => !item.isScrap && (bool)item.spawnPrefab).Select(SetDefaultRarities).ToArray();
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

        private static SpawnableItemWithRarity SetDefaultRarities(Item item) // TO DO: main function is to convert item to spawnableitemwithrarity
        {
            if (item.itemName == "Mapper" || item.itemName == "Binoculars") // check if items are mapper or binoculars
            {
                return null; // ERROR Might throw an exception, but it's fine for now
            }

            // temporary test


            SpawnableItemWithRarity spawnableItemWithRarity = new SpawnableItemWithRarity
            {
                rarity = 1,
                spawnableItem = item
            };
            return spawnableItemWithRarity;
            // should check configs and set rarity accordingly otherwise do below
            // should check if it has a price or value and set rarity accordingly, otherwise set to average of all item rarities in that level

        }
    }
}
