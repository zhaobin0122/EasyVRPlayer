using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using NibiruTask;
namespace Nvr.Internal
{
    public enum m_eKeyBoard : byte
    {
        none = 0,
        Add = 1,
        Delete = 2,
        Space = 3,
        ChangePage = 4,
        ToggleCase = 5,
        Submit = 6,
    }

    public class NibiruKeyBoard
    {

        public static NibiruKeyBoard m_instance = null;

        public static NibiruKeyBoard Instance
        {
            get
            {
                if (m_instance == null || m_instance.m_objSelf == null)
                    m_instance = new NibiruKeyBoard();
                return m_instance;
            }
        }

        public GameObject m_objSelf = null;
        private Transform keyBoardTransform = null;
        private KeyBoardSingle[] m_arrCKeyBoardSingle = null;
        private Text m_textKeyBoard = null;
        /// <summary>
        /// 文本框显示字符串
        /// </summary>
        private string m_strKeyBoard = "";
        private RaycastHit hit;
        private int m_dwPageIndex = 0;
        private int m_dwToggleCase = 0;
        private Text m_text;
        /// <summary>
        /// 最大输入长度
        /// </summary>
        private int m_dwMaxSize = 30;
        /// <summary>
        /// 初始字符串
        /// </summary>
        private string m_strInitChar = " Nibiru KeyBoard ";

        private bool isShowing = false;

        public NibiruKeyBoard()
        {
            // Debug.Log("------>Create NibiruKeyBoard");
            m_objSelf = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Keyboard/NibiruKeyBoard"));
            m_objSelf.SetActive(false);
            keyBoardTransform = m_objSelf.transform;
            keyBoardTransform.position = new Vector3(1000, 1000, 1000);
            m_arrCKeyBoardSingle = keyBoardTransform.GetComponentsInChildren<KeyBoardSingle>();
            m_textKeyBoard = keyBoardTransform.Find("KeyPanel/InputBg/text_KeyBoard").GetComponent<Text>();
            m_textKeyBoard.text = m_strInitChar;
            isShowing = false;
        }

        void ReInit()
        {
            m_textKeyBoard.text = m_strInitChar;
            m_strKeyBoard = "";
            m_dwPageIndex = 0;
            m_dwToggleCase = 0;
            ShowPage(m_dwPageIndex, m_dwToggleCase);
        }

        /// <summary>
        ///  设置输入的字符串需要显示在哪个Text组件
        /// </summary>
        /// <param name="_text"></param>
        public void SetText(Text _text)
        {
            m_text = _text;
            m_textKeyBoard.text = "";
            m_strKeyBoard = "";
        }


        /// <summary>
        /// 显示键盘页数
        /// </summary>
        /// <param name="_pageIndex">键盘索引 ： 0=字母，1=数字</param>
        public void Show(int _pageIndex = 0)
        {
            Show(_pageIndex, new Vector3(0, -0.3f, 1), new Vector3(30, 0, 0));
        }

        public void Show(int _pageIndex, Vector3 position, Vector3 rotation)
        {
            if (isShowing)
            {
                Debug.Log("NibiruKeyBoard is Showing.");
                return;
            }

            m_objSelf.SetActive(true);
            isShowing = true;
            // 设置坐标和旋转
            keyBoardTransform.position = position;
            keyBoardTransform.Rotate(rotation);

            List<KeyBoardInfo> m_listKeyBoard = CoreStaticDataManager.instance.GetKeyBoardInfoByPage(_pageIndex % 2);

            for (int i = 0; i < m_arrCKeyBoardSingle.Length; i++)
            {
                if (i < m_listKeyBoard.Count)
                {
                    m_arrCKeyBoardSingle[i].gameObject.SetActive(true);
                    if ((m_eKeyBoard)m_listKeyBoard[i].m_bType == m_eKeyBoard.Add)
                    {
                        string _strChar = Char(int.Parse(m_listKeyBoard[i].m_strShow_1));
                        m_arrCKeyBoardSingle[i].m_textKey.text = _strChar;
                        m_arrCKeyBoardSingle[i].m_strChar = _strChar;

                    }
                    else
                    {
                        m_arrCKeyBoardSingle[i].m_textKey.text = m_listKeyBoard[i].m_strShow_1;
                        m_arrCKeyBoardSingle[i].m_strChar = m_listKeyBoard[i].m_strShow_1;
                    }
                    m_arrCKeyBoardSingle[i].m_imageKey.GetComponent<RectTransform>().sizeDelta = new Vector2(m_listKeyBoard[i].m_dwScaleX, m_listKeyBoard[i].m_dwScaleY);
                    m_arrCKeyBoardSingle[i].m_trsSelf.localPosition = new Vector3(m_listKeyBoard[i].m_dwPosX, m_listKeyBoard[i].m_dwPosY, 22);
                    m_arrCKeyBoardSingle[i].m_CKeyBoardInfo = m_listKeyBoard[i];
                    m_arrCKeyBoardSingle[i].m_colliderKey.size = new Vector3(m_listKeyBoard[i].m_dwScaleX, m_listKeyBoard[i].m_dwScaleY, 1);

                }
                else
                {
                    m_arrCKeyBoardSingle[i].gameObject.SetActive(false);
                }
            }
        }

