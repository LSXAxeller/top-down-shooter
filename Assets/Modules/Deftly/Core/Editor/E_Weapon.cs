// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;
using Deftly;

[CustomEditor(typeof(Weapon))]
//[CanEditMultipleObjects]
public class E_Weapon : Editor
{
    private bool _melee;
    private Weapon _x;
    private static readonly GUIContent ButtonAdd = new GUIContent("+", "Add spawn point");
    private static readonly GUIContent ButtonRemove = new GUIContent("-", "Remove this spawn point");
    
    // Basic Stats
    private readonly GUIContent _title =        new GUIContent("Title", "The name of this weapon");
    private readonly GUIContent _type =         new GUIContent("Type", "The type of weapon");
    private readonly GUIContent _style =        new GUIContent("Style", "The firing style of the weapon");
    private readonly GUIContent _projectile =   new GUIContent("Projectile", "The GameObject prefab that the weapon will spawn");
    private readonly GUIContent _ejector =      new GUIContent("Ammo Ejector", "The Particle System that will eject spent ammo casings after firing");
    private readonly GUIContent _accuracy =     new GUIContent("Accuracy", "Accuracy of the weapon. 1.0 is perfect, 0.0 is mayhem.");
    private readonly GUIContent _range =        new GUIContent("Intellect Range", "Max range at which Intellect(AI) will try to fire this weapon.");
    private readonly GUIContent _autoReload =   new GUIContent("Auto Reload", "Automatically try to reload the weapon at zero ammo.");
    private readonly GUIContent _uiSprite =     new GUIContent("UI Sprite", "Optional Sprite slot for easy reference in UI");
    private readonly GUIContent _canHitMulti =  new GUIContent("Can Hit Multiple", "Can this weapon damage more than one thing per swing?");
    private readonly GUIContent _yield =        new GUIContent("Can Move While Attacking", "Can the character move while attacking?");
    private readonly GUIContent _mask =         new GUIContent("Mask", "The Layers this weapon is *allowed* to hit");
    private readonly GUIContent _trail =        new GUIContent("Trail Obj", "Optionally define a trail that is on/off synced with the attacks");
    private readonly GUIContent _pickup =       new GUIContent("Pickup Ref Obj", "The Prefab that is used to Pickup this Weapon.");
    private readonly GUIContent _typeId =       new GUIContent("Type ID", "Weapon 'Type ID', used to indentify which weapon type this is. \n\nAnimator Controllers can use this to move between states based upon the ID. For instance '1' for Melee weapons, '2' for Ranged weapons. Or, a unique id for each weapon if you need many states and have many custom animations.");

    // Sounds
    private readonly GUIContent _sNoAmmo =      new GUIContent("No Ammo", "Sound when the weapon has no ammo left in the magazine.");
    private readonly GUIContent _sReload =      new GUIContent("Reload", "Sound when the weapon reloads");
    private readonly GUIContent _fireSounds =   new GUIContent("Firing Sounds", "Randomly selected when the weapon is fired");

    // Timing
    private readonly GUIContent _tReload =      new GUIContent("Reload", "How long it takes to reload this weapon");
    private readonly GUIContent _tCooldown =    new GUIContent("Cooldown", "The time between shots");
    private readonly GUIContent _tSwap =        new GUIContent("Swap Time", "How long it takes to swap to this weapon");

    // Spawn Points
    private readonly GUIContent _capacity =     new GUIContent("Capacity", "The ammo capacity of each magazine");
    private readonly GUIContent _startsWith =   new GUIContent("Start Mags", "How many Magazines the weapon starts with");
    private readonly GUIContent _fireCost =     new GUIContent("Fire Cost", "How much ammo is used when the weapon is fired");

