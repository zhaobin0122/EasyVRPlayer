
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

namespace Oahc
{
    public class ObjModelLoader : MonoBehaviour
    {
        public enum FileType
        {
            OBJ, MTL, PNG
        }

        private Vector3 modelScaleSize = Vector3.one;
        private Transform parentTransform;

        public void LoadObjFile(string objPath, Transform parent) {
            LoadObjFile(objPath, Vector3.one, parent);
        }

        public void LoadObjFile(string objPath, Vector3 scaleSize, Transform parent)
        {
            parentTransform = parent;
            modelScaleSize = scaleSize;
            Thread readFileThread = new Thread(new ParameterizedThreadStart(ReadFiles));
            readFileThread.Start(objPath);
        }

        private Dictionary<FileType, byte[]> FileDict = new Dictionary<FileType, byte[]>();
        private bool loaded = false;
        private void Update()
        {
            if (FileDict.Count == 3 && !loaded)
            {
                CreateModelObject(null, "OBJ_Model");
                loaded = true;
            }
        }

        public GameObject GetObjModel()
        {
            return ObjModel;
        }

        void ReadFiles(object objPath)
        {
            ReadFileCore(FileType.OBJ, (string)objPath);
            ReadFileCore(FileType.MTL, ((string)objPath).Replace("obj", "mtl"));
            ReadFileCore(FileType.PNG, ((string)objPath).Replace("obj", "png"));
        }

        void ReadFileCore(FileType fileType, string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, System.IO.FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            byte[] binary = new byte[fileStream.Length];
            fileStream.Read(binary, 0, (int)fileStream.Length);
            if (!FileDict.ContainsKey(fileType))
            {
                FileDict.Add(fileType, binary);
            }
            else
            {
                FileDict[fileType] = binary;
            }

            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;
        }

        GameObject ObjModel = null;
        void CreateModelObject(string filePath, string objName)
        {
            //解析内容
            ObjMesh objInstace = new ObjMesh();
            string objtext = System.Text.Encoding.Default.GetString(FileDict[FileType.OBJ]);
            objInstace = objInstace.LoadFromObj(objtext);

            //计算网格
            Mesh mesh = new Mesh();
            mesh.name = "Mesh";
            mesh.vertices = objInstace.VertexArray;
            mesh.triangles = objInstace.TriangleArray;
            if (objInstace.UVArray.Length > 0)
                mesh.uv = objInstace.UVArray;
            if (objInstace.NormalArray.Length > 0)
                mesh.normals = objInstace.NormalArray;
            mesh.RecalculateBounds();

            //生成物体
            ObjModel = new GameObject(objName);
            if (parentTransform != null)
            {
                ObjModel.transform.SetParent(parentTransform);
            }
            ObjModel.transform.localScale = modelScaleSize;
            ObjModel.transform.localPosition = Vector3.zero;
            ObjModel.transform.localRotation = Quaternion.identity;
            MeshFilter meshFilter = ObjModel.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = ObjModel.AddComponent<MeshRenderer>();

            string directory = filePath == null ? null : Path.GetDirectoryName(filePath);
            string mtltext = System.Text.Encoding.Default.GetString(FileDict[FileType.MTL]);
            Material[] materials = ObjMaterial.Instance.LoadFormMtl(mtltext, directory, FileDict[FileType.PNG]);
            meshRenderer.materials = materials;
        }

        public void LoadFormFile(string objName, string modelFilePath, string texturePath)
        {
            if (!File.Exists(modelFilePath))
                Debug.Log("请确认obj模型文件是否存在!");
            if (!modelFilePath.EndsWith(".obj"))
                Debug.Log("请确认这是一个obj模型文件");

            //读取内容
            StreamReader reader = new StreamReader(modelFilePath, Encoding.Default);
            string content = reader.ReadToEnd();
            reader.Close();

            //解析内容
            ObjMesh objInstace = new ObjMesh();
            objInstace = objInstace.LoadFromObj(content);

            //计算网格
            Mesh mesh = new Mesh();
            mesh.vertices = objInstace.VertexArray;
            mesh.triangles = objInstace.TriangleArray;
            if (objInstace.UVArray.Length > 0)
                mesh.uv = objInstace.UVArray;
            if (objInstace.NormalArray.Length > 0)
                mesh.normals = objInstace.NormalArray;
            mesh.RecalculateBounds();

            //生成物体
            GameObject go = new GameObject(objName);
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();

            //获取mtl文件路径
            string mtlFilePath = modelFilePath.Replace(".obj", ".mtl");
            StreamReader mtlReader = new StreamReader(mtlFilePath, Encoding.Default);
            string mtlContent = mtlReader.ReadToEnd();
            mtlReader.Close();
            //从mtl文件中加载材质
            Material[] materials = ObjMaterial.Instance.LoadFormMtl(mtlContent, texturePath, null);

            meshRenderer.materials = materials;
        }



        //IEnumerator LoadFileAsync(FileType fileType, string filePath)
        //{
        //    WWW www = new WWW(filePath);
        //    yield return null;
        //    yield return www;
        //    Debug.Log("Load." + filePath + ",isDone=" + www.isDone);
        //    if ((www.error == null || www.error.Length == 0) && www.isDone)
        //    {
        //        // 保持顺序
        //        if (!FileDict.ContainsKey(fileType))
        //        {
        //            FileDict.Add(fileType, Encoding.Default.GetBytes((www.text)));
        //        }
        //        else
        //        {
        //            FileDict[fileType] = Encoding.Default.GetBytes(www.text);
        //        }

        //        www.Dispose();

        //        switch (fileType)
        //        {
        //            case FileType.OBJ:
        //                StartCoroutine(LoadFileAsync(FileType.MTL, filePath.Replace("obj", "mtl")));
        //                break;
        //            case FileType.MTL:
        //                StartCoroutine(LoadFileAsync(FileType.PNG, filePath.Replace("mtl", "png")));
        //                break;
        //            case FileType.PNG:
        //                StartCoroutine(CreateModelObject(filePath, "Model"));
        //                break;
        //            default: break;
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("Load Error :" + www.error + "," + www.text);
        //    }
        //    Debug.Log("LoadFileAsync finish " + fileType);
        //}
    }
}