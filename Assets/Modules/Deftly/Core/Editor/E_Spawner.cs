// (c) Copyright Cleverous 2015. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Deftly
{
    [CustomEditor(typeof (Spawner))]
    public class E_Spawner : Editor
    {
        private Spawner _x;
        private static readonly GUIContent ButtonAdd = new GUIContent("+", "Add Enemy");
        private static readonly GUIContent ButtonRemove = new GUIContent("-", "Remove Enemy");

        private void OnEnable()
        {
            _x = (Spawner) target;
        }

        public override void OnInspectorGUI()
        {
            GUI.changed = false;
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("General Information", EditorStyles.toolbarButton)) EditorUtils.SpawnerBasic = !EditorUtils.SpawnerBasic;
            if (EditorUtils.SpawnerBasic) ShowBasicInfo();

            if (GUILayout.Button("Spawner Prefabs and Counts", EditorStyles.toolbarButton)) EditorUtils.SpawnerPrefabs = !EditorUtils.SpawnerPrefabs;
            if (EditorUtils.SpawnerPrefabs) ShowSpawnerPrefabs();

            if (GUILayout.Button("Spawn Points", EditorStyles.toolbarButton)) EditorUtils.SpawnerPoints = !EditorUtils.SpawnerPoints;
            if (EditorUtils.SpawnerPoints) ShowSpawnPoints();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed) EditorUtility.SetDirty(_x);
        }

        private void ShowBasicInfo()
        {
            EditorGUILayout.Space();

            _x.SpawnerType = (Spawner.SpawnerStyle)EditorGUILayout.EnumPopup("Spawner Type", _x.SpawnerType);

            EditorGUIUtility.labelWidth = 140;
            GUI.enabled = _x.SpawnerType == Spawner.SpawnerStyle.FixedCount;
            _x.FixedWaveCount = EditorGUILayout.IntSlider("Fixed Wave Count", _x.FixedWaveCount, 1, 60);
            GUI.enabled = true;
            _x.TimeBetweenWaves = EditorGUILayout.Slider("Time Between Waves", _x.TimeBetweenWaves, 1, 60);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        private void ShowSpawnerPrefabs()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 1;
            float fw = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = 5;

            GUI.color = Color.green;
            if (GUILayout.Button(ButtonAdd, GUILayout.Width(20f)))
            {
                _x.EnemyPrefabs.Add(null);
                _x.EnemyCounts.Add(1);
            }
            GUI.color = Color.white;

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("", "Prefab", GUILayout.MaxWidth(130));
            EditorGUILayout.LabelField("", "Count", GUILayout.MaxWidth(70));
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _x.EnemyPrefabs.Count; i++)
            {
                DrawEnemy(_x.EnemyPrefabs, _x.EnemyCounts, i);
            }

            EditorGUIUtility.fieldWidth = fw;
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        private void ShowSpawnPoints()
        {
            EditorGUILayout.Space();

            GUI.color = Color.green;
            if (GUILayout.Button(ButtonAdd, GUILayout.Width(20f))) _x.Positions.Add(null);
            GUI.color = Color.white;

            for (int i = 0; i < _x.Positions.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Point " + (i + 1), EditorStyles.boldLabel);
                _x.Positions[i] = EditorGUILayout.ObjectField(_x.Positions[i], typeof(GameObject), false, GUILayout.MaxWidth(130)) as GameObject;

                GUI.color = Color.red;
                if (GUILayout.Button(ButtonRemove, GUILayout.Width(20f)))
                {
                    _x.Positions.RemoveAt(i);
                    return;
                }
                GUI.color = Color.white;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        private void DrawEnemy(List<GameObject> enemyRefList, List<int> enemyCountList, int index)
        {
            EditorGUIUtility.fieldWidth = 1;
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 20;
            EditorGUILayout.LabelField("Enemy " + (index+1), EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 1;

            enemyRefList[index] = EditorGUILayout.ObjectField(enemyRefList[index], typeof (GameObject), false, GUILayout.MaxWidth(130)) as GameObject;
            enemyCountList[index] = EditorGUILayout.IntField(enemyCountList[index], GUILayout.MaxWidth(45));
            GUI.color = Color.red;
            if (GUILayout.Button(ButtonRemove, GUILayout.Width(20f)))
            {
                _x.EnemyPrefabs.RemoveAt(index);
                _x.EnemyCounts.RemoveAt(index);
                return;
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }
}