    // Ik stuff
    private readonly GUIContent _pivot =        new GUIContent("Pivot Pt", "The point the offset of the weapon is calculated from");
    private readonly GUIContent _nonDomGoal =   new GUIContent("Non-Dominant Goal", "Specify a goal for the hand. This should should be a child object on the weapon Prefab.");
    //private readonly GUIContent _nonDomHandP =  new GUIContent("Additional Pos Offset", "Additional position offset from the goal.\n\nTypically 0,0,0..");
    //private readonly GUIContent _nonDomHandR =  new GUIContent("Additional Rot Offset", "Additional rotation offset from the goal.\n\nTypically 0,0,0..");
    private readonly GUIContent _offsetP =      new GUIContent("Weapon Resting Position Offset", "The Dominant Hand (holding the weapon) will reach to this offset from the Pivot Point. \n\nFor instance, 0,0,1 with a Shoulder pivot will put the weapon directly forward from the Dominant Hand's shoulder 1 unit.");
    private readonly GUIContent _offsetR =      new GUIContent("Dominant Hand Additional Rotation", "The hand will align the given Thumb Direction (from the Subject) to World Up, but you can add optional rotation angles for the Dominant Hand with this field.");
    private readonly GUIContent _offsetElbowN = new GUIContent("Non Dominant Elbow Offset Goal", "Offset position for the elbow goal from the Non Dominant Shoulder.\n\nThis allows you to tell the elbow to try to go to some position, when possible.");
    private readonly GUIContent _offsetElbowD = new GUIContent("Dominant Elbow Offset Goal", "Offset position for the elbow goal from the Dominant Shoulder.\n\nThis allows you to tell the elbow to try to go to some position, when possible.");
    private readonly GUIContent _hand =         new GUIContent("Hand", "Which hand this weapon is held in. \n\nNOTE: Currently disabled.");
     
    void OnEnable()
    {
        _x = (Weapon)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        _melee = _x.Stats.WeaponType == WeaponType.Melee;


        if (GUILayout.Button("General Information", EditorStyles.toolbarButton)) EditorUtils.WeaponStats = !EditorUtils.WeaponStats;
        if (EditorUtils.WeaponStats) ShowStats();

        if (GUILayout.Button(_melee ? "Sound Effects" : "Sounds, Timing, Cooldowns", EditorStyles.toolbarButton)) EditorUtils.WeaponSoundsAndTiming = !EditorUtils.WeaponSoundsAndTiming;
        if (EditorUtils.WeaponSoundsAndTiming) ShowSoundsAndTiming();

        if (_melee)
        {
            if (GUILayout.Button("Melee Attacks", EditorStyles.toolbarButton)) EditorUtils.WeaponAttacks = !EditorUtils.WeaponAttacks;
            if (EditorUtils.WeaponAttacks) ShowAttacks();

            if (GUILayout.Button("Weapon Impact Tags", EditorStyles.toolbarButton)) EditorUtils.WeaponImpactTags = !EditorUtils.WeaponImpactTags;
            if (EditorUtils.WeaponImpactTags) ShowImpactTags();
        }
        if (!_melee)
        {
            if (GUILayout.Button("Spawn Points", EditorStyles.toolbarButton)) EditorUtils.WeaponSpawns = !EditorUtils.WeaponSpawns;
            if (EditorUtils.WeaponSpawns) ShowSpawns();

            if (GUILayout.Button("Ammunition", EditorStyles.toolbarButton)) EditorUtils.WeaponAmmo = !EditorUtils.WeaponAmmo;
            if (EditorUtils.WeaponAmmo) ShowAmmo();
        }

        if (GUILayout.Button("Mecanim Ik Configuration", EditorStyles.toolbarButton)) EditorUtils.WeaponIk = !EditorUtils.WeaponIk;
        if (EditorUtils.WeaponIk) ShowIk();

        #region _#### not working prefab applier_
        /*
        if (Application.isPlaying) GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Apply Changes to Prefab", EditorStyles.toolbarButton))
        {
            GameObject go = _x.gameObject;
            GameObject prefab = (GameObject)PrefabUtility.GetPrefabObject(go);
            PrefabUtility.ReconnectToLastPrefab(go);
            PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }
        */
        #endregion

        EditorGUILayout.Space();
        
        Save();
    }

    private void Save()
    {
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(_x);
    }
    private void ShowStats()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        _x.Stats.Title = EditorGUILayout.TextField(_title, _x.Stats.Title);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _x.Stats.WeaponType = (WeaponType)EditorGUILayout.EnumPopup(_type, _x.Stats.WeaponType);
        EditorGUIUtility.labelWidth = 40;
        _x.Stats.FireStyle = (FireStyle)EditorGUILayout.EnumPopup(_style, _x.Stats.FireStyle);
        EditorGUIUtility.labelWidth = 90;
        EditorGUILayout.EndHorizontal();

