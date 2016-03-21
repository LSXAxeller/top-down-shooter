// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;
using Deftly;

[CustomEditor(typeof(DeftlyCamera))]
public class E_Camera : Editor
{
    private DeftlyCamera _x;

    private readonly GUIContent _following =        new GUIContent("Follow Style", "The 'feel' of how the camera follows the targets");
    private readonly GUIContent _tracking =         new GUIContent("Tracking Style", "Option to follow the Average Position or the Average 'aiming direction'");
    private readonly GUIContent _trackDistance =    new GUIContent("Track Distance", "The distance from each character that is sampled during tracking");
    private readonly GUIContent _trackSpeed =       new GUIContent("Track Speed", "How fast the camera tracks its targets with Loose mode");
    private readonly GUIContent _offset =           new GUIContent("Position Offset", "The literal positional offset in xyz from the camera's targets");

    void OnEnable()
    {
        _x = (DeftlyCamera)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        _x.FollowingStyle = (DeftlyCamera.MoveStyle) EditorGUILayout.EnumPopup(_following, _x.FollowingStyle);
        _x.Tracking = (DeftlyCamera.TrackingStyle) EditorGUILayout.EnumPopup(_tracking, _x.Tracking);

        _x.TrackDistance = EditorGUILayout.Slider(_trackDistance, _x.TrackDistance, 0f, 10f);
        _x.TrackSpeed = EditorGUILayout.Slider(_trackSpeed, _x.TrackSpeed, 1f, 20f);

        _x.Offset = EditorGUILayout.Vector3Field(_offset, _x.Offset);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("Targets"), true);

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(_x);
    }
}