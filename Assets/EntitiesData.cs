using UnityEngine;

public class EntitiesData : MonoBehaviour {

    public static EntityID id;
    [System.Serializable]
	public enum EntityID
    {
        Tool_Eraser = -1,
        Info_T = 0,
        Info_T_Active = 6,
        Info_CT = 1,
        Info_CT_Active = 7,
        Info_Hostage = 2,
        Info_BombSpot = 3,
        Weapon_M4A1 = 4,
        Weapon_AK47 = 5,
    }

    static public string EntityToString(int index)
    {
        id = (EntityID)index;
        return id.ToString("D");
    }
}
