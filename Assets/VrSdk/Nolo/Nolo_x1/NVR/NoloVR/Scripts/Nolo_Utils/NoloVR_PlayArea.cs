/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_PlayArea.cs
 *   
*************************************************************/

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class NoloVR_PlayArea : MonoBehaviour {

    public float height = 2.0f;
    public enum PlaySize
    {
        _300x500,
        _250x400,
        _200x350
    }

    public PlaySize size;
    [HideInInspector]
    public Vector3[] vertices;

    void OnEnable()
    {
        GetVectors();
    }

    void GetVectors()
    {
        var str = size.ToString().Substring(1);
        var arr = str.Split(new char[] { 'x' }, 2);

        // convert to half size in meters (from cm)
        var x = float.Parse(arr[0]) / 200;
        var z = float.Parse(arr[1]) / 200;

        var corners = new Vector3[] { new Vector3(x,0,z),
              new Vector3(x,0,-z),
               new Vector3(-x,0,-z),
                  new Vector3(-x,0,z)};
        vertices = new Vector3[corners.Length * 2];
        for (int i = 0; i < corners.Length; i++)
        {
            var c = corners[i];
            vertices[i] = new Vector3(c.x, 0.01f, c.z);
        }
        for (int i = 0; i < corners.Length; i++)
        {
            int next = (i + 1) % corners.Length;
            int prev = (i + corners.Length - 1) % corners.Length;

            var nextSegment = (vertices[next] - vertices[i]).normalized;
            var prevSegment = (vertices[prev] - vertices[i]).normalized;

            var vert = vertices[i];

            vertices[corners.Length + i] = vert;
        }
    }

    // Update is called once per frame
    Hashtable values;
    void Update () {

        if (!Application.isPlaying)
        {
            var fields = GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            bool rebuild = false;

            if (values == null )
            {
                rebuild = true;
            }
            else
            {
                foreach (var f in fields)
                {
                    if (!values.Contains(f) || !f.GetValue(this).Equals(values[f]))
                    {
                        rebuild = true;
                        break;
                    }
                }
            }

            if (rebuild)
            {
                GetVectors();
                values = new Hashtable();
                foreach (var f in fields)
                    values[f] = f.GetValue(this);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (vertices == null || vertices.Length == 0)
            return;

        var offset = transform.TransformVector(Vector3.up * height);
        for (int i = 0; i < 4; i++)
        {
            int next = (i + 1) % 4;
            var a = transform.TransformPoint(vertices[i]);
            var b = a + offset;
            var c = transform.TransformPoint(vertices[next]);
            var d = c + offset;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(a, c);
            Gizmos.DrawLine(b, d);
        }
    }
}
