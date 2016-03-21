// (c) Copyright Cleverous 2015. All rights reserved.

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Deftly
{
    public class UiCharacter : MonoBehaviour
    {
        public bool LogDebug = false;
        public GameObject RestartPanel;
        public Subject Subject;

        public Text WeaponName;
        public Image WeaponImage;
        public Image WeaponAmmoBar;
        public Text WeaponMagazines;
        public Text WeaponDamage;

        public Image HealthImage;
        public Text HealthText;

        public Text ArmorText;
        public Image ArmorBar;

        public Sprite NoWeapon;
        
        [Space] [Space] 
        public Text DmgTaken;
        public Text DmgDealt;
        public Text Misses;
        public Text Hits;
        public Text Kills;
        public Text XpCount;
        public Image XpFillBar;
        
        [Space] [Space]
        public Text Agi;
        public Text Dex;
        public Text End;
        public Text Str;
        public Text Level;

        [Space] [Space]
        public Text Score;
        public Text Coin;

        private Weapon _thisWeapon;
        private Projectile _projectile;

        void Start()
        {
            if (Subject == null) return;
            StartCoroutine(UpdateScoring());
            // Subscribe for weapon events
            Subject.OnSwitchedWeapon += UpdateWeapon;
            Subject.OnReload += UpdateWeaponMags;
            Subject.OnFire += UpdateWeaponAmmo;
            Subject.OnHealthChanged += UpdateVitals;
            Subject.OnGetPowerup += UpdateAllData;
            Subject.OnLevelChange += UpdateVitals;
            Subject.OnLevelChange += UpdateStatInfo;
            UpdateVitals();
            UpdateWeapon(Subject.GetCurrentWeaponGo());
        }

        void Update()
        {
            if (Subject != null && Subject.IsDead && RestartPanel != null)
            {
                RestartPanel.SetActive(true);
            }
        }
        void UpdateWeapon(GameObject weapon)
        {
            if (LogDebug) Debug.Log("Character UI reloaded the weapon...");
            if (weapon == null)
            {
                WeaponName.text = "none";
                WeaponDamage.text = "0";
                WeaponImage.sprite = NoWeapon;
                WeaponMagazines.text = "0";
                WeaponAmmoBar.fillAmount = 0f;
                return;
            }
            _thisWeapon = weapon.GetComponent<Weapon>();
            if (_thisWeapon.Stats.WeaponType == WeaponType.Ranged)
            {
                _projectile = _thisWeapon.Stats.FiresProjectile.GetComponent<Projectile>();
                UpdateWeaponMags();
            }
            UpdateWeaponMisc();
        }
        void UpdateWeaponMisc()                 // update Weapon Name, Damage, Sprite
        {
            if (LogDebug) Debug.Log("Character UI updated title and damage...");
            if (WeaponName != null)
            {
                WeaponName.text = _thisWeapon.Stats.Title;
            }

            if (WeaponDamage != null)
            {
                WeaponDamage.text = _thisWeapon.Stats.WeaponType == WeaponType.Melee 
                    ? _thisWeapon.Attacks.Aggregate("", (current, atk) => current + (+ atk.Damage + "/")) 
                    : _projectile.Stats.Damage.ToString();
            }

            if (WeaponImage != null && _thisWeapon != null && _thisWeapon.Stats.UiImage != null)
            {
                WeaponImage.sprite = _thisWeapon.Stats.UiImage;
            }
        }
        void UpdateWeaponAmmo(Subject unused)   // update Weapon Ammo 
        {
            // unused argument to satisfy callback requirements
            if (LogDebug) Debug.Log("Character UI reloaded the AMMO..");
            if (WeaponAmmoBar != null)
            {
                WeaponAmmoBar.fillAmount = (float)_thisWeapon.Stats.CurrentAmmo / _thisWeapon.Stats.MagazineSize;
            }
        }
        void UpdateWeaponMags()                 // update Weapon Magazines
        {
            UpdateWeaponAmmo(null);

            if (WeaponMagazines != null)
            {
                WeaponMagazines.text = _thisWeapon.Stats.CurrentMagazines.ToString();
            }
        }
        void UpdateVitals()                      // update Health and Armor
        {
            if (LogDebug) Debug.Log("Character UI reloaded the AMMO..");
            if (HealthImage != null)
            {
                HealthImage.fillAmount = Subject.Health / Subject.Stats.Health.Max;
            }
            if (HealthText != null)
            {
                HealthText.text = Subject.Health.ToString();
            }
            if (ArmorText != null)
            {
                ArmorText.text = Subject.Armor.ToString();
            }
            if (ArmorBar != null)
            {
                ArmorBar.fillAmount = Subject.Armor / Subject.Stats.Armor.Max;
            }
        }

        private void UpdateStatInfo()
        {
            if (Agi) Agi.text = ((int)Subject.Stats.Agility.Actual).ToString();
            if (Dex) Dex.text = ((int)Subject.Stats.Dexterity.Actual).ToString();
            if (End) End.text = ((int)Subject.Stats.Endurance.Actual).ToString();
            if (Str) Str.text = ((int)Subject.Stats.Strength.Actual).ToString();
        }

        void UpdateAllData()                    // update Health, Armor, Ammo & Mags
        {
            UpdateVitals();
            UpdateWeaponAmmo(null);
            UpdateWeaponMags();
        }

        // irregular, because why not?
        public IEnumerator UpdateScoring()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                if (DmgTaken) DmgTaken.text = Subject.Stats.DamageTaken.ToString();
                if (DmgDealt) DmgDealt.text = Subject.Stats.DamageDealt.ToString();
                if (Misses) Misses.text = "N/A";
                if (Hits) Hits.text = Subject.Stats.ShotsConnected.ToString();
                if (Kills) Kills.text = Subject.Stats.Kills.ToString();
                if (XpCount) XpCount.text = (Subject.Stats.Experience.Actual + " / " + Subject.Stats.Experience.Max);
                if (XpFillBar) XpFillBar.fillAmount = Subject.Stats.Experience.Actual/Subject.Stats.Experience.Max;
                if (Level) Level.text = ((int)Subject.Stats.Level.Actual).ToString();
                if (Coin) Coin.text = Subject.Stats.Coin.ToString();
                //if (Score) Score.text = Subject.Stats.Score.ToString();
            }
        }
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}