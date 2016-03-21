// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;
using Deftly;

[CustomEditor(typeof(Intellect))]
//[CanEditMultipleObjects]
public class E_Intellect : Editor
{
    private Intellect _x;
    
    // Basic Stats
    private readonly GUIContent _ignoreUpdates      = new GUIContent("Ignore Updates", "How many update cycles to ignore before processing the AI");
    private readonly GUIContent _senseFrequency     = new GUIContent("Sense Frequency", "How many AI process cycles to ignore before updating the senses");
    private readonly GUIContent _provokeType        = new GUIContent("Provoke Type", "How the AI becomes provoked");
    private readonly GUIContent _threatPriority     = new GUIContent("Threat Priority", "Priority choice for selecting targets");
    private readonly GUIContent _provokeDelay       = new GUIContent("Provoke Delay", "After being provoked, how long to wait before taking action");
    private readonly GUIContent _helpAllies         = new GUIContent("Help Allies", "'If a nearby Intellect on my team has a bigger Threat, I will attack it'");
    //private readonly GUIContent _canRunShoot        = new GUIContent("Run and Shoot", "UNIMPLEMENTED");
    private readonly GUIContent _fov                = new GUIContent("Field Of View", "Field of view for the AI, in angle units facing forward");
    private readonly GUIContent _sightRange         = new GUIContent("Sight Range", "The sight range, how far can the AI can see before recognizing Subjects");
    // private readonly GUIContent _attackRange        = new GUIContent("Attack Range", "The maximum distance the target can be away from the AI in order to fire");
    private readonly GUIContent _wanderRange        = new GUIContent("Wander Range", "UNIMPLEMENTED");
    private readonly GUIContent _engageThresh       = new GUIContent("Engage Threshold", "From the edge of Attack Range, this is a grace distance");
    private readonly GUIContent _sightMask          = new GUIContent("Sight Mask", "The Layers that the AI is *allowed* to see. Used for navigation and confirming line of sight to targets");
    private readonly GUIContent _threatMask         = new GUIContent("Threat Mask", "An optimization mask, only Subjects which could be threats should be specified");
    private readonly GUIContent _fleeHealth         = new GUIContent("Flee Health", "When health reaches this value, the AI will Flee");
    private readonly GUIContent _animDir            = new GUIContent("Animator Direction", "In the Animator Controller, the name of the Direction parameter");
    private readonly GUIContent _animSpeed          = new GUIContent("Animator Speed", "In the Animator Controller, the name of the Speed parameter");
    private readonly GUIContent _alertTime          = new GUIContent("Alert Time", "Time spent in the Alert mood before returning to normal.");
    //private readonly GUIContent _patrolPoints       = new GUIContent("Patrol Points", "The nodes, in sequence, which the AI will Patrol");
    //private readonly GUIContent _patrolDeviation    = new GUIContent("Patrol Deviation", "When provoked, the Maximum distance the AI will deviate from a node while patrolling");
    
    void OnEnable()
    {
        _x = (Intellect)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Note: Intellect class not fully implemented yet.", MessageType.None);
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 80;
        _x.LogDebug = EditorGUILayout.Toggle("Log Debug", _x.LogDebug);
        _x.DrawGizmos = EditorGUILayout.Toggle("Draw Gizmos", _x.DrawGizmos);
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 125;
        _x.IgnoreUpdates = EditorGUILayout.IntField(_ignoreUpdates, _x.IgnoreUpdates);
        _x.SenseFrequency = EditorGUILayout.IntField(_senseFrequency, _x.SenseFrequency);

        EditorGUILayout.Space();

        _x.MyProvokeType = (Intellect.ProvokeType)EditorGUILayout.EnumPopup(_provokeType, _x.MyProvokeType);
        _x.MyThreatPriority = (Intellect.ThreatPriority)EditorGUILayout.EnumPopup(_threatPriority, _x.MyThreatPriority);
        _x.ProvokeDelay = EditorGUILayout.FloatField(_provokeDelay, _x.ProvokeDelay);
        _x.HelpAllies = EditorGUILayout.Toggle(_helpAllies, _x.HelpAllies);
        _x.MaxAllyCount = EditorGUILayout.IntField("Max Allies", _x.MaxAllyCount);

        EditorGUILayout.Space();

        _x.FieldOfView = EditorGUILayout.FloatField(_fov, _x.FieldOfView);
        _x.SightRange = EditorGUILayout.FloatField(_sightRange, _x.SightRange);
        // _x.AttackRange = EditorGUILayout.FloatField(_attackRange, _x.AttackRange);
        _x.WanderRange = EditorGUILayout.FloatField(_wanderRange, _x.WanderRange);
        _x.EngageThreshold = EditorGUILayout.FloatField(_engageThresh, _x.EngageThreshold);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("SightMask"), _sightMask, false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ThreatMask"), _threatMask, false);
        _x.FleeHealthThreshold = EditorGUILayout.IntField(_fleeHealth, _x.FleeHealthThreshold);

        EditorGUILayout.Space();

        _x.AnimatorDirection = EditorGUILayout.TextField(_animDir, _x.AnimatorDirection);
        _x.AnimatorSpeed = EditorGUILayout.TextField(_animSpeed, _x.AnimatorSpeed);
        _x.AlertTime = EditorGUILayout.FloatField(_alertTime, _x.AlertTime);

        EditorGUILayout.Space();

        _x.JukeTime = EditorGUILayout.FloatField("Juke Time", _x.JukeTime);
        _x.JukeFrequency = EditorGUILayout.FloatField("Juke Frequency", _x.JukeFrequency);
        _x.JukeFrequencyRandomness = EditorGUILayout.FloatField("Juke Freq Rng", _x.JukeFrequencyRandomness);

        EditorGUILayout.Space();
        
        SerializedProperty tps = serializedObject.FindProperty("PatrolPoints");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps, true);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        // EditorGUILayout.PropertyField(serializedObject.FindProperty("PatrolPoints"), _patrolPoints, false);
        // _x.MaxDeviation = EditorGUILayout.FloatField(_patrolDeviation, _x.MaxDeviation);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(_x);
    }
}