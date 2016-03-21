// (c) Copyright Cleverous 2015. All rights reserved.

using System;
using UnityEngine;

namespace Deftly
{
    public class Powerup : MonoBehaviour
    {        
        // TODO move the clamping management to the targeted classes
        [Flags]
        public enum Target { Health, Armor, Ammo, Magazines }
        public enum ActionType { Add, Subtract, Set }
        public enum PickupAffect { Current, All }

        public int Value;

        public Target TargetStat;
        public ActionType Act;
        public PickupAffect Pickup;
        public LayerMask Mask;
        private GameObject _collector;
         
        private Subject _keeper;

        void OnTriggerEnter(Collider col)
        {
            _collector = col.gameObject;
            if (!StaticUtil.LayerMatchTest(Mask, _collector)) return;
            _keeper = col.GetComponent<Subject>();
            if (_keeper == null) return;

            if (TargetStat == Target.Health)
            {
                switch (Act)
                {
                    case ActionType.Add:
                    {
                        _keeper.Health += Value;
                        break;
                    }
                    case ActionType.Subtract:
                    {
                        _keeper.Health -= Value;
                        break;
                    }
                    case ActionType.Set:
                    {
                        _keeper.Health = Value;
                        break;
                    }
                }
            }

            if (TargetStat == Target.Armor)
            {
                switch (Act)
                {
                    case ActionType.Add:
                        {
                            _keeper.Armor += Value;
                            break;
                        }
                    case ActionType.Subtract:
                        {
                            _keeper.Armor -= Value;
                            break;
                        }
                    case ActionType.Set:
                        {
                            _keeper.Armor = Value;
                            break;
                        }
                }
            }

            if (TargetStat == Target.Ammo)
            {
                if (Pickup == PickupAffect.Current)
                {
                    Weapon w = _keeper.GetCurrentWeaponComponent();
                    AffectWeaponAmmo(w);
                }
                else
                {
                    foreach (GameObject wpn in _keeper.WeaponListRuntime)
                    {
                        Weapon w = wpn.GetComponent<Weapon>();
                        AffectWeaponAmmo(w);
                    }
                }
            }

            if (TargetStat == Target.Magazines)
            {
                if (Pickup == PickupAffect.Current)
                {
                    Weapon w = _keeper.GetCurrentWeaponComponent();
                    AffectWeaponMags(w);
                }
                else
                {
                    foreach (GameObject wpn in _keeper.WeaponListRuntime)
                    {
                        Weapon w = wpn.GetComponent<Weapon>();
                        AffectWeaponMags(w);
                    }
                }
            }

            _keeper.DoGrabPowerup();
            Disappear();
        }
        void AffectWeaponAmmo(Weapon weapon)
        {
            if (weapon.Stats.WeaponType == WeaponType.Melee) return;
            int ammo = Value;
            ammo = Mathf.Clamp(ammo, 0, weapon.Stats.MagazineSize);

            switch (Act)
            {
                case ActionType.Add:
                {
                    weapon.Stats.CurrentAmmo += ammo;
                    break;
                }
                case ActionType.Subtract:
                {
                    weapon.Stats.CurrentAmmo -= ammo;
                    break;
                }
                case ActionType.Set:
                {
                    weapon.Stats.CurrentAmmo = ammo;
                    break;
                }
            }
        }
        void AffectWeaponMags(Weapon weapon)
        {
            if (weapon.Stats.WeaponType == WeaponType.Melee) return;
            int mags = Value;
            mags = Mathf.Clamp(mags, 0, 999);
            switch (Act)
            {
                case ActionType.Add:
                    {
                        weapon.Stats.CurrentMagazines += mags;
                        break;
                    }
                case ActionType.Subtract:
                    {
                        weapon.Stats.CurrentMagazines -= mags;
                        break;
                    }
                case ActionType.Set:
                    {
                        weapon.Stats.CurrentMagazines = mags;
                        break;
                    }
            }
        }
        void Disappear()
        {
            StaticUtil.DeSpawn(gameObject);
        }
    }
}