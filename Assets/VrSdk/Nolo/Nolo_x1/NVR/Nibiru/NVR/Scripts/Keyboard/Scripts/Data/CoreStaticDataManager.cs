using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DURATYPE = System.UInt16;
using POSTYPE = System.UInt16;
using DBSERIAL = System.Int64;
using SLOTCODE = System.UInt32;
using CODETYPE = System.UInt32;
using KEYTYPE = System.UInt32;
using LEVELTYPE = System.UInt16;
using SLOTIDX = System.Byte;
using MONEY = System.UInt64;
using DAMAGETYPE = System.UInt32;
using WORD = System.UInt16;
using time_t = System.Int64;
namespace Nvr.Internal
{
    public class DataRow : List<string>
    {
        public DataRow(string data) : base()
        {
            string[] datas = data.Split('\t');
            this.AddRange(datas);
        }

        public string Pull()
        {
            string s = this[0];
            this.RemoveAt(0);
            return s;
        }

        public byte Pull(byte t)
        {
            string s = Pull();
            return Convert.ToByte(s);
        }

        public UInt16 Pull(UInt16 t)
        {
            return Convert.ToUInt16(Pull());
        }

        public bool Pull(bool t)
        {
            return Convert.ToBoolean(Pull(1));
        }

        public UInt32 Pull(UInt32 t)
        {
            return Convert.ToUInt32(Pull());
        }

        public Int32 Pull(Int32 t)
        {
            return Convert.ToInt32(Pull());
        }

        public float Pull(float t)
        {
            return float.Parse(Pull());
        }

        public Enum Pull(Enum t)
        {
            return (Enum)Enum.Parse(t.GetType(), Pull());
        }
    }

    public class DataTable : List<DataRow>
    {
        public DataTable(string data) : base()
        {
            string[] rows = data.Split('\n');

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].StartsWith("//") || string.IsNullOrEmpty(rows[i]))
                    continue;

                this.Add(new DataRow(rows[i].Trim()));
            }
        }
    }

    /// <summary>
    /// 静态数据
    /// </summary>
    public class CoreStaticDataManager
    {
        private static CoreStaticDataManager m_instance = null;
        public static CoreStaticDataManager instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new CoreStaticDataManager();
                return m_instance;
            }
        }

        #region test
        public static string dataPath = UnityEngine.Application.dataPath;
        #endregion

        public readonly KeyBoardInfo[] m_arrKeyBoardInfo = null;

        public CoreStaticDataManager()
        {

            m_arrKeyBoardInfo = InitKeyBoardInfo();

        }

        private T[] StaticMultiRowTableInit<T>(string tableName)
        {
            string dataString = UnityEngine.Resources.Load<UnityEngine.TextAsset>(string.Concat("Text/", tableName)).text;

            DataTable cards = new DataTable(dataString);

            T[] tValue = (T[])Activator.CreateInstance(typeof(T[]), cards.Count);

            for (int i = 0; i < tValue.Length; i++)
            {
                try
                {
                    tValue[i] = (T)Activator.CreateInstance(typeof(T), cards[i]);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Break();
                    UnityEngine.Debug.LogWarning("e   " + e.Message + "  i " + i + "  tableName   " + tableName);
                }
            }

            return tValue;
        }

        #region 初始化 


        private KeyBoardInfo[] InitKeyBoardInfo()
        {
            return StaticMultiRowTableInit<KeyBoardInfo>("KeyBoardInfo");
        }

        #endregion

        #region 获取
        public List<KeyBoardInfo> GetKeyBoardInfoByPage(int _dwPageIndex)
        {
            List<KeyBoardInfo> m_listTemp = new List<KeyBoardInfo>();
            for (int i = 0; i < m_arrKeyBoardInfo.Length; i++)
            {
                if (m_arrKeyBoardInfo[i].m_dwPage == _dwPageIndex)
                {
                    m_listTemp.Add(m_arrKeyBoardInfo[i]);
                }
            }
            return m_listTemp;

        }

        #endregion

    }

    /// <summary>
    /// 关卡详细数据
    /// </summary>
    public class LevelStaticInfo
    {
        public LevelStaticInfo(DataRow data)
        {
            m_dwLevel = data.Pull(m_dwLevel);
            m_dwWidth = data.Pull(m_dwWidth);
            m_dwInitCube = data.Pull(m_dwInitCube);
            m_dwOnceAddCube = data.Pull(m_dwOnceAddCube);
            m_dwGate = data.Pull(m_dwGate);
        }
        /// <summary>
        /// 关卡
        /// </summary>
        public int m_dwLevel;
        /// <summary>
        /// 宽
        /// </summary>
        public int m_dwWidth;
        /// <summary>
        /// 初始立方体
        /// </summary>
        public int m_dwInitCube;
        /// <summary>
        /// 每次增加立方体数量
        /// </summary>
        public int m_dwOnceAddCube;
        /// <summary>
        /// 门
        /// </summary>
        public int m_dwGate;

    }

    public class KeyBoardInfo
    {

        public KeyBoardInfo(DataRow data)
        {
            m_dwID = data.Pull(m_dwID);
            m_dwPage = data.Pull(m_dwPage);
            m_strShow_1 = data.Pull();
            m_strShow_2 = data.Pull();
            m_strShow = data.Pull();
            m_dwPosX = data.Pull(m_dwPosX);
            m_dwPosY = data.Pull(m_dwPosY);
            m_dwScaleX = data.Pull(m_dwScaleX);
            m_dwScaleY = data.Pull(m_dwScaleY);
            m_bType = data.Pull(m_bType);
        }

        public int m_dwID;
        public int m_dwPage;
        public string m_strShow_1;
        public string m_strShow_2;
        public string m_strShow;
        public float m_dwPosX;
        public float m_dwPosY;
        public int m_dwScaleX;
        public int m_dwScaleY;
        public byte m_bType;
    }
}