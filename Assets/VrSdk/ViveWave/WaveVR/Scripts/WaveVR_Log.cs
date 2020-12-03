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
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace WaveVR_Log
{
	[System.Obsolete("namespace WaveVR_Log obsolete, use namespace WVR_Log instead.")]
	public class Log
	{
		private const int ANDROID_LOG_VERBOSE = 2;
		private const int ANDROID_LOG_DEBUG = 3;
		private const int ANDROID_LOG_INFO = 4;
		private const int ANDROID_LOG_WARN = 5;
		private const int ANDROID_LOG_ERROR = 6;

#if UNITY_ANDROID && !UNITY_EDITOR
		[DllImportAttribute("log", EntryPoint = "__android_log_print", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		internal static extern int __android_log_print(int prio, string tag, string fmt, System.IntPtr ptr);
#else
		private static int __android_log_print(int prio, string tag, string fmt, System.IntPtr ptr)
		{
			return 0;
		}
#endif

		public static void d(string tag, string message, bool logInEditor = false)
		{
			__android_log_print(ANDROID_LOG_DEBUG, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.Log(tag + " " + message);
#endif
		}
		public static void i(string tag, string message, bool logInEditor = false)
		{
			__android_log_print(ANDROID_LOG_INFO, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.Log(tag + " " + message);
#endif
		}
		public static void w(string tag, string message, bool logInEditor = false)
		{
			__android_log_print(ANDROID_LOG_WARN, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.LogWarning(tag + " " + message);
#endif
		}
		public static void e(string tag, string message, bool logInEditor = false)
		{
			__android_log_print(ANDROID_LOG_ERROR, tag, message, System.IntPtr.Zero);
#if UNITY_EDITOR
			if (logInEditor)
				Debug.LogError(tag + " " + message);
#endif
		}

		public static EnterAndExit ee(string message, bool logInEditor = false)
		{
			return new EnterAndExit("Unity", message, "+", "-", logInEditor);
		}

		public static EnterAndExit ee(string tag, string message, bool logInEditor = false)
		{
			return new EnterAndExit(tag, message, "+", "-", logInEditor);
		}

		public static EnterAndExit ee(string tag, string postfixEnter, string postfixExit, bool logInEditor = false)
		{
			return new EnterAndExit(tag, "", postfixEnter, postfixExit, logInEditor);
		}

		public static EnterAndExit ee(string tag, string message, string postfixEnter, string postfixExit, bool logInEditor = false)
		{
			return new EnterAndExit(tag, message, postfixEnter, postfixExit, logInEditor);
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
			public float interval = 3;   // default is 3 seconds
			private float lastTime = 0;
			private bool print = true;
			public delegate string StringProcessDelegate();

			public PeriodLog()
			{
				lastTime = Time.realtimeSinceStartup;
			}

			public void check()
			{
				var time = Time.realtimeSinceStartup;
				print = false;
				if (time > (lastTime + interval))
				{
					lastTime = time;
					print = true;
				}
			}

			public void d(string tag, string message, bool logInEditor = false)
			{
				if (print) Log.d(tag, message, logInEditor);
			}

			// If not print, the delegate will not be processed.  Save performance waste of string concat.
			public void d(string tag, StringProcessDelegate strDelegate, bool logInEditor = false)
			{
				if (print) Log.d(tag, strDelegate(), logInEditor);
			}
		}

		public static PeriodLog gpl = new PeriodLog();
	}
}
