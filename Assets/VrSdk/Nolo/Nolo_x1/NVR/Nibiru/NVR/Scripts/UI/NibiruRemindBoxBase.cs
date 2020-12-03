using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nvr.Internal
{
    public class NibiruRemindBoxBase : MonoBehaviour
    {
        //private GameObject box;
        [NonSerialized]
        public GameObject remindbox;
        private static bool isClose = false;
        private Text defaultText;
        private GameObject cameraObject;
        private GameObject tagImage;
        float time = 0;
        float timeEnd;

        public bool Showing()
        {
            return remindbox != null;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public bool Init(float timeend)
        {
            time = 0;
            timeEnd = timeend;
            cameraObject = GameObject.Find("MainCamera");
            if (remindbox != null)
            {
                Destroy(remindbox);
            }
            remindbox = Instantiate(Resources.Load<GameObject>("RemindBox/RemindBox"));
            remindbox.GetComponent<Canvas>().worldCamera = cameraObject.GetComponent<Camera>();
            // 保证UI显示在视角正前方
            Vector3 forward = cameraObject.transform.forward * 3;
            remindbox.transform.localPosition =
                cameraObject.transform.position +
                new Vector3(forward.x, forward.y, forward.z);
            remindbox.transform.localRotation = cameraObject.transform.rotation;

            defaultText = remindbox.GetComponentInChildren<Text>();
            isClose = false;
            return true;
        }
        /// <summary>
        /// 创建Image
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        public void Create(string name, Vector3 position, Vector2 size)
        {
            GameObject image = new GameObject(name, typeof(Image));
            //判断是否是滑块
            if(name.Equals("VolumeTag"))
            {
                tagImage = image;
            }
            image.GetComponent<Image>().sprite = Resources.Load<Sprite>("RemindBox/" + name);
            image.transform.SetParent(remindbox.transform);
            image.GetComponent<RectTransform>().sizeDelta = size;
            image.GetComponent<Image>().rectTransform.localPosition = position;
            image.GetComponent<Image>().rectTransform.localScale = new Vector3(1f, 1f, 1f);
            image.GetComponent<Image>().rectTransform.localRotation = Quaternion.identity;
            image.GetComponent<Image>().raycastTarget = false;
        }
        /// <summary>
        /// 创建text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="context"></param>
        /// <param name="position"></param>
        public void Create(string name, string context, Vector3 position, Vector2 size)
        {
            GameObject text = new GameObject(name);
            text.AddComponent<Text>();
            Text mText = text.GetComponent<Text>();
            mText.font = defaultText.font;
            text.transform.SetParent(remindbox.transform);
            text.GetComponent<RectTransform>().sizeDelta = size;
            mText.alignment = defaultText.alignment;
            mText.rectTransform.localPosition = position;
            mText.rectTransform.localRotation = Quaternion.identity;
            mText.text = context;
            mText.fontSize = 50;
            mText.rectTransform.localScale = new Vector3(0.3f, 0.3f, 1f);
            mText.raycastTarget = false;
        }
        /// <summary>
        /// 创建button
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="action"></param>
        public void Create(string name, Vector3 position,Vector2 size, NibiruRemindBoxEvent.RemindBoxEvent action)
        {
            GameObject quad = (GameObject)Instantiate(Resources.Load<GameObject>("RemindBox/Quad"),remindbox.transform);
            quad.name = name;
            quad.transform.localPosition = position;
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = new Vector3(size.x, size.y, 1);
            quad.GetComponent<NibiruRemindBoxEvent>().handleRemindBox = action;
            quad.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0f);
            // 怀疑是unity的bug，导致碰撞信息失效
            quad.GetComponent<BoxCollider>().enabled = false;
            quad.GetComponent<BoxCollider>().enabled = true;
        }
        /// <summary>
        /// 创建tag,重置时间
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public void Create(Vector3 position,Vector2 size)
        {
            if(time>= timeEnd)
            {
                Image[] contents = remindbox.GetComponentsInChildren<Image>();
                foreach (Image child in contents)
                {
                    child.color = new Color(255, 255, 255, 1);
                }
                Text[] context = remindbox.GetComponentsInChildren<Text>();
                foreach (Text child in context)
                {
                    child.color = new Color(255, 255, 255, 1);
                }
            }
            time = 0;
            if (tagImage != null)
            {
                Destroy(tagImage);
            }
            Create("VolumeTag", position, size);
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            isClose = true;
        }
        public void ReleaseDestory()
        {
            if (remindbox != null)
            {
                Destroy(remindbox);
            }
        }
        /// <summary>
        /// 淡出
        /// </summary>
        /// <param name="remindbox"></param>
        public void FadeOut(GameObject remindbox)
        {
            if (remindbox != null)
            {
                Image[] contents = remindbox.GetComponentsInChildren<Image>();
                foreach (Image child in contents)
                {
                    child.color = new Color(255, 255, 255, child.color.a - 0.01f);
                }
                Text[] context = remindbox.GetComponentsInChildren<Text>();
                foreach (Text child in context)
                {
                    child.color = new Color(255, 255, 255, child.color.a - 0.01f);
                }
                MeshRenderer[] meshRenderer = remindbox.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshRenderer)
                {
                    mr.material.color = new Color(mr.material.color.r, mr.material.color.g, mr.material.color.b, mr.material.color.a - 0.01f);
                }
                if (context[0].color.a <= 0)
                {
                    Destroy(remindbox);
                    time = 0;
                    isClose = false;
                    //清除原点选中效果
                    NvrReticle mNvrReticle = NvrViewer.Instance.GetNvrReticle();
                    if(mNvrReticle != null)
                    {
                        mNvrReticle.OnGazeExit(null, null);
                    }
                }
            }
        }

        void Update()
        {
            if (isClose)
            {
                time += Time.deltaTime;
                if (time >= timeEnd)
                {
                    FadeOut(remindbox);
                }
            }
        }
    }
}
