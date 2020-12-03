/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_Utils.cs
 *   
*************************************************************/
using UnityEngine;
using System.Collections;
using NoloClientCSharp;

public struct Nolo_Transform
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 vecVelocity;
    public Vector3 vecAngularVelocity;

    public Nolo_Transform(Nolo_Pose pose)
    {
        this.pos.x = pose.pos.x;
        this.pos.y = pose.pos.y;
        this.pos.z = pose.pos.z;

        this.rot.w = pose.rot.w;
        this.rot.x = pose.rot.x;
        this.rot.y = pose.rot.y;
        this.rot.z = pose.rot.z;

        this.vecVelocity.x = pose.vecVelocity.x;
        this.vecVelocity.y = pose.vecVelocity.y;
        this.vecVelocity.z = -pose.vecVelocity.z;

        this.vecAngularVelocity.x = pose.vecAngularVelocity.x;
        this.vecAngularVelocity.y = pose.vecAngularVelocity.y;
        this.vecAngularVelocity.z = pose.vecAngularVelocity.z;

    }
    public Nolo_Transform(Vector3 pos,Quaternion rot,Vector3 vecVelocity,Vector3 vecAngularVelocity)
    {
        this.pos = pos;
        this.rot = rot;
        this.vecVelocity = vecVelocity;
        this.vecAngularVelocity = vecAngularVelocity;
    }
    public Nolo_Transform(NVector3 pos,NQuaternion rot,NVector3 vecVelocity,NVector3 vecAngularVelocity)
    {
        this.pos.x = pos.x;
        this.pos.y = pos.y;
        this.pos.z = pos.z;

        this.rot.w = rot.w;
        this.rot.x = rot.x;
        this.rot.y = rot.y;
        this.rot.z = rot.z;

        this.vecVelocity.x = vecVelocity.x;
        this.vecVelocity.y = vecVelocity.y;
        this.vecVelocity.z = -vecVelocity.z;

        this.vecAngularVelocity.x = vecAngularVelocity.x;
        this.vecAngularVelocity.y = vecAngularVelocity.y;
        this.vecAngularVelocity.z = vecAngularVelocity.z;
    }

    public static Nolo_Transform identity
    {
       get { return new Nolo_Transform(Vector3.zero, Quaternion.identity,Vector3.zero,Vector3.zero); }
    }
}

public class NOLO_Events
{
    public enum EventsType
    {
        TrackingOutofRange,
        TrackingInRange,
        TurnAround,
        RecenterLeft,
        RecenterRight,

        //获取定位设备类型
        GetTrackModel,
    }

    public delegate void Handler(params object[] args);

    public static void Listen(EventsType eventMessage, Handler action)
    {
        var actions = listeners[eventMessage] as Handler;
        if (actions != null)
        {
            listeners[eventMessage] = actions + action;
        }
        else
        {
            listeners[eventMessage] = action;
        }
    }
    public static void Remove(EventsType eventMessage, Handler action)
    {
        var actions = listeners[eventMessage] as Handler;
        if (actions != null)
        {
            listeners[eventMessage] = actions - action;
        }
    }
    public static void Send(EventsType eventMessage, params object[] args)
    {
        var actions = listeners[eventMessage] as Handler;
        if (actions != null)
        {
            actions(args);
        }
    }
    private static Hashtable listeners = new Hashtable();
}



