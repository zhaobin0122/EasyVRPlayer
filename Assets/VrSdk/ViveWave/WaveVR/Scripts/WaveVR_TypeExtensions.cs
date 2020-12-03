// "WaveVR SDK 
// © 2017 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the WaveVR SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using wvr;

namespace wvr.TypeExtensions
{
	// Reserved for class
	public static class ClassExtensions
	{

	}

	// You can put enum extensions here.
	public static class EnumExtensions
	{
#if false
		// Template
		public static string Name(this WVR_DeviceType e)
		{
			switch (e)
			{
				default: return "";
			}
		}
#endif
		public static string Name(this WVR_DeviceType e)
		{
			switch (e)
			{
				case WVR_DeviceType.WVR_DeviceType_Controller_Left: return "controller left";
				case WVR_DeviceType.WVR_DeviceType_Controller_Right: return "controller right";
				case WVR_DeviceType.WVR_DeviceType_HMD: return "HMD";
				default: return "Invalidate";
			}
		}

		public static string Name(this WVR_InputId e)
		{
			switch (e)
			{
				case WVR_InputId.WVR_InputId_Alias1_System: return "Syste";
				case WVR_InputId.WVR_InputId_Alias1_Menu: return "Menu";
				case WVR_InputId.WVR_InputId_Alias1_Grip: return "Grip";
				case WVR_InputId.WVR_InputId_Alias1_DPad_Left: return "DPad_Left";
				case WVR_InputId.WVR_InputId_Alias1_DPad_Up: return "DPad_Up";
				case WVR_InputId.WVR_InputId_Alias1_DPad_Right: return "DPad_Right";
				case WVR_InputId.WVR_InputId_Alias1_DPad_Down: return "DPad_Down";
				case WVR_InputId.WVR_InputId_Alias1_Volume_Up: return "Volume_Up";
				case WVR_InputId.WVR_InputId_Alias1_Volume_Down: return "Volume_Down";
				case WVR_InputId.WVR_InputId_Alias1_Digital_Trigger: return "Digital_Trigger";
				case WVR_InputId.WVR_InputId_Alias1_Back: return "Back";
				case WVR_InputId.WVR_InputId_Alias1_Enter: return "Enter";
				case WVR_InputId.WVR_InputId_Alias1_Touchpad: return "Touchpad";
				case WVR_InputId.WVR_InputId_Alias1_Trigger: return "Trigger";
				case WVR_InputId.WVR_InputId_Alias1_Thumbstick: return "Thumbstick";
				default: return e.ToString();
			}
		}
	}
}
