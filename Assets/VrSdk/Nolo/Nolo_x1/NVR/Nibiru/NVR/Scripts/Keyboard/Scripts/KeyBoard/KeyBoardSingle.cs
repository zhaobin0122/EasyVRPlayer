using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Nvr.Internal
{
    public class KeyBoardSingle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public Transform m_trsSelf;
        public Image m_imageKey;
        public Text m_textKey;
        public string m_strChar;
        public KeyBoardInfo m_CKeyBoardInfo = null;
        public BoxCollider m_colliderKey;

        // Use this for initialization
        void Awake()
        {
            m_trsSelf = this.transform;
            m_imageKey = m_trsSelf.GetComponent<Image>();
            m_textKey = m_trsSelf.GetComponentInChildren<Text>();
            m_colliderKey = m_trsSelf.GetComponent<BoxCollider>();
            m_colliderKey.enabled = false;
        }

        void Start()
        {
            // 怀疑是unity的bug，导致碰撞信息失效
            m_colliderKey.enabled = true;
        }

        public void OnPointerEnter(PointerEventData data)
        {
            //if((m_eKeyBoard)m_CKeyBoardInfo.m_bType == m_eKeyBoard.Add || (m_eKeyBoard)m_CKeyBoardInfo.m_bType == m_eKeyBoard.Space)
            m_imageKey.sprite = Resources.Load<Sprite>("KeyBoard/keyboard_letter_down");
        }

        public void OnPointerExit(PointerEventData data)
        {
            //if ((m_eKeyBoard)m_CKeyBoardInfo.m_bType == m_eKeyBoard.Add || (m_eKeyBoard)m_CKeyBoardInfo.m_bType == m_eKeyBoard.Space)
            m_imageKey.sprite = Resources.Load<Sprite>("KeyBoard/keyboard_letter_up");
        }
    }
}