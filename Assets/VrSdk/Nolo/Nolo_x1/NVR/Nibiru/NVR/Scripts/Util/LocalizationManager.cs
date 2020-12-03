using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nvr.Internal
{
    public class LocalizationManager
    {

        private static LocalizationManager _instance;

        public static LocalizationManager GetInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LocalizationManager();
                }
                return _instance;
            }
        }

        public const string chinese = "Text/Chinese";
        public const string english = "Text/English";


        private Dictionary<string, string> dic_CN = new Dictionary<string, string>();
        private Dictionary<string, string> dic_EN = new Dictionary<string, string>();
        /// <summary>    
        /// 读取配置文件，将文件信息保存到字典里    
        /// </summary>    
        public LocalizationManager()
        {
            // cn
            TextAsset taCN = Resources.Load<TextAsset>(chinese);
            string text = taCN.text;

            string[] linesCN = text.Split('\n');
            foreach (string line in linesCN)
            {
                if (line == null || line.Length <= 1)
                {
                    continue;
                }
                string[] keyAndValue = line.Split('=');
                // Debug.Log("line=" + line + "," + line.Length);
                dic_CN.Add(keyAndValue[0], keyAndValue[1].Replace("\\n", "\n"));
            }

            // en
            TextAsset taEN = Resources.Load<TextAsset>(english);
            text = taEN.text;

            string[] linesEN = text.Split('\n');
            foreach (string line in linesEN)
            {
                if (line == null || line.Length <= 1)
                {
                    continue;
                }
                string[] keyAndValue = line.Split('=');
                // Debug.Log("line=" + line + "," + line.Length);
                dic_EN.Add(keyAndValue[0], keyAndValue[1].Replace("\\n", "\n"));
            }
        }

        /// <summary>    
        /// 获取value    
        /// </summary>    
        /// <param name="key"></param>    
        /// <returns></returns>    
        public string GetValue(string key)
        {
            Dictionary<string, string> dic = getDIC();
            if (dic.ContainsKey(key) == false)
            {
                return null;
            }
            string value = null;
            dic.TryGetValue(key, out value);
            return value;
        }

        public string GetValue(string key, string language)
        {
            Dictionary<string, string> dic = language == chinese ? dic_CN : dic_EN;
            if (dic.ContainsKey(key) == false)
            {
                return null;
            }
            string value = null;
            dic.TryGetValue(key, out value);
            return value;
        }

        private Dictionary<string, string> getDIC()
        {
            return isCN() ? dic_CN : dic_EN;
        }

        private bool isCN()
        {
            if (languageType != null) return languageType == chinese;

            return Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.Chinese
                || Application.systemLanguage == SystemLanguage.ChineseTraditional;
        }

        private string languageType = null;
        public void ChangeLanguage(string language)
        {
            languageType = language;
        }
    }
}