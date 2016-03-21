// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Agent))]
public class E_Agent : Editor
{
    private Agent _x;

    private readonly GUIContent _speed =            new GUIContent("Speed", "The speed at which the agent follows the waypoints");
    private readonly GUIContent _acceleration =     new GUIContent("Acceleration", "Limit how quickly the agent changes velocity");
    private readonly GUIContent _angularSpeed =     new GUIContent("Angular Speed", "How fast the agent changes its direction");
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
        _x.acceleration = EditorGUILayout.Slider(_acceleration, _x.acceleration, 0f, 10f);
        _x.angularSpeed = EditorGUILayout.Slider(_angularSpeed, _x.angularSpeed, 0f, 10f);
        _x.stoppingDistance = EditorGUILayout.Slider(_stoppingDistance, _x.stoppingDistance, 0f, 10f);
        
        EditorGUILayout.Space();

        DrawDefaultInspector();

        if (GUI.changed) EditorUtility.SetDirty(_x);
    }
}