// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using AOT;

namespace wvr.render.utils
{
	public class Message
	{
		public bool isFree = true;
	}

	public class MessagePool
	{
		private readonly List<Message> pool = new List<Message>(2) {};
		private int index = 0;

		public MessagePool() {}

		private int Next(int value)
		{
			if (++value >= pool.Count)
				value = 0;
			return value;
		}

		public T Obtain<T>() where T : Message, new()
		{
			int c = pool.Count;
			int i = index;
			for (int j = 0; j < c; i++, j++)
			{
				if (i >= c)
					i = 0;
				if (pool[i].isFree)
				{
					//Debug.LogError("Obtain idx=" + i);
					index = i;
					return (T)pool[i];
				}
			}
			index = Next(i);
			var newItem = new T()
			{
				isFree = true
			};
			pool.Insert(index, newItem);
			//Debug.LogError("Obtain new one.  Pool.Count=" + pool.Count);
			return newItem;
		}

		public void Lock(Message msg)
		{
			msg.isFree = false;
		}

		public void Release(Message msg)
		{
			msg.isFree = true;
		}
	}

	public class PreAllocatedQueue : MessagePool
	{
		private readonly List<Message> list = new List<Message>(2) { null, null };
		private int queueBegin = 0;
		private int queueEnd = 0;

		public PreAllocatedQueue() : base() {}

		private int Next(int value)
		{
			if (++value >= list.Count)
				value = 0;
			return value;
		}

		public void Enqueue(Message msg)
		{
			Lock(msg);
			queueEnd = Next(queueEnd);

			if (queueEnd == queueBegin)
			{
				list.Insert(queueEnd, msg);
				queueBegin++;
			}
			else
			{
				list[queueEnd] = msg;
			}
		}

		public Message Dequeue()
		{
			queueBegin = Next(queueBegin);
			return list[queueBegin];
		}
	}
}

namespace wvr.render.thread
{
	using wvr.render.utils;

	// Run a lambda/delegate code in RenderThread
	public class RenderThreadSyncObject
	{
		// In Windows, Marshal.GetFunctionPointerForDelegate() will cause application hang
		private static IntPtr GetFunctionPointerForDelegate(Delegate del)
		{
#if UNITY_EDITOR && UNITY_ANDROID
			return IntPtr.Zero;
#elif UNITY_ANDROID
			return Marshal.GetFunctionPointerForDelegate(del);
#else
			return IntPtr.Zero;
#endif
		}

		public delegate void RenderEventDelegate(int e);
		private static readonly RenderEventDelegate handle = new RenderEventDelegate(RunSyncObjectInRenderThread);
		private static readonly IntPtr handlePtr = GetFunctionPointerForDelegate(handle);

		public delegate void Receiver(PreAllocatedQueue dataQueue);

		private static List<RenderThreadSyncObject> CommandList = new List<RenderThreadSyncObject>();

		private readonly PreAllocatedQueue queue = new PreAllocatedQueue();
		public PreAllocatedQueue Queue { get { return queue; } }

		private readonly Receiver receiver;
		private readonly int id;

		public RenderThreadSyncObject(Receiver render)
		{
			receiver = render;
			if (receiver == null)
				throw new ArgumentNullException("receiver should not be null");

			CommandList.Add(this);
			id = CommandList.IndexOf(this);
		}

		~RenderThreadSyncObject()
		{
			try { CommandList.RemoveAt(id); } finally { }
		}

		void IssuePluginEvent(IntPtr callback, int eventID)
		{
#if UNITY_EDITOR && UNITY_ANDROID
			if (Application.isEditor)
			{
				receiver(queue);
				return;
			}
#endif

#if UNITY_ANDROID
			GL.IssuePluginEvent(callback, eventID);
			return;
#else
			receiver(queue);
			return;
#endif
		}

		void IssuePluginEvent(CommandBuffer cmdBuf, IntPtr callback, int eventID)
		{
#if UNITY_EDITOR && UNITY_ANDROID
			if (Application.isEditor)
				throw new NotImplementedException("Should not use this function in Windows");
#endif

#if UNITY_ANDROID
			cmdBuf.IssuePluginEvent(callback, eventID);
			return;
#else
			throw new NotImplementedException("Should not use this function in Windows");
#endif
		}

		// Run in GameThread
		public void IssueEvent()
		{
			// Let the render thread run the RunSyncObjectInRenderThread(id)
			IssuePluginEvent(handlePtr, id);
		}

		public void IssueInCommandBuffer(CommandBuffer cmdBuf)
		{
			// Let the render thread run the RunSyncObjectInRenderThread(id)
			IssuePluginEvent(cmdBuf, handlePtr, id);
		}

		// Called by RunSyncObjectInRenderThread()
		private void Receive()
		{
			receiver(queue);
		}

		[MonoPInvokeCallback(typeof(RenderEventDelegate))]
		private static void RunSyncObjectInRenderThread(int id)
		{
			CommandList[id].Receive();
		}

#if false
		private static void InvokeInRenderThread(RenderEventDelegate del, int data)
		{
			// Can we do this?  No, unless can keep the del non-GC-ed before execution.
			IntPtr ptr = Marshal.GetFunctionPointerForDelegate(del);
			GL.IssuePluginEvent(ptr, data);
		}

		private void example1()
		{
			InvokeInRenderThread(data => { WaveVR_Render.Instance.origin = (WVR_PoseOriginModel)data; }, (int)WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead_3DoF);
		}
#endif
	}

	public class WaveVR_RenderThreadExample
	{
		public class TestMessage : Message
		{
			public int textureId;
		}

		static void Example1()
		{
			RenderThreadSyncObject setTexture = new RenderThreadSyncObject(
				// receiver
				(queue) => {
					lock (queue)
					{
						// Run in RenderThread
						var msg = (TestMessage) queue.Dequeue();
						//int textureId = msg.textureId;
						queue.Release(msg);
					}
				});

			// Assume this loop is in the render loop.
			do
			{
				int textureId = 1;
				var queue = setTexture.Queue;
				lock (queue)
				{
					var msg = queue.Obtain<TestMessage>();
					msg.textureId = textureId;
					queue.Enqueue(msg);
				}
				setTexture.IssueEvent();
			} while (false);
		}
	}
}  // namespace wvr.render.thread
