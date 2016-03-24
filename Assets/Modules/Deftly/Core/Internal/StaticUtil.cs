// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    // TODO : POOLING
    // NOTE : Options.Init() is called from DeftlyCamera.Start().

    public static class StaticUtil
    {
        public static void SpawnFloatingText(Object origin, Vector3 position, string value)
        {
            if (Options.Data.UseFloatingText != true) return;
            if (Options.Refs.TextPrefab == null)
            {
                Debug.LogError("Floating Damage Prefab not found! Check Deftly Global Options Floating Text Name and confirm it is in a Resources folder.");
                return;
            }

            GameObject dmg = Object.Instantiate(Options.Refs.TextPrefab);
            FloatingText txt = dmg.GetComponent<FloatingText>();
            if (txt == null)
            {
                Debug.LogError("No Floating Text component found on the Floating Text Prefab! Add one to the prefab.");
                return;
            }
            txt.StartPosition = position;
            txt.Value = value;
        }

        /// <summary> Checks to see if a GameObject is on a layer in a LayerMask. </summary>
        /// <returns> True if the provided LayerMask contains the layer of the GameObject provided. </returns>
        public static bool LayerMatchTest(LayerMask approvedLayers, GameObject objInQuestion)
        {
            return ((1 << objInQuestion.layer) & approvedLayers) != 0;
        }

        public static void SpawnLoot(GameObject prefab, Vector3 location)
        {
            GameObject newLoot = (GameObject)Object.Instantiate(prefab, location + Vector3.up, Quaternion.identity);           
        }

        /// <summary>Wrapper for your pooling solution integration.</summary>
        public static Object Spawn(Object original, Vector3 position, Quaternion rotation)
        {
            // Sweet pooling code here
            return Object.Instantiate(original, position, rotation);
        }
        /// <summary>Wrapper for your pooling solution integration.</summary>
        public static void DeSpawn(Object target)
        {
            // Sweet pooling code here
            Object.Destroy(target);
        }

        public static void GiveXp(int amount, Subject target)
        {
            int val = (int)target.Stats.Experience.Actual + amount;
            if (val >= target.Stats.Experience.Max)
            {
                target.Stats.Experience.Actual += amount;
                target.LevelUp();
            }
            else target.Stats.Experience.Actual += amount;
        }
        public static float StatShouldBe(Stat stat, int level)
        {
            return (stat.Base + stat.IncreasePerLevel) * level;
        }
        public static bool SameTeam(Subject guy, Subject otherGuy)
        {
            return guy.Stats.TeamId == otherGuy.Stats.TeamId;
        }
    }
}