        void ShowPage(int _pageIndex, int _toggleCase)
        {
            List<KeyBoardInfo> m_listKeyBoard = CoreStaticDataManager.instance.GetKeyBoardInfoByPage(_pageIndex % 2);
            for (int i = 0; i < m_arrCKeyBoardSingle.Length; i++)
            {
                if (i < m_listKeyBoard.Count)
                {
                    m_arrCKeyBoardSingle[i].gameObject.SetActive(true);
                    if ((m_eKeyBoard)m_listKeyBoard[i].m_bType == m_eKeyBoard.Add)
                    {

                        string _strChar = Char(int.Parse(_toggleCase % 2 == 0 ? m_listKeyBoard[i].m_strShow_1 : m_listKeyBoard[i].m_strShow_2));
                        m_arrCKeyBoardSingle[i].m_textKey.text = _strChar;
                        m_arrCKeyBoardSingle[i].m_strChar = _strChar;
                    }
                    else
                    {
                        m_arrCKeyBoardSingle[i].m_textKey.text = _toggleCase % 2 == 0 ? m_listKeyBoard[i].m_strShow_1 : m_listKeyBoard[i].m_strShow_2;
                        m_arrCKeyBoardSingle[i].m_strChar = _toggleCase % 2 == 0 ? m_listKeyBoard[i].m_strShow_1 : m_listKeyBoard[i].m_strShow_2;
                    }
                    m_arrCKeyBoardSingle[i].m_imageKey.GetComponent<RectTransform>().sizeDelta = new Vector2(m_listKeyBoard[i].m_dwScaleX, m_listKeyBoard[i].m_dwScaleY);
                    m_arrCKeyBoardSingle[i].m_trsSelf.localPosition = new Vector3(m_listKeyBoard[i].m_dwPosX, m_listKeyBoard[i].m_dwPosY, 22);
                    m_arrCKeyBoardSingle[i].m_CKeyBoardInfo = m_listKeyBoard[i];
                    m_arrCKeyBoardSingle[i].m_colliderKey.size = new Vector3(m_listKeyBoard[i].m_dwScaleX, m_listKeyBoard[i].m_dwScaleY, 1);
                }
                else
                {
                    m_arrCKeyBoardSingle[i].gameObject.SetActive(false);
                }
            }
        }

        public void OnPressEnterByCamera()
        {
            Transform mTransform = NvrViewer.Instance.GetHead().transform;
            if (Physics.Raycast(mTransform.position, mTransform.TransformDirection(Vector3.forward), out hit))
            {
                OnPressEnter(hit.collider.gameObject);
            }
        }

        public void OnPressEnterByQuat()
        {
            if(NvrControllerHelper.ControllerRaycastObject != null)
            {
                OnPressEnter(NvrControllerHelper.ControllerRaycastObject);
            }
        }

        public void OnPressEnterByMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 20))
            {
                OnPressEnter(hit.collider.gameObject);
            }
        }

        public void OnPressEnter(GameObject hit)
        {
            if (hit.name.Substring(0, 3) == "Key")
            {
                KeyBoardSingle _CKeyBoardSingle = hit.GetComponent<KeyBoardSingle>();

                m_eKeyBoard keyType = (m_eKeyBoard)_CKeyBoardSingle.m_CKeyBoardInfo.m_bType;
                switch (keyType)
                {
                    case m_eKeyBoard.Add:
                        if (m_strKeyBoard.Length >= m_dwMaxSize)
                            return;
                        m_strKeyBoard += _CKeyBoardSingle.m_strChar;
                        m_textKeyBoard.text = m_strKeyBoard;
                        break;
                    case m_eKeyBoard.ChangePage:
                        m_dwPageIndex++;
                        m_dwToggleCase = 0;
                        ShowPage(m_dwPageIndex, m_dwToggleCase);
                        break;
                    case m_eKeyBoard.Delete:
                        if (m_strKeyBoard.Length > 0)
                        {
                            m_strKeyBoard = m_strKeyBoard.Substring(0, m_strKeyBoard.Length - 1);
                            m_textKeyBoard.text = m_strKeyBoard;
                        }
                        break;
                    case m_eKeyBoard.Space:
                        if (m_strKeyBoard.Length >= m_dwMaxSize)
                            return;
                        m_strKeyBoard += " ";
                        m_textKeyBoard.text = m_strKeyBoard;
                        break;
                    case m_eKeyBoard.Submit:
                        ReInit();
                        Dismiss();
                        break;
                    case m_eKeyBoard.ToggleCase:
                        m_dwToggleCase++;
                        ShowPage(m_dwPageIndex, m_dwToggleCase);
                        break;
                }

                if (m_text != null && keyType != m_eKeyBoard.Submit)
                {
                    m_text.text = m_strKeyBoard;
                }
            }
        }

        public void Dismiss()
        {
            if (!isShowing) return;
            isShowing = false;
            m_objSelf.SetActive(false);
            GameObject.DestroyImmediate(m_objSelf);
            m_objSelf = null;
            if (keyBoardTransform != null)
            {
                // 很远不可见
                keyBoardTransform.position = new Vector3(1000, 1000, 1000);
                keyBoardTransform.rotation = new Quaternion(0, 0, 0, 0);
            }
        }

        public bool isShown()
        { return isShowing; }


        public string Char(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                return "" + (char)asciiCode;
            }
            else
            {
                throw new System.Exception("ASCII code is not valid. ");
            }
        }

        /// <summary>
        ///  获取当前输入的字符串
        /// </summary>
        /// <returns></returns>
        public string GetKeyBoardString()
        {
            return m_strKeyBoard;
        }

        public Transform GetKeyBoardTransform()
        {
            return keyBoardTransform;
        }
    }
}