//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
namespace Nvr.Internal
{
    public struct PointerEventArgs
    {
        public uint controllerIndex;
        public uint flags;
        public float distance;
        public Transform target;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);


    public class NvrLaserPointer : MonoBehaviour
    {
        public Color color = Color.white;
        public float thickness = 0.004f;
        public GameObject holder;
        public GameObject pointer;

        private GameObject losdot;

        private GameObject hitObject;

        bool isActive = false;
        public bool addRigidBody = false;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;

        Transform previousContact = null;

        float zDistance = 10.0f;

        Transform cacheTransform;

        public void SetHolderLocalPosition(Vector3 localPosition)
        {
            if (holder == null)
            {
                holder = new GameObject("NvrLaserPointer");
                holder.transform.parent = this.transform;
                holder.transform.localPosition = localPosition;
                holder.transform.localRotation = Quaternion.identity;
            }
            else
            {
                holder.transform.localPosition = localPosition;
            }
        }

        // Use this for initialization
        void Start()
        {
            cacheTransform = transform;

            if (holder == null)
            {
                holder = new GameObject("NvrLaserPointer");
                holder.transform.parent = this.transform;
                holder.transform.localPosition = new Vector3(0, -0.005f, 0.08f);
                holder.transform.localRotation = Quaternion.identity;
            }

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.transform.localScale = new Vector3(thickness, thickness, zDistance);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.identity;
            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    Object.Destroy(collider);
                }
            }

            Material newMaterial = Resources.Load<Material>("Materials/UnlitColor");
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;     
            losdot = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NvrLosDot"));
            // 解决射线白点有偏转问题 
            // losdot.transform.parent = holder.transform;
            losdot.gameObject.name = "LosDot_" + (Time.time * 1000);
            losdot.SetActive(false);
        }

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            if (PointerIn != null)
                PointerIn(this, e);
        }

        public virtual void OnPointerOut(PointerEventArgs e)
        {
            if (PointerOut != null)
                PointerOut(this, e);
        }

        void OnDisable()
        {
            if (losdot != null)
            {
                Destroy(losdot);
                losdot = null;
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (!isActive)
            {
                isActive = true;
                transform.GetChild(0).gameObject.SetActive(true);
            }

            if(losdot != null && holder != null)
            {
                losdot.SetActive(holder.activeSelf);
            }

            if(holder == null || pointer == null || !holder.activeSelf)
            {
                return;
            }

            float dist = zDistance;

            Ray raycast = new Ray(cacheTransform.position, cacheTransform.forward);
            RaycastHit hit;
            bool bHit = Physics.Raycast(raycast, out hit);

            if (previousContact && previousContact != hit.transform)
            {
                PointerEventArgs args = new PointerEventArgs();
                args.distance = 0f;
                args.flags = 0;
                args.target = previousContact;
                OnPointerOut(args);
                previousContact = null;
            }

            if (bHit && previousContact != hit.transform)
            {
                PointerEventArgs argsIn = new PointerEventArgs();
                argsIn.distance = hit.distance;
                argsIn.flags = 0;
                argsIn.target = hit.transform;
                OnPointerIn(argsIn);
                previousContact = hit.transform;
                hitObject = hit.collider.gameObject;
            }

            if (!bHit)
            {
                previousContact = null;
                hitObject = null;
                if(losdot != null) losdot.SetActive(false);
            }

            if (bHit && hit.distance < zDistance)
            {
                dist = hit.distance;
                if (losdot != null)
                {
                    losdot.SetActive(true);
                    losdot.transform.position = hit.point - new Vector3(0, -holder.transform.localPosition.y , 0.01f);
                }
            }
            pointer.transform.localScale = new Vector3(thickness, thickness, dist);
            pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        }

        public GameObject GetLosDot()
        {
            return losdot;
        }
    }
}