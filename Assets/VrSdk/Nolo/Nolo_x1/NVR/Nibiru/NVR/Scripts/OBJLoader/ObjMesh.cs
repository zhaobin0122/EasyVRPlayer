
using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace Oahc
{
    public class ObjMesh
    {
        /// <summary>
        /// UV坐标列表
        /// </summary>
        private List<Vector3> uvArrayList;

        /// <summary>
        /// 法线列表
        /// </summary>
        private List<Vector3> normalArrayList;

        /// <summary>
        /// 顶点列表
        /// </summary>
        private List<Vector3> vertexArrayList;

        /// <summary>
        /// 面相关的顶点索引、法线索引、UV索引列表
        /// </summary>
        private List<Vector3> faceVertexNormalUV;

        /// <summary>
        /// UV坐标数组
        /// </summary>
        public Vector2[] UVArray;

        /// <summary>
        /// 法线数组
        /// </summary>
        public Vector3[] NormalArray;

        /// <summary>
        /// 顶点数组
        /// </summary>
        public Vector3[] VertexArray;

        /// <summary>
        /// 面数组
        /// </summary>
        public int[] TriangleArray;

        /// <summary>
        /// 构造函数	/// </summary>
        public ObjMesh()
        {
            //初始化列表
            this.uvArrayList = new List<Vector3>();
            this.normalArrayList = new List<Vector3>();
            this.vertexArrayList = new List<Vector3>();
            this.faceVertexNormalUV = new List<Vector3>();
        }


        /// <summary>
        /// 从一个文本化后的.obj文件中加载模型
        /// </summary>
        public ObjMesh LoadFromObj(string objText)
        {
            if (objText.Length <= 0)
                return null;

            //v这一行在3dsMax中导出的.obj文件
            //  前面是两个空格后面是一个空格
            objText = objText.Replace("  ", " ");

            //将文本化后的obj文件内容按行分割
            string[] allLines = objText.Split('\n');
            foreach (string line in allLines)
            {
                //将每一行按空格分割
                string[] chars = line.Split(' ');
                //根据第一个字符来判断数据的类型
                switch (chars[0])
                {
                    case "v":
                        //处理顶点
                        this.vertexArrayList.Add(new Vector3(
                            ConvertToFloat(chars[1]),
                            ConvertToFloat(chars[2]),
                            ConvertToFloat(chars[3]))
                        );
                        break;
                    case "vn":
                        //处理法线
                        this.normalArrayList.Add(new Vector3(
                            ConvertToFloat(chars[1]),
                            ConvertToFloat(chars[2]),
                            ConvertToFloat(chars[3]))
                        );
                        break;
                    case "vt":
                        //处理UV
                        this.uvArrayList.Add(new Vector3(
                            ConvertToFloat(chars[1]),
                            ConvertToFloat(chars[2]))
                        );
                        break;
                    case "f":
                        //处理面
                        GetTriangleList(chars);
                        break;
                }
            }

            //合并三角面
            Combine();
            return this;
        }

        /// <summary>
        /// 合并三角面
        /// </summary>
        private void Combine()
        {
            //使用一个字典来存储要合并的索引信息
            Dictionary<int, ArrayList> toCambineList = new Dictionary<int, ArrayList>();
            for (int i = 0; i < faceVertexNormalUV.Count; i++)
            {
                if (faceVertexNormalUV[i] != Vector3.zero)
                {
                    //相同索引的列表
                    ArrayList SameIndexList = new ArrayList();
                    SameIndexList.Add(i);
                    for (int j = 0; j < faceVertexNormalUV.Count; j++)
                    {
                        if (faceVertexNormalUV[j] != Vector3.zero)
                        {
                            if (i != j)
                            {
                                //如果顶点索引和法线索引相同，说明它们在一个面上
                                Vector3 iTemp = (Vector3)faceVertexNormalUV[i];
                                Vector3 jTemp = (Vector3)faceVertexNormalUV[j];
                                if (iTemp.x == jTemp.x && iTemp.y == jTemp.y)
                                {
                                    //将索引相同索引列表然后将其重置为零向量
                                    //PS:这是个危险的地方，如果某个索引信息为Vector3.Zero
                                    //就会被忽略过去，可是貌似到目前为止没有发现为Vector3.Zero的情况
                                    SameIndexList.Add(j);
                                    faceVertexNormalUV[j] = Vector3.zero;
                                }
                            }
                        }
                    }
                    //用一个索引来作为字典的键名，这样它可以代替对应列表内所有索引
                    toCambineList.Add(i, SameIndexList);
                }
            }

            //初始化各个数组
            this.VertexArray = new Vector3[toCambineList.Count];
            this.UVArray = new Vector2[toCambineList.Count];
            this.NormalArray = new Vector3[toCambineList.Count];
            this.TriangleArray = new int[faceVertexNormalUV.Count];

            //定义遍历字典的计数器
            int count = 0;

            //遍历词典
            foreach (KeyValuePair<int, ArrayList> IndexTtem in toCambineList)
            {
                //根据索引给面数组赋值
                foreach (int item in IndexTtem.Value)
                {
                    TriangleArray[item] = count;
                }

                //当前的顶点、UV、法线索引信息
                Vector3 VectorTemp = (Vector3)faceVertexNormalUV[IndexTtem.Key];

                //给顶点数组赋值
                VertexArray[count] = (Vector3)vertexArrayList[(int)VectorTemp.x - 1];

                //给UV数组赋值
                if (uvArrayList.Count > 0)
                {
                    Vector3 tVec = (Vector3)uvArrayList[(int)VectorTemp.y - 1];
                    UVArray[count] = new Vector2(tVec.x, tVec.y);
                }

                //给法线数组赋值
                if (normalArrayList.Count > 0)
                {
                    NormalArray[count] = (Vector3)normalArrayList[(int)VectorTemp.z - 1];
                }

                count++;
            }
        }

        /// <summary>
        /// 获取面列表.
        /// </summary>
        /// <param name="chars">Chars.</param>
        private void GetTriangleList(string[] chars)
        {
            List<Vector3> indexVectorList = new List<Vector3>();
            List<Vector3> triangleList = new List<Vector3>();

            for (int i = 1; i < chars.Length; ++i)
            {
                //将每一行按照空格分割后从第一个元素开始
                //按照/继续分割可依次获得顶点索引、法线索引和UV索引
                string[] indexs = chars[i].Split('/');
                if (indexs.Length < 3) continue;

                Vector3 indexVector = new Vector3(0, 0);
                //顶点索引
                indexVector.x = ConvertToInt(indexs[0]);
                //法线索引
                if (indexs.Length > 1)
                {
                    if (indexs[1] != "")
                        indexVector.y = ConvertToInt(indexs[1]);
                }
                //UV索引
                if (indexs.Length > 2)
                {
                    if (indexs[2] != "")
                        indexVector.z = ConvertToInt(indexs[2]);
                }

                //将索引向量加入列表中
                indexVectorList.Add(indexVector);
            }

            //这里需要研究研究
            for (int j = 1; j < indexVectorList.Count - 1; ++j)
            {
                //按照0,1,2这样的方式来组成面
                triangleList.Add(indexVectorList[0]);
                triangleList.Add(indexVectorList[j]);
                triangleList.Add(indexVectorList[j + 1]);
            }

            //添加到索引列表
            foreach (Vector3 item in triangleList)
            {
                faceVertexNormalUV.Add(item);
            }
        }

        /// <summary>
        /// 将一个字符串转换为浮点类型
        /// </summary>
        /// <param name="s">待转换的字符串</param>
        /// <returns></returns>
        private float ConvertToFloat(string s)
        {
            return (float)System.Convert.ToDouble(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 将一个字符串转化为整型	/// </summary>
        /// <returns>待转换的字符串</returns>
        /// <param name="s"></param>
        private int ConvertToInt(string s)
        {
            return System.Convert.ToInt32(s, CultureInfo.InvariantCulture);
        }

    }
}