        if (!_melee)
        {
            _x.Stats.FiresProjectile = EditorGUILayout.ObjectField(_projectile, _x.Stats.FiresProjectile, typeof(GameObject), true) as GameObject;
            _x.PickupReference = EditorGUILayout.ObjectField(_pickup, _x.PickupReference, typeof(GameObject), false) as GameObject;
            EditorGUIUtility.labelWidth = 90;
            _x.Stats.AmmoEjector = EditorGUILayout.ObjectField(_ejector, _x.Stats.AmmoEjector, typeof(ParticleSystem), true) as ParticleSystem;

            EditorGUILayout.BeginHorizontal();
            _x.Stats.EffectiveRange = EditorGUILayout.Slider(_range, _x.Stats.EffectiveRange, 2f, 100);
            _x.Stats.TypeId = EditorGUILayout.IntField(_typeId, _x.Stats.TypeId);
            EditorGUILayout.EndHorizontal();

            _x.Stats.Accuracy = EditorGUILayout.Slider(_accuracy, _x.Stats.Accuracy, 0.1f, 1.0f);
            _x.Stats.AutoReload = EditorGUILayout.Toggle(_autoReload, _x.Stats.AutoReload);
        }
        else
        {
            EditorGUIUtility.labelWidth = 60;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Mask"), _mask, false);
            EditorGUIUtility.labelWidth = 90;
            _x.PickupReference = EditorGUILayout.ObjectField(_pickup, _x.PickupReference, typeof(GameObject), false) as GameObject;
            _x.Stats.WeaponTrail = EditorGUILayout.ObjectField(_trail, _x.Stats.WeaponTrail, typeof(GameObject), true) as GameObject;

            EditorGUILayout.BeginHorizontal();
            _x.Stats.EffectiveRange = EditorGUILayout.Slider(_range, _x.Stats.EffectiveRange, 1f, 10);
            _x.Stats.TypeId = EditorGUILayout.IntField(_typeId, _x.Stats.TypeId);
            EditorGUILayout.EndHorizontal(); 
            
            EditorGUIUtility.labelWidth = 160;
            _x.Stats.CanHitMultiples = EditorGUILayout.Toggle(_canHitMulti, _x.Stats.CanHitMultiples);
            _x.Stats.CanAttackAndMove = EditorGUILayout.Toggle(_yield, _x.Stats.CanAttackAndMove);
        }
        EditorGUIUtility.labelWidth = 90;

        _x.Stats.UiImage = EditorGUILayout.ObjectField(_uiSprite, _x.Stats.UiImage, typeof(Sprite), false) as Sprite;

