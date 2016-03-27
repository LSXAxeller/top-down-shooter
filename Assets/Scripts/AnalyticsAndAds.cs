using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsAndAds : MonoBehaviour {

    int totalPotions = 5;
    int totalCoins = 100;
    string weaponID = "Weapon_102";

    private void Start()
    {
        Analytics.CustomEvent("gameOver", new Dictionary<string, object>
        {
            { "potions", totalPotions },
            { "coins", totalCoins },
            { "activeWeapon", weaponID }
        });

        Analytics.Transaction("12345abcde", 0.99m, "USD", null, null);
        
        Gender gender = Gender.Female;
        Analytics.SetUserGender(gender);

        int birthYear = 2014;
        Analytics.SetUserBirthYear(birthYear);
    }
}
