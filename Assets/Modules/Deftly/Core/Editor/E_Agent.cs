// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;
using Deftly;

[CustomEditor(typeof(Agent))]
public class E_Agent : Editor
{
    private Agent _x;

    private readonly GUIContent _speed =            new GUIContent("Speed", "The speed at which the agent follows the waypoints");
    private readonly GUIContent _stoppingDistance = new GUIContent("Stopping Distance", "The distance from the waypoint at which the agent should stop or change waypoints");
    
    void OnEnable()
    {
        _x = (Agent)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        EditorGUILayout.LabelField("Current Velocity: " + _x.desiredVelocity);
        EditorGUILayout.LabelField("Remaining Distance: " + _x.remainingDistance);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        _x.speed = EditorGUILayout.Slider(_speed, _x.speed, 0f, 10f);
        _x.stoppingDistance = EditorGUILayout.Slider(_stoppingDistance, _x.stoppingDistance, 0f, 10f);
        
        EditorGUILayout.Space();

        DrawDefaultInspector();

        if (GUI.changed) EditorUtility.SetDirty(_x);
    }
}