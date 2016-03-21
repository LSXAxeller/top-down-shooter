// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace Deftly
{
    public class OptionsWindow : EditorWindow
    {
        private readonly GUIContent _showTxt =      new GUIContent("Show Floating Damage", "Will floating damage text be shown?");
        private readonly GUIContent _name =         new GUIContent("Prefab Name", "The name of the prefab which contains a GUIText and Floating Damage script. Must be in a *Resources* folder.");
        private readonly GUIContent _difficulty =   new GUIContent("Game Difficulty", "The Game Difficulty Level (if used)");
        private readonly GUIContent _pickup =       new GUIContent("Weapon Pickup Auto Switch", "Automatically change to the weapon after picking it up?");
        private readonly GUIContent _rpgStuff =     new GUIContent("RPG Elements", "Use Leveling and Stat features?");
        private readonly GUIContent _friendlyFire = new GUIContent("Friendly Fire", "Subjects of the same team can damage one another?");

        public static bool Thanks;
        private static bool _needToSave;

        public static OptionsData CurrentOptions;

        public static bool ResourceStatus;
        public static GameObject ResourceTemp;
        public static Rect WindowRect = new Rect(150,150, 400, 400);
        public static OptionsWindow Window;

        [MenuItem("Window/Deftly Global Options")]
        static void Init()
        {
            Window = (OptionsWindow)GetWindow(typeof(OptionsWindow));
            Window.titleContent.text = "Deftly Options";
            Window.position = WindowRect;
            Window.Show();

            LoadOptionValues();
            
            _needToSave = false;
            ResourceTemp = Resources.Load(CurrentOptions.FloatingTextPrefabName) as GameObject;
        }
        void OnGUI()
        {
            GUI.changed = false;
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // *****************************
            // Header, Buttons, Links
            // *****************************
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Release Notes")) Application.OpenURL("http://www.cleverous.com/#!deftly-release-notes/c1cdb");
            if (GUILayout.Button("Trello")) Application.OpenURL("https://trello.com/b/Wc9Zqt6r/cleverous-tds-kit-deftly");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Jabbr Chat")) Application.OpenURL("https://jabbr.net/#/rooms/PlayMakerDev");
            if (GUILayout.Button("Beta Group")) Application.OpenURL("https://groups.google.com/forum/#!forum/deftly-beta");
            GUI.color = _needToSave ? Color.red : Color.white;
            if (GUILayout.Button("Save"))
            {
                GUI.changed = false;
                Save();
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // *****************************
            // Variables
            // *****************************
            EditorGUILayout.LabelField("General Options", EditorStyles.boldLabel);

            CurrentOptions.UseFloatingText = EditorGUILayout.Toggle(_showTxt, CurrentOptions.UseFloatingText);
            if (CurrentOptions.UseFloatingText)
            {
                EditorGUI.indentLevel = 1;
                CurrentOptions.FloatingTextPrefabName = EditorGUILayout.TextField(_name, CurrentOptions.FloatingTextPrefabName);

                if (GUI.changed) ResourceTemp = Resources.Load(CurrentOptions.FloatingTextPrefabName) as GameObject;
                GUI.color = ResourceTemp == null ? Color.red : Color.green;
                EditorGUILayout.LabelField(ResourceTemp == null ? "No Prefab Found! Confirm it is in a /Resources/ folder." : "Located Prefab successfully.");
                GUI.color = Color.white;
                EditorGUI.indentLevel = 0;
            }
            CurrentOptions.Difficulty = EditorGUILayout.FloatField(_difficulty, CurrentOptions.Difficulty);
            CurrentOptions.WeaponPickupAutoSwitch = EditorGUILayout.Toggle(_pickup, CurrentOptions.WeaponPickupAutoSwitch);
            CurrentOptions.UseRpgElements = EditorGUILayout.Toggle(_rpgStuff, CurrentOptions.UseRpgElements);
            CurrentOptions.FriendlyFire = EditorGUILayout.Toggle(_friendlyFire, CurrentOptions.FriendlyFire);


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // *****************************
            // Footer, Special Thanks, Cleanup
            // *****************************
            EditorGUILayout.LabelField("More soon..", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            Thanks = EditorGUILayout.Foldout(Thanks, "Special Thanks");
            if (Thanks)
            {
                GUI.skin.label.wordWrap = true;
                EditorGUILayout.LabelField(
                    "Jean Fabre, Alex Chouls, Sebastain 'SebasRez' Alvarez, Nils '600' Jakrins, Jasper Flick, Emil 'AngryAnt' Johansen and all the nice folks on Unity Forum/Answers.",
                    EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Deftly™ Version: " + Options.AssetVersion + ", © Cleverous™ 2015");

            if (GUI.changed) _needToSave = true;
        }

        static void Save()
        {
            Options.Save(CurrentOptions);
            _needToSave = false;
        }
        static void LoadOptionValues()
        {
            CurrentOptions = Options.LoadStoredData();
        }
    }
}