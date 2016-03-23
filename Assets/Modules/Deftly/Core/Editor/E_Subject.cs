// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;
using Deftly;

[CustomEditor(typeof(Subject))]
//[CanEditMultipleObjects]
public class E_Subject : Editor
{
    // TODO MaxWeaponSlots

    private Subject _x;
    public static Texture2D TeamTex;
    private static readonly GUIStyle TeamGuiStyle = new GUIStyle();
    private static Color _teamColor;

    private static readonly GUIContent ButtonAdd = new GUIContent("+", "Add weapon");
    private static readonly GUIContent ButtonRemove = new GUIContent("-", "Remove this weapon");

    // Basic data
    private readonly GUIContent _subjectGroup =     new GUIContent("Subject Group", "Is this a Character or is it a Prop?");
    private readonly GUIContent _teamId =           new GUIContent("Team ID", "What team is this Subject on?");
    private readonly GUIContent _title =            new GUIContent("Title", "The name of this Subject");
    private readonly GUIContent _godMode =          new GUIContent("God Mode", "All incoming damage is ignored");
    private readonly GUIContent _unlimitedMags =    new GUIContent("Unlimited Mags", "Magazines are not consumed when reloading");
    private readonly GUIContent _hitReaction =      new GUIContent("Hit Reaction", "When hit/damaged, this sound effect plays");
    private readonly GUIContent _crippledTime =     new GUIContent("Crippled Time", "After losing all health, enter 'downed' status where it can be revived. After this time, the Subject will turn into a corpse.");
    private readonly GUIContent _corpseTime =       new GUIContent("Corpse Time", "The model will disappear after this time (begins after Crippled ends)");

    // Base Stats
    //private readonly GUIContent _maxHealth =        new GUIContent("Max Health", "The max value of health this Subject can have");
    //private readonly GUIContent _health =           new GUIContent("Health", "The current health of this Subject");
    //private readonly GUIContent _armor =            new GUIContent("Armor", "Armor directly subtracts from incoming damage");
    private readonly GUIContent _armorType =        new GUIContent("Armor Type");

    // Weapon Data
    private readonly GUIContent _useMecanim =       new GUIContent("Use Mecanim", "Toggle the use of Mecanim and IK features");
    private readonly GUIContent _changeWeapon =     new GUIContent("Change Weapon", "This sound plays every time you swap weapons");
    
    // Physical data
    private readonly GUIContent _deadBody =         new GUIContent("Dead Body Obj", "The sub-object dead body spawned when you die");
    
    // Control speeds
    private readonly GUIContent _turnSpeed =        new GUIContent("Turn Speed", "The max rate at which the character turns");
    private readonly GUIContent _moveSpeed =        new GUIContent("Move Speed", "The max rate at which the character moves");

    void OnEnable()
    {
        TeamTex = new Texture2D(1, 1, TextureFormat.RGBA32, false) {hideFlags = HideFlags.HideAndDontSave};
        TeamGuiStyle.normal.background = TeamTex;
        _x = (Subject)target;  
    }

