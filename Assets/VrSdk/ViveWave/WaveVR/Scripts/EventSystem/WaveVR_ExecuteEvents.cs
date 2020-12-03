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
using UnityEngine.EventSystems;

public static class WaveVR_ExecuteEvents
{
	#region Event Executor of Hover
	/// Use ExecuteEvents.Execute (GameObject, BaseEventData, WaveVR_ExecuteEvents.pointerHoverHandler)
	private static void HoverExecutor(IPointerHoverHandler handler, BaseEventData eventData)
	{
		handler.OnPointerHover (ExecuteEvents.ValidateEventData<PointerEventData> (eventData));
	}

	public static ExecuteEvents.EventFunction<IPointerHoverHandler> pointerHoverHandler
	{
		get{ return HoverExecutor; }
	}
	#endregion
}
