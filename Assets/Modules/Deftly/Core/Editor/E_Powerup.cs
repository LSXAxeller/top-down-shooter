// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;
using Deftly;

[CustomEditor(typeof(Powerup))]
public class E_Powerup : Editor
{
    private Powerup PowerUp;
    private enum CharacterTarget { Character }
    private CharacterTarget CharTarget = CharacterTarget.Character;
    private string label;

    void OnEnable()
    {
        PowerUp = (Powerup)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Powerup Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 1;
        PowerUp.TargetStat = (Powerup.Target)EditorGUILayout.EnumPopup(PowerUp.TargetStat);
        PowerUp.Act = (Powerup.ActionType)EditorGUILayout.EnumPopup(PowerUp.Act);
        if (PowerUp.TargetStat == Powerup.Target.Health | PowerUp.TargetStat == Powerup.Target.Armor)
        {
            EditorGUILayout.EnumPopup(CharTarget);
        }
        else
        {
            PowerUp.Pickup = (Powerup.PickupAffect)EditorGUILayout.EnumPopup(PowerUp.Pickup);
        }
        
        PowerUp.Value = EditorGUILayout.IntField(PowerUp.Value);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("These layers can pickup this Powerup:", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Mask"), false);

        switch (PowerUp.TargetStat)
        {
            case Powerup.Target.Health:
            {
                LabelVitals();
                break;
            }
            case Powerup.Target.Armor:
            {
                LabelVitals();
                break;
            }
            case Powerup.Target.Ammo:
            {
                LabelWeapon();
                break;
            }
            case Powerup.Target.Magazines:
            {
                LabelWeapon();
                break;
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("This Powerup will:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("... " + label);

        EditorGUILayout.Space(); 
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(PowerUp);
    }

    void LabelVitals()
    {
        if (PowerUp.Act == Powerup.ActionType.Set)
        {
            label = PowerUp.Act + " the " + PowerUp.TargetStat + " of the Character to " + PowerUp.Value;
        }
        else
        {
            label = PowerUp.Act + " " + PowerUp.Value + " " + PowerUp.TargetStat + (PowerUp.Act == Powerup.ActionType.Add ? " to the Character " : " from the Character");
        }
    }
    void LabelWeapon()
    {
        if (PowerUp.Act == Powerup.ActionType.Set)
        {
            label = PowerUp.Act + " " + PowerUp.TargetStat + " of " + (PowerUp.Pickup == Powerup.PickupAffect.Current ? "the Current " : "Each ") + "weapon to " + PowerUp.Value;
        }
        else
        {
            label = PowerUp.Act + " " + PowerUp.Value + " " + PowerUp.TargetStat + (PowerUp.Act == Powerup.ActionType.Add ? " to " : " from ") + (PowerUp.Pickup == Powerup.PickupAffect.Current ? "the Current " : "Each ") + "weapon";
        }
    }
}