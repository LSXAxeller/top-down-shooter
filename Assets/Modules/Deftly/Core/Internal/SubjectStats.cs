// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;

namespace Deftly
{
    public enum SubjectGroup { Player, Intellect, Other };
    public enum ArmorType { Absorb, Nullify };

    [System.Serializable]
    public class SubjectStats
    {
        // Descriptive Information, properties, preferences
        [SerializeField] public SubjectGroup SubjectGroup;
        [SerializeField] public int SubjectSkin;
        [SerializeField] public ArmorType ArmorType;
        [SerializeField] public int TeamId;
        [SerializeField] public string Title;
        [SerializeField] public AudioClip HitSound;
        [SerializeField] public GameObject HitIndicator;
        [SerializeField] public AudioClip DeathSound;
        [SerializeField] public int MaxWeaponSlots;
        [SerializeField] public float CrippledTime;
        [SerializeField] public float CorpseTime;
        [SerializeField] public GameObject DeathFx;
        [SerializeField] public GameObject LevelUpFx;
        [SerializeField] public GameObject SpawnFx;

        [SerializeField] public bool UseMecanim;
        [SerializeField] public GameObject WeaponMountPoint;
        [SerializeField] public GameObject DeadBodyObj;
        [SerializeField] public bool UseWeaponIk;
        [SerializeField] public bool UseLeftHandIk;
        [SerializeField] public bool UseRightHandIk;
        [SerializeField] public float CharacterScale;

        // Real Character Stats
        [SerializeField] public Stat Level;
        [SerializeField] public Stat Experience;
        [SerializeField] public Stat XpReward;

        [SerializeField] public Stat Health;
        [SerializeField] public Stat Armor;
        [SerializeField] public Stat Strength;
        [SerializeField] public Stat Agility;
        [SerializeField] public Stat Dexterity;
        [SerializeField] public Stat Endurance;
        

        // Storage for kills/deaths/damage etc.
        public int Kills;
        public int Deaths;
        public int DamageDealt;
        public int DamageTaken;
        public int ShotsConnected;
        public int ShotsMissed;
        public int Coin;
        public int Score;
    }

    [System.Serializable]
    public class Stat
    {
        public Stat(float sBase, float min, float max, float perLevel, float actual)
        {
            Base = sBase;
            Actual = actual;
            Min = min;
            Max = max;
            IncreasePerLevel = perLevel;
        }
        [SerializeField] public float Base;
        [SerializeField] public float Actual;
        [SerializeField] public float Min;
        [SerializeField] public float Max;
        [SerializeField] public float IncreasePerLevel;
    }
}