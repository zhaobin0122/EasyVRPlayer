// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System;
using System.Text;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Runtime.InteropServices;
#elif UNITY_STANDALONE
using System.Runtime.InteropServices;
using System.IO;
#endif
using UnityEngine;

namespace WVR_Log
{
	public class Log
	{
		public static bool EnableDebugLog = true;
		private const int LOG_VERBOSE = 2;
		private const int LOG_DEBUG = 3;
		private const int LOG_INFO = 4;
		private const int LOG_WARN = 5;
		private const int LOG_ERROR = 6;

		// A default StringBuilder
		// Please don't use Insert().  Insert() will let StringBuilder create new buffer when Clear().
		// Please use SB only in game thread.  It's not thread safe.
		private readonly static int SBLength = 511;
		public readonly static StringBuilder SB = new StringBuilder(SBLength, SBLength);
		public static StringBuilder CSB
		{
			get
			{
#if NET_2_0 || NET_2_0_SUBSET
					SB.Length = 0;
					return SB;
#else
				return SB.Clear();
#endif
			}
		}

#if UNITY_ANDROID && !UNITY_EDITOR
		[DllImportAttribute("log", EntryPoint = "__android_log_print", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		internal static extern int __log_print(int prio, string tag, string fmt, System.IntPtr ptr);

#elif UNITY_STANDALONE
		[DllImportAttribute("wave_api", EntryPoint = "WVR_LOGV", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void WVR_LOGV(string log);
		[DllImportAttribute("wave_api", EntryPoint = "WVR_LOGD", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void WVR_LOGD(string log);
		[DllImportAttribute("wave_api", EntryPoint = "WVR_LOGI", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void WVR_LOGI(string log);
		[DllImportAttribute("wave_api", EntryPoint = "WVR_LOGW", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void WVR_LOGW(string log);
		[DllImportAttribute("wave_api", EntryPoint = "WVR_LOGE", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void WVR_LOGE(string log);

		private static int __log_print(int prio, string tag, string fmt, System.IntPtr ptr)
		{
			if (prio == LOG_VERBOSE)
				WVR_LOGV(tag + " " + fmt);
			else if (prio == LOG_DEBUG)
				WVR_LOGD(tag + " " + fmt);
			else if (prio == LOG_INFO)
				WVR_LOGI(tag + " " + fmt);
			else if (prio == LOG_WARN)
				WVR_LOGW(tag + " " + fmt);
			else
				WVR_LOGE(tag + " " + fmt);
			return 0;
		}
#else
		private static int __log_print(int prio, string tag, string fmt, System.IntPtr ptr)
		{
			return 0;
		}
#endif

		public static void v(string tag, string message, bool logInEditor = false)
		{
			__log_print(LOG_VERBOSE, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.Log(tag + " " + message);
#endif
		}

		public static void d(string tag, string message, bool logInEditor = false)
		{
			__log_print(LOG_DEBUG, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.Log(tag + " " + message);
#endif
		}
		public static void i(string tag, string message, bool logInEditor = false)
		{
			__log_print(LOG_INFO, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.Log(tag + " " + message);
#endif
		}
		public static void w(string tag, string message, bool logInEditor = false)
		{
			__log_print(LOG_WARN, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.LogWarning(tag + " " + message);
#endif
		}
		public static void e(string tag, string message, bool logInEditor = false)
		{
			__log_print(LOG_ERROR, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.LogError(tag + " " + message);
#endif
		}

		public static EnterAndExit ee(string message)
		{
			return new EnterAndExit("Unity", message, "+", "-");
		}

		public static EnterAndExit ee(string tag, string message)
		{
			return new EnterAndExit(tag, message, "+", "-");
		}

		public static EnterAndExit ee(string tag, string postfixEnter, string postfixExit)
		{
			return new EnterAndExit(tag, "", postfixEnter, postfixExit);
		}

		public static EnterAndExit ee(string tag, string message, string postfixEnter, string postfixExit)
		{
			return new EnterAndExit(tag, message, postfixEnter, postfixExit);
		}

		/**
		 * The *using* syntax will help calling the dispose of its argument.
		 * 
		 * Usage example:
		 * void func() {
		 *   using(var ee = Log.ee("WVR", "func is ", "enter", "exit")
		 *   {
		 *	  // Do your work here
		 *   }
		 * }
		 * 
		 * Log:
		 *	WVR D func is enter
		 *	... other logs
		 *	WVR D func is exit
		**/
		public class EnterAndExit : IDisposable
		{
			string tag, message, enter, exit;
			bool logInEditor = false;
			public EnterAndExit(string tag, string message, string postfixEnter, string postfixExit, bool logInEditor = false)
			{
				this.tag = tag;
				this.message = message;
				this.exit = postfixExit;
				this.logInEditor = logInEditor;
				Log.d(tag, message + postfixEnter, logInEditor);
			}

			public void Dispose()
			{
				Log.d(tag, message + exit, logInEditor);
			}
		}

		public class PeriodLog
		{
			public delegate string StringProcessDelegate();

			public float interval = 3;   // default is 3 seconds
			private float lastTime = 0;
			public bool Print { get; private set; }

			public PeriodLog()
			{
				lastTime = Time.realtimeSinceStartup;
			}

			public void check()
			{
				var time = Time.realtimeSinceStartup;
				Print = false;
				if (time > (lastTime + interval))
				{
					lastTime = time;
					Print = true;
				}
			}

			public void d(string tag, string message, bool logInEditor = false)
			{
				if (Print) Log.d(tag, message);
			}

			// If not print, the delegate will not be processed.  Save performance waste of string concat.
			[Obsolete("The delegate still use GC.Alloc to remember your variable.")]
			public void d(string tag, StringProcessDelegate strDelegate, bool logInEditor = false)
			{
				if (Print) Log.d(tag, strDelegate(), logInEditor);
			}
		}

		public static PeriodLog gpl = new PeriodLog();
	}
}
