// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
using UnityEditor;

public class EditorUtils : Editor
{
    //
    // Try not to rename this stuff.
    // This keeps a memory of which foldouts are opened/closed.
    // Add a new bool for each new foldout required.
    //



    // Subject.cs Foldout memory
    public static bool SubjectGeneral {     get { return EditorPrefs.GetBool("Deftly_SubjectGeneral"); }     set { EditorPrefs.SetBool("Deftly_SubjectGeneral", value); } }
    public static bool SubjectStats {       get { return EditorPrefs.GetBool("Deftly_SubjectStats"); }       set { EditorPrefs.SetBool("Deftly_SubjectStats", value); } }
    public static bool SubjectWeaponData {  get { return EditorPrefs.GetBool("Deftly_SubjectWeaponData"); }  set { EditorPrefs.SetBool("Deftly_SubjectWeaponData", value); } }
    public static bool SubjectIk {          get { return EditorPrefs.GetBool("Deftly_SubjectIk"); }          set { EditorPrefs.SetBool("Deftly_SubjectIk", value); } }
    public static bool SubjectControls {    get { return EditorPrefs.GetBool("Deftly_SubjectControls"); }    set { EditorPrefs.SetBool("Deftly_SubjectControls", value); } }

    // Weapon.cs Foldout memory
    public static bool WeaponStats {        get { return EditorPrefs.GetBool("Deftly_WeaponStats"); }        set { EditorPrefs.SetBool("Deftly_WeaponStats", value); } }
    public static bool WeaponSoundsAndTiming { get { return EditorPrefs.GetBool("Deftly_WeaponSoundsAndTiming"); } set { EditorPrefs.SetBool("Deftly_WeaponSoundsAndTiming", value); } }
    public static bool WeaponAttacks {      get { return EditorPrefs.GetBool("Deftly_WeaponAttacks"); }      set { EditorPrefs.SetBool("Deftly_WeaponAttacks", value); } }
    public static bool WeaponSpawns {       get { return EditorPrefs.GetBool("Deftly_WeaponSpawns"); }       set { EditorPrefs.SetBool("Deftly_WeaponSpawns", value); } }
    public static bool WeaponAmmo {         get { return EditorPrefs.GetBool("Deftly_WeaponAmmo"); }         set { EditorPrefs.SetBool("Deftly_WeaponAmmo", value); } }
    public static bool WeaponIk {           get { return EditorPrefs.GetBool("Deftly_WeaponIk"); }           set { EditorPrefs.SetBool("Deftly_WeaponIk", value); } }
    public static bool WeaponImpactTags {   get { return EditorPrefs.GetBool("Deftly_WeaponImpactTags"); }   set { EditorPrefs.SetBool("Deftly_WeaponImpactTags", value); } }

    // Intellect.cs Foldout memory




    // Projectile.cs Foldout memory



    // Spawner.cs Foldout memory
    public static bool SpawnerBasic {       get { return EditorPrefs.GetBool("Deftly_SpawnerBasic"); }      set { EditorPrefs.SetBool("Deftly_SpawnerBasic", value); } }
    public static bool SpawnerPrefabs {     get { return EditorPrefs.GetBool("Deftly_SpawnerPrefabs"); }    set { EditorPrefs.SetBool("Deftly_SpawnerPrefabs", value); } }
    public static bool SpawnerPoints {      get { return EditorPrefs.GetBool("Deftly_SpawnerPoints"); }     set { EditorPrefs.SetBool("Deftly_SpawnerPoints", value); } }


    public static void AddBlackLine()
    {
        GUI.color = Color.black;
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUI.color = Color.white;
    }
}