        EditorGUILayout.Space();
    }
    private void ShowSoundsAndTiming()
    {
        EditorGUILayout.Space();

        EditorGUIUtility.labelWidth = 70;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FireSounds"), _fireSounds, true);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (!_melee)
        {
            _x.Stats.NoAmmoSound = EditorGUILayout.ObjectField(_sNoAmmo, _x.Stats.NoAmmoSound, typeof(AudioClip), false) as AudioClip;
            _x.Stats.ReloadSound = EditorGUILayout.ObjectField(_sReload, _x.Stats.ReloadSound, typeof(AudioClip), false) as AudioClip;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Timing:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _x.Stats.ReloadTime = EditorGUILayout.FloatField(_tReload, _x.Stats.ReloadTime);
            _x.Stats.TimeToCooldown = EditorGUILayout.FloatField(_tCooldown, _x.Stats.TimeToCooldown);
            EditorGUILayout.EndHorizontal();
        }

        _x.Stats.SwapTime = EditorGUILayout.FloatField(_tSwap, _x.Stats.SwapTime);

        EditorGUILayout.Space();

    }
    private void ShowImpactTags()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button(ButtonAdd, GUILayout.Width(20f)))
        {
            _x.ImpactTagNames.Add(null);
            _x.ImpactEffects.Add(null);
            _x.ImpactSounds.Add(null);
        }
        GUI.color = Color.white;
        EditorGUIUtility.labelWidth = 300;
        EditorGUILayout.HelpBox("Note: First (0) is default if there is no match.", MessageType.None);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        DisplayCompounds();
        EditorGUI.indentLevel = 0;

        EditorGUILayout.Space();

    }
    private void ShowAttacks()
    {
        EditorGUILayout.Space();
        if (_x.AttackPrefabs.Count == 0) EditorGUILayout.HelpBox("A Melee weapon must have at least one Attack!", MessageType.Error);        


        GUI.color = Color.green;
        if (GUILayout.Button(ButtonAdd, GUILayout.Width(20f))) _x.AttackPrefabs.Add(null);
        GUI.color = Color.white;


        if (_x.AttackPrefabs.Count > 0)
        {
            for (int i = 0; i < _x.AttackPrefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 1;
                EditorGUIUtility.labelWidth = 70;
                _x.AttackPrefabs[i] = EditorGUILayout.ObjectField(i + ") Attack", _x.AttackPrefabs[i], typeof (GameObject), true) as GameObject;
                
                
                GUI.color = Color.red;
                if (GUILayout.Button(ButtonRemove, GUILayout.Width(20f))) _x.AttackPrefabs.RemoveAt(i);
                

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                if (_x.AttackPrefabs[i] == null) EditorGUILayout.HelpBox("Cannot have null Attacks.", MessageType.Error);
            }
        }

        EditorGUI.indentLevel = 0;
        EditorGUILayout.Space();

    }
    private void ShowSpawns()
    {
        EditorGUILayout.Space();
        if (_x.Stats.ProjectileSpawnPoints.Count == 0) EditorGUILayout.HelpBox("A Weapon must have at least one Projectile Spawn Point!", MessageType.Error);
        

        GUI.color = Color.green;
        if (GUILayout.Button(ButtonAdd, GUILayout.Width(20f))) _x.Stats.ProjectileSpawnPoints.Add(null);
        GUI.color = Color.white;


        if (_x.Stats.ProjectileSpawnPoints.Count > 0)
        {
            for (int i = 0; i < _x.Stats.ProjectileSpawnPoints.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel = 1;
                EditorGUIUtility.labelWidth = 100;
                _x.Stats.ProjectileSpawnPoints[i] = EditorGUILayout.ObjectField(i + ") Spawn Pt", _x.Stats.ProjectileSpawnPoints[i], typeof (GameObject), true) as GameObject;


                GUI.color = Color.red;
                if (GUILayout.Button(ButtonRemove, GUILayout.Width(20f))) _x.Stats.ProjectileSpawnPoints.RemoveAt(i);
                

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                if (_x.Stats.ProjectileSpawnPoints[i] == null) EditorGUILayout.HelpBox("Cannot have null spawn points.", MessageType.Error);
            }
        }

        EditorGUI.indentLevel = 0;
        EditorGUILayout.Space();

    }
    private void ShowAmmo()
    {
        EditorGUILayout.Space();

        EditorGUIUtility.labelWidth = 70;
        EditorGUILayout.LabelField("Magazines:", EditorStyles.boldLabel);


        EditorGUILayout.BeginHorizontal();
        _x.Stats.MagazineSize = EditorGUILayout.IntField(_capacity, _x.Stats.MagazineSize);
        _x.Stats.StartingMagazines = EditorGUILayout.IntField(_startsWith, _x.Stats.StartingMagazines);
        EditorGUILayout.EndHorizontal();

        _x.Stats.AmmoCost = EditorGUILayout.IntField(_fireCost, _x.Stats.AmmoCost);

        EditorGUILayout.Space();

    }
    private void ShowIk()
    {
        EditorGUILayout.Space();

        if (_melee)
        {
            EditorGUILayout.LabelField("NOTE: Melee Weapons are handled entirely by animation. Ik is not used.", EditorStyles.helpBox);
            EditorGUILayout.Space();
            return;
        }
 
        EditorGUILayout.LabelField("NOTE: Configure weapons for the Right Hand. Weapons held by the Left Hand is currently disabled.", EditorStyles.helpBox);

        EditorGUILayout.Space();
        EditorUtils.AddBlackLine();
        EditorUtils.AddBlackLine();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Relative Weapon Position:", EditorStyles.boldLabel);
        GUI.color = Color.yellow;
        EditorGUILayout.LabelField("Specify the weapon's relative offset from the Pivot.", EditorStyles.helpBox);
        GUI.color = Color.white;
        EditorGUIUtility.labelWidth = 150;

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        _x.Stats.MountPivot = (MountPivot) EditorGUILayout.EnumPopup(_pivot, _x.Stats.MountPivot);        
        _x.Stats.WeaponHeldInHand = (Hand) EditorGUILayout.EnumPopup(_hand, _x.Stats.WeaponHeldInHand);
        EditorGUILayout.EndHorizontal();



        bool wm = EditorGUIUtility.wideMode;
        EditorGUIUtility.wideMode = false;

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 1;
        GUILayout.Label(EditorGUIUtility.IconContent("MoveTool"));
        EditorGUIUtility.labelWidth = 200;
        _x.Stats.PositionOffset = EditorGUILayout.Vector3Field(_offsetP, _x.Stats.PositionOffset);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 1;
        GUILayout.Label(EditorGUIUtility.IconContent("RotateTool"));
        EditorGUIUtility.labelWidth = 200;
        _x.Stats.RotationOffset = EditorGUILayout.Vector3Field(_offsetR, _x.Stats.RotationOffset);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 1;
        GUILayout.Label(EditorGUIUtility.IconContent("RectTool"));
        _x.Stats.UseElbowHintR = EditorGUILayout.Toggle("", _x.Stats.UseElbowHintR);
        EditorGUIUtility.labelWidth = 200;
        _x.Stats.DominantElbowOffset = EditorGUILayout.Vector3Field(_offsetElbowD, _x.Stats.DominantElbowOffset);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 1;
        GUILayout.Label(EditorGUIUtility.IconContent("RectTool"));
        _x.Stats.UseElbowHintL = EditorGUILayout.Toggle("", _x.Stats.UseElbowHintL);
        EditorGUIUtility.labelWidth = 200;
        _x.Stats.NonDominantElbowOffset = EditorGUILayout.Vector3Field(_offsetElbowN, _x.Stats.NonDominantElbowOffset);
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.wideMode = wm;
        EditorGUILayout.LabelField("Hint: Do not use 0,0,0 for elbow goals (that's the shoulder position).", EditorStyles.helpBox);
        EditorGUILayout.Space();
        EditorUtils.AddBlackLine();
        EditorUtils.AddBlackLine();
        EditorGUILayout.Space();


        EditorGUILayout.LabelField("IK (Non-Dominant) Hand Position:", EditorStyles.boldLabel);
        EditorGUIUtility.labelWidth = 150;
        _x.Stats.NonDominantHandGoal = EditorGUILayout.ObjectField(_nonDomGoal, _x.Stats.NonDominantHandGoal, typeof(Transform), true) as Transform;
        
        // I don't think these are necessary anymore.
        //_x.Stats.NonDominantHandPos = EditorGUILayout.Vector3Field(_nonDomHandP, _x.Stats.NonDominantHandPos);
        //_x.Stats.NonDominantHandRot = EditorGUILayout.Vector3Field(_nonDomHandR, _x.Stats.NonDominantHandRot);
        

        EditorGUILayout.Space();
        EditorUtils.AddBlackLine();
        EditorUtils.AddBlackLine();
        EditorGUILayout.Space();

    }
    private void DisplayCompounds()
    {
        for (int i = 0; i < _x.ImpactTagNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel = 1;
            EditorGUIUtility.labelWidth = 95;

            _x.ImpactTagNames[i] = EditorGUILayout.TextField(i + ")Tag Name", _x.ImpactTagNames[i]);

            EditorGUILayout.EndHorizontal();

            if (_x.ImpactTagNames.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 30;
                _x.ImpactSounds[i] = EditorGUILayout.ObjectField("♫", _x.ImpactSounds[i], typeof(AudioClip), false) as AudioClip;
                _x.ImpactEffects[i] = EditorGUILayout.ObjectField("☼", _x.ImpactEffects[i], typeof(GameObject), false) as GameObject;
                GUI.color = Color.red;
                if (GUILayout.Button(ButtonRemove, GUILayout.Width(20f)))
                {
                    _x.ImpactTagNames.RemoveAt(i);
                    _x.ImpactEffects.RemoveAt(i);
                    _x.ImpactSounds.RemoveAt(i);
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
        }
    }
}
