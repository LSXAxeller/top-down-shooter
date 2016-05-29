// (c) Copyright Cleverous 2015. All rights reserved.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

//using UnityEditor;

namespace Deftly
{
    public static class Options
    {
        /// <summary> Serialized data options, string/int/float/bool etc. Anything that can be pushed to XML.</summary>
        public static OptionsData Data;
        /// <summary> References to objects in the hierarchy and such. Anything that cannot be pushed to XML.</summary>
        public static OptionsRefs Refs;

        public const string AssetVersion = "0.6.9";
        public const float MapVersion = 0.1f;

        public static string FileLocation = Application.dataPath + "/Deftly/Core/Resources";
        public static string FileName = "OptionsData";
        public static string FileNameExt = ".xml";
        private static string _dataAsString;

        public static void Init()
        {
            LoadStoredData();
            UpdateGameplayRefs();
        }

        public static void Save(OptionsData newPush)
        {
            Data.UseFloatingText = newPush.UseFloatingText;
            Data.FloatingTextPrefabName = newPush.FloatingTextPrefabName;
            Data.Difficulty = newPush.Difficulty;
            Data.WeaponPickupAutoSwitch = newPush.WeaponPickupAutoSwitch;
            Data.UseRpgElements = newPush.UseRpgElements;
            Data.FriendlyFire = newPush.FriendlyFire;

            UpdateGameplayRefs();
            
            _dataAsString = SerializeDataToString(Data);
            CreateXml();
        }
        public static OptionsData LoadStoredData()
        {
            if (Application.isEditor && _dataAsString == "") CreateXml();
            LoadStringFromResources();
            Data = (OptionsData)DeserializeStringToData(_dataAsString);
            return Data;
        }
        private static void UpdateGameplayRefs()
        {
            Refs.TextPrefab = Resources.Load(Data.FloatingTextPrefabName) as GameObject;
        }
        private static void LoadStringFromResources()
        {
            TextAsset binary = Resources.Load<TextAsset>(FileName);
            _dataAsString = binary.text;
        }

        private static string UtfToString(byte[] bytes)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(bytes);
            return (constructedString);
        }
        private static byte[] StringToUtf(string xml)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(xml);
            return byteArray;
        }

        private static string SerializeDataToString(OptionsData staticOptions)
        {
            MemoryStream ms = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(typeof(OptionsData));
            XmlTextWriter tw = new XmlTextWriter(ms, Encoding.UTF8);
            xs.Serialize(tw, staticOptions);
            ms = (MemoryStream)tw.BaseStream;

            string xmlizedString = UtfToString(ms.ToArray());
            return xmlizedString; 
        }
        private static object DeserializeStringToData(string stringData)
        {
            XmlSerializer xs = new XmlSerializer(typeof (OptionsData));
            MemoryStream ms = new MemoryStream(StringToUtf(stringData));
            return xs.Deserialize(ms);
        }
        private static void CreateXml()
        {
            StreamWriter writer;
            FileInfo t = new FileInfo(FileLocation + "\\" + FileName + FileNameExt);
            if (!t.Exists)
            {
                writer = t.CreateText();
            }
            else
            {
                t.Delete();

                writer = t.CreateText();
            }
            writer.Write(_dataAsString);
            writer.Close();

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            Debug.Log("Deftly: Options Saved");
        }

    }

    public struct OptionsData
    {
        public float Difficulty;
        public string FloatingTextPrefabName;
        public bool UseFloatingText;
        public bool WeaponPickupAutoSwitch;
        public bool UseRpgElements;
        public bool FriendlyFire;
    }
    public struct OptionsRefs
    {
        public GameObject TextPrefab;
    }
}