    public override void OnInspectorGUI()
    {  
        GUI.changed = false;
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        if (GUILayout.Button("General Information", EditorStyles.toolbarButton)) EditorUtils.SubjectGeneral = !EditorUtils.SubjectGeneral;
        if (EditorUtils.SubjectGeneral) ShowGeneral();

        if (GUILayout.Button("Primary Stats", EditorStyles.toolbarButton)) EditorUtils.SubjectStats = !EditorUtils.SubjectStats;
        if (EditorUtils.SubjectStats) ShowStats();

        if (_x.Stats.SubjectGroup != SubjectGroup.Other)
        {
            if (GUILayout.Button("Weapons", EditorStyles.toolbarButton))
                EditorUtils.SubjectWeaponData = !EditorUtils.SubjectWeaponData;
            if (EditorUtils.SubjectWeaponData) ShowWeapons();
        }

        if (_x.Stats.SubjectGroup != SubjectGroup.Other)
        {
            if (GUILayout.Button("Controller/Animator Data", EditorStyles.toolbarButton))
                EditorUtils.SubjectControls = !EditorUtils.SubjectControls;
            if (EditorUtils.SubjectControls) ShowControls();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(_x);
    }

    private void ShowGeneral()
    {
        EditorGUILayout.Space();

        switch (_x.Stats.TeamId)
        {
            case 0:
                _teamColor = Color.red;
                break;
            case 1:
                _teamColor = Color.blue;
                break;
            case 2:
                _teamColor = Color.cyan;
                break;
            case 3:
                _teamColor = Color.magenta;
                break;
            case 4:
                _teamColor = Color.yellow;
                break;
            case 5:
                _teamColor = Color.green;
                break;
            case 6:
                _teamColor = Color.black;
                break;
            case 7:
                _teamColor = Color.grey;
                break;
            case 8:
                _teamColor = Color.white;
                break;
        }
        if (TeamTex != null)
        {
            TeamTex.SetPixel(0, 0, _teamColor);
            TeamTex.Apply();
        }
        GUILayout.BeginVertical();
        GUILayout.Label(TeamTex, TeamGuiStyle, GUILayout.MaxWidth(1000), GUILayout.Height(5));
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        _x.Stats.SubjectGroup = (SubjectGroup) EditorGUILayout.EnumPopup(_subjectGroup, _x.Stats.SubjectGroup);
        _x.Stats.TeamId = EditorGUILayout.IntSlider(_teamId, _x.Stats.TeamId, 0, 8);
        _x.Stats.Title = EditorGUILayout.TextField(_title, _x.Stats.Title);
        _x.Stats.HitSound = EditorGUILayout.ObjectField(_hitReaction, _x.Stats.HitSound, typeof (AudioClip), false) as AudioClip;

        EditorGUILayout.Space();

        _x.Stats.CrippledTime = EditorGUILayout.Slider(_crippledTime, _x.Stats.CrippledTime, 0f, 30f);
        _x.Stats.CorpseTime = EditorGUILayout.Slider(_corpseTime, _x.Stats.CorpseTime, 0f, 60f);
        _x.Stats.SpawnFx = EditorGUILayout.ObjectField("Spawn Fx", _x.Stats.SpawnFx, typeof(GameObject), false) as GameObject;
        _x.Stats.LevelUpFx = EditorGUILayout.ObjectField("LevelUp Fx", _x.Stats.LevelUpFx, typeof (GameObject), false) as GameObject;
        _x.Stats.DeathFx = EditorGUILayout.ObjectField("Death Fx", _x.Stats.DeathFx, typeof(GameObject), false) as GameObject;

        _x.GodMode = EditorGUILayout.Toggle(_godMode, _x.GodMode);
        _x.UnlimitedAmmo = EditorGUILayout.Toggle(_unlimitedMags, _x.UnlimitedAmmo);
        _x.Stats.UseMecanim = EditorGUILayout.Toggle(_useMecanim, _x.Stats.UseMecanim);
        if (!_x.Stats.UseMecanim) EditorGUILayout.HelpBox("Non-Mecanim characters have not been thoroughly tested.", MessageType.Warning);

        _x.LogDebug = EditorGUILayout.Toggle("Log Debug", _x.LogDebug);

        EditorGUILayout.Space();

    }
    private void ShowStats()
    {
        EditorGUILayout.Space();
        _x.Stats.ArmorType = (ArmorType)EditorGUILayout.EnumPopup(_armorType, _x.Stats.ArmorType);
        EditorGUILayout.HelpBox(_x.Stats.ArmorType == ArmorType.Absorb
                ? "'Absorb' armor will absorb damage until drained. \nArmor degrades."
                : "'Nullify' armor reduces damage by specified amount. \nArmor does not degrade.", MessageType.None);
        
        EditorGUILayout.BeginHorizontal();

        EditorGUIUtility.labelWidth = 1;
        float fw = EditorGUIUtility.fieldWidth;
        EditorGUIUtility.fieldWidth = 5;
        EditorGUILayout.LabelField("");

        EditorGUILayout.LabelField("", "Base", GUILayout.MaxWidth(45));
        EditorGUILayout.LabelField("", "Min", GUILayout.MaxWidth(45));
        EditorGUILayout.LabelField("", "Max", GUILayout.MaxWidth(45));
        EditorGUILayout.LabelField("", "Per Lvl", GUILayout.MaxWidth(45));
        EditorGUILayout.LabelField("", "Actual", GUILayout.MaxWidth(45));
        EditorGUILayout.EndHorizontal();

        DrawStat(_x.Stats.Level, "Level");
        DrawStat(_x.Stats.Experience, "Xp");
        DrawStat(_x.Stats.XpReward, "Kill Xp");
        EditorGUILayout.Space();
        DrawStat(_x.Stats.Health, "Health");
        DrawStat(_x.Stats.Armor, "Armor");
        DrawStat(_x.Stats.Agility, "AGI");
        DrawStat(_x.Stats.Dexterity, "DEX");
        DrawStat(_x.Stats.Endurance, "END");
        DrawStat(_x.Stats.Strength, "STR");

        EditorGUIUtility.fieldWidth = fw;

        if (GUILayout.Button("Reset Stat Values")) { Subject.ResetCharacterStatValues(_x.Stats); }
        EditorGUILayout.Space();
    }
    private void ShowWeapons()
    {
        EditorGUILayout.Space();

        // show a runtime list of actual weapons
        if (Application.isPlaying) EditorGUILayout.PropertyField(serializedObject.FindProperty("WeaponListRuntime"), new GUIContent("Runtime Weapon List"), true);
        

        GUI.color = Color.green;
        if (GUILayout.Button(ButtonAdd, GUILayout.Width(20f))) _x.WeaponListEditor.Add(null);
        GUI.color = Color.white;


        // Handle Weapon List
        if (_x.WeaponListEditor.Count > 0)
        {
            for (int i = 0; i < _x.WeaponListEditor.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 1;
                EditorGUIUtility.labelWidth = 120;
                Weapon w = null;
                if (_x.WeaponListEditor[i] != null) w = _x.WeaponListEditor[i].GetComponent<Weapon>();
                _x.WeaponListEditor[i] = EditorGUILayout.ObjectField(i + ". " + (w != null ? w.Stats.Title : "Blank"), _x.WeaponListEditor[i], typeof (GameObject), true) as GameObject;
                
                
                GUI.color = Color.red;
                if (GUILayout.Button(ButtonRemove, GUILayout.Width(20f)))
                {
                    _x.WeaponListEditor.RemoveAt(i);
                    return;
                }
                EditorGUI.indentLevel = 0;
                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;


                if (_x.WeaponListEditor[i] == null) EditorGUILayout.HelpBox("Cannot have null weapon slots... (yet)", MessageType.Error);
            }
        }

        _x.SwapSound = EditorGUILayout.ObjectField(_changeWeapon, _x.SwapSound, typeof (AudioClip), false) as AudioClip;

        EditorGUILayout.Space();
    }

    private void ShowControls()
    {
        EditorGUILayout.Space();

        EditorGUIUtility.labelWidth = 120;
        EditorGUI.indentLevel = 1;

        EditorGUILayout.Space();

        _x.Stats.DeadBodyObj = EditorGUILayout.ObjectField(_deadBody, _x.Stats.DeadBodyObj, typeof(GameObject), true) as GameObject;
        _x.ControlStats.TurnSpeed = EditorGUILayout.Slider(_turnSpeed, _x.ControlStats.TurnSpeed, 1, 20);
        _x.ControlStats.MoveSpeed = EditorGUILayout.Slider(_moveSpeed, _x.ControlStats.MoveSpeed, 1, 35);

        EditorGUILayout.Space();

    }

    private void DrawStat(Stat s, string label)
    {
        EditorGUIUtility.fieldWidth = 1;
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 20;
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 1;

        s.Base = EditorGUILayout.IntField("Base", (int)s.Base, GUILayout.MaxWidth(45));
        s.Min = EditorGUILayout.IntField((int)s.Min, GUILayout.MaxWidth(45));    
        s.Max = EditorGUILayout.IntField((int)s.Max, GUILayout.MaxWidth(45));
        s.IncreasePerLevel = EditorGUILayout.FloatField(s.IncreasePerLevel, GUILayout.MaxWidth(45));
        s.Actual = EditorGUILayout.FloatField(s.Actual, GUILayout.MaxWidth(45));
        EditorGUILayout.EndHorizontal();
    }
}