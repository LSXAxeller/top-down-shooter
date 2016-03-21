using UnityEngine;
using UnityEngine.UI;

public enum Team { CT, T , VIP};

public class Info : MonoBehaviour {

    public bool isSpawnable = true;
    public bool isSpecial = false;
    public Team team = Team.CT;
    public int range;
    public string infoText;

    private Image icon;

    public void Reset()
    {
        isSpecial = false;
        isSpawnable = true;
        team = Team.CT;
        range = 2;
        infoText = string.Empty;
    }

    public void OnEnable()
    {
        icon = GetComponent<Image>();
    }
}
