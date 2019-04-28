using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace Capstones.UnityFramework
{
    public class LanguageConverter
    {
        private const string JSONPATH = "Assets/CapstonesRes/Game/Config/languagePack.json";
        private static Dictionary<string, string> langDict = new Dictionary<string,string>();

        public static bool isInitialized = false;

        public static void InitData()
        {
            langDict.Clear();

            string jsonText = null;
            // 该路径下的语言包是为了翻译公司可以方便查看翻译后的效果
            string topPriorityJsonPath = Path.Combine(Application.persistentDataPath, "languagePack.json");

            if (File.Exists(topPriorityJsonPath))
            {
                jsonText = File.ReadAllText(topPriorityJsonPath);
            }
            else
            {
                TextAsset jsonAsset = ResManager.LoadRes(JSONPATH, typeof(TextAsset)) as TextAsset;

                if (jsonAsset != null)
                {
                    jsonText = jsonAsset.text;
                }
            }

            if (string.IsNullOrEmpty(jsonText))
            {
                return;
            }

            JSONObject json = new JSONObject(jsonText);

            for (int i = 0, len = json.list.Count; i < len; i++)
            {
                string key = json.keys[i];
                string val = json.list[i].str;

                if (langDict.ContainsKey(key))
                {
                    if (GLog.IsLogWarningEnabled) GLog.LogWarning(string.Format("Dupliciate key in langDict: {0}", key));
                }
                else
                {
                    langDict.Add(key, val);
                }
            }

            isInitialized = true;
        }

        public static string GetLangValue(string key, params string[] args)
        {
            if (!isInitialized)
            {
                InitData();
            }

            string val = string.Empty;
            langDict.TryGetValue(key, out val);

            if (val == null)
            {
                val = key;
            }

            if (args.Length == 0)
            {
                return val;
            }

            StringBuilder sb = new StringBuilder(val);
            StringBuilder formatStringBuilder = new StringBuilder(3);

            for (int i = 0, len = args.Length; i < len; i++)
            {
                string item = args[i];
                formatStringBuilder.Remove(0, formatStringBuilder.Length);
                formatStringBuilder.Append("{").Append(i + 1).Append("}");
                string formatStr = formatStringBuilder.ToString();
                int braceStartIndex = val.IndexOf(formatStr);

                if (braceStartIndex >= 0)
                {
                    sb.Replace(formatStr, item);
                }
                else
                {
                    formatStringBuilder.Remove(0, formatStringBuilder.Length);
                    formatStringBuilder.Append("{").Append(i + 1).Append(":");
                    formatStr = formatStringBuilder.ToString();
                    braceStartIndex = val.IndexOf(formatStr);

                    if (braceStartIndex >= 0)
                    {
                        int unitsStrStartIndex = braceStartIndex + formatStr.Length;
                        int braceEndIndex = val.IndexOf("}", unitsStrStartIndex);

                        if (braceEndIndex > 0)
                        {
                            formatStr = val.Substring(braceStartIndex, braceEndIndex - braceStartIndex + 1);
                            StringBuilder itemBuilder = new StringBuilder(item, formatStr.Length);
                            string unitsStr = val.Substring(unitsStrStartIndex, braceEndIndex - unitsStrStartIndex);
                            string[] unitsArr = unitsStr.Split('|');

                            if (unitsArr.Length == 1)
                            {
                                itemBuilder.Append(unitsArr[0]);
                            }
                            else
                            {
                                double itemNum = double.Parse(item);
                                itemBuilder.Append(unitsArr[(itemNum > 1 ? 1 : 0)]);
                            }

                            sb.Replace(formatStr, itemBuilder.ToString());
                        }
                    }
                }
            }

            return sb.ToString();
        }

        public static bool ContainsKey(string key)
        {
            if (!isInitialized)
            {
                InitData();
            }

            return langDict.ContainsKey(key);
        }

        public static void IterateText(Transform trans)
        {
            Text[] textArr = trans.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < textArr.Length; i++)
            {
                string txt = textArr[i].text;
                int posIndex = txt.IndexOf('@');
                if (posIndex >= 0)
                {
                    string langValue = GetLangValue(txt.Substring(posIndex + 1));
                    textArr[i].text = txt.Replace(txt.Substring(posIndex), langValue);
                }
            }
        }
    }
}
