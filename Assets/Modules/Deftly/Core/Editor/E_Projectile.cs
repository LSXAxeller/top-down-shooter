// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;
using Deftly;

[CustomEditor(typeof(Projectile))]
//[CanEditMultipleObjects]
public class E_Projectile : Editor
{
    private bool _showImpactTags = true;
    private Projectile _x;
    private static readonly GUIContent ButtonAdd = new GUIContent("+", "Add new tag filter");
    private static readonly GUIContent ButtonRemove = new GUIContent("-", "Remove this tag filter");

    // Basic Options
    private readonly GUIContent _title =        new GUIContent("Title", "The name of this projectile");
    private readonly GUIContent _weaponType =   new GUIContent("Weapon Type", "The manner in which the weapon is fired. Check the documentation for details");
    private readonly GUIContent _impactStyle =  new GUIContent("Impact Style", "How impact effect orientation is handled");
    private readonly GUIContent _mask =         new GUIContent("Mask", "The Layers this projectile is *allowed* to hit");

    // Stats
    private readonly GUIContent _speed =        new GUIContent("Speed", "Speed of the projectile. High speeds are safe from passing through thin colliders");
    private readonly GUIContent _continuous =   new GUIContent("Constant", "Is speed continuous or a 'one shot' push?");
    private readonly GUIContent _damage =       new GUIContent("Damage", "Amount of damage this will inflict");
    private readonly GUIContent _aoeCaused =    new GUIContent("AoE", "Does this weapon have an AoE damage effect?");
    private readonly GUIContent _aoeFx =        new GUIContent("AoE Fx", "Prefab of the AoE effect which spawns at the point of detonation");
    private readonly GUIContent _aoeRadius =    new GUIContent("Radius", "Radius of the AoE damage");
    private readonly GUIContent _aoeForce =     new GUIContent("Force", "Explosion force caused by AoE");
    private readonly GUIContent _maxTravel =    new GUIContent("Max Travel", "The max distance this can travel before being destroyed");
    private readonly GUIContent _lifetime =     new GUIContent("Lifetime", "The amount of time this can exist before being destroyed");
    private readonly GUIContent _bouncer =      new GUIContent("Bouncer", "If on: Does not detonate on impact, waits for Lifetime to expire and causes AoE");
    private readonly GUIContent _usePhysics =   new GUIContent("Locomotion", "Translate does not require Rigidbody's to use Continuous and uses Transform.Translate");

    // Advanced
    private readonly GUIContent _detach =       new GUIContent("Detach On Destroy", "This object will be detached on a hit. Useful for having Particle Effects outlive the projectile mesh");
    private readonly GUIContent _muzzleFlash =  new GUIContent("Muzzle Flash", "This will be instantiated at the Spawn Pt when fired, usually a particle system prefab");

    void OnEnable()
    {
        _x = (Projectile)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        #region Basic & Stats

        _x.LogDebug = EditorGUILayout.Toggle("Log Debug", _x.LogDebug);
        _x.Stats.Title = EditorGUILayout.TextField(_title, _x.Stats.Title);
        _x.Stats.weaponType = (ProjectileStats.WeaponType) EditorGUILayout.EnumPopup(_weaponType, _x.Stats.weaponType);
        _x.ImpactStyle = (Projectile.ImpactType)EditorGUILayout.EnumPopup(_impactStyle, _x.ImpactStyle);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Mask"), _mask, false);        
        
        
        EditorGUILayout.Space();

        _x.Stats.LineRenderer = _x.GetComponent<LineRenderer>();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60;
        _x.Stats.Speed = EditorGUILayout.Slider(_speed, _x.Stats.Speed, 0.5f, 80);
        if (_x.Stats.MoveStyle == ProjectileStats.ProjectileLocomotion.Physics)
            _x.Stats.ConstantForce = EditorGUILayout.Toggle(_continuous, _x.Stats.ConstantForce);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _x.Stats.Damage = EditorGUILayout.IntSlider(_damage, _x.Stats.Damage, 0, 200);
        _x.Stats.CauseAoeDamage = EditorGUILayout.Toggle(_aoeCaused, _x.Stats.CauseAoeDamage);
        if (_x.Stats.CauseAoeDamage)
        {
            EditorGUILayout.EndHorizontal();
            _x.Stats.AoeEffect = EditorGUILayout.ObjectField(_aoeFx, _x.Stats.AoeEffect, typeof(GameObject), false) as GameObject;
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 45;
            _x.Stats.AoeRadius = EditorGUILayout.FloatField(_aoeRadius, _x.Stats.AoeRadius);
            _x.Stats.AoeForce = EditorGUILayout.FloatField(_aoeForce, _x.Stats.AoeForce);
            EditorGUIUtility.labelWidth = 60;
            _x.Stats.Bouncer = EditorGUILayout.Toggle(_bouncer, _x.Stats.Bouncer);
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("NOTE: AoE Explosion Force does not affect Rigidbodies that have no active colliders. eg, dying Subjects.", EditorStyles.helpBox);
            GUI.color = Color.white;
        }
        else
        {
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUIUtility.labelWidth = 90;
        _x.Stats.MaxDistance = EditorGUILayout.Slider(_maxTravel, _x.Stats.MaxDistance, 0.1f, 100);
        _x.Stats.Lifetime = EditorGUILayout.Slider(_lifetime, _x.Stats.Lifetime, 0.1f, 10);

        _x.Stats.MoveStyle = (ProjectileStats.ProjectileLocomotion)EditorGUILayout.EnumPopup(_usePhysics, _x.Stats.MoveStyle);
        _x.Stats.UsePhysics = _x.Stats.MoveStyle == ProjectileStats.ProjectileLocomotion.Physics;

        
        EditorGUILayout.Space();

        _x.DetachOnDestroy = EditorGUILayout.ObjectField(_detach, _x.DetachOnDestroy, typeof(GameObject), true) as GameObject;
        _x.AttackEffect = EditorGUILayout.ObjectField(_muzzleFlash, _x.AttackEffect, typeof(GameObject), false) as GameObject;


        #endregion

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        #region Tags 
 
        _showImpactTags = EditorGUILayout.Foldout(_showImpactTags, "Impact Tags", EditorStyles.foldout);

        if (_showImpactTags)
        {
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
        }

        #endregion

        EditorGUILayout.Space();
        EditorGUILayout.Space(); 

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(_x);
    }
    void DisplayCompounds()
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
                _x.ImpactSounds[i] = EditorGUILayout.ObjectField("♫", _x.ImpactSounds[i], typeof (AudioClip), false) as AudioClip;
                _x.ImpactEffects[i] = EditorGUILayout.ObjectField("☼", _x.ImpactEffects[i], typeof (GameObject), false) as GameObject;
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