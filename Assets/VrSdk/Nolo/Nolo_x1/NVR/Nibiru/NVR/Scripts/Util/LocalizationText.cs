using UnityEngine;
namespace Nvr.Internal
{
    public class LocalizationText : MonoBehaviour
    {

        public string key = " ";
        void Start()
        {
            GetComponent<TextMesh>().text = LocalizationManager.GetInstance.GetValue(key);
        }

        public void UpdateKey(string keyValue)
        {
            this.key = keyValue;
            GetComponent<TextMesh>().text = LocalizationManager.GetInstance.GetValue(key);
        }

        public void refresh(string language)
        {
            GetComponent<TextMesh>().text = LocalizationManager.GetInstance.GetValue(key, language);
        }
    }
}