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
using wvr;
using WVR_Log;

public class WaveVR_ButtonList : MonoBehaviour {
	private static string LOG_TAG = "WaveVR_ButtonList";
	private void INFO(string msg) { Log.i (LOG_TAG, msg, true); }
	private void DEBUG(string msg) {
		if (Log.EnableDebugLog)
			Log.d (LOG_TAG, msg, true);
	}

	public enum EButtons
	{
		Unavailable = WVR_InputId.WVR_InputId_Alias1_System,
		Menu = WVR_InputId.WVR_InputId_Alias1_Menu,
		Grip = WVR_InputId.WVR_InputId_Alias1_Grip,
		DPadUp = WVR_InputId.WVR_InputId_Alias1_DPad_Up,
		DPadRight = WVR_InputId.WVR_InputId_Alias1_DPad_Right,
		DPadDown = WVR_InputId.WVR_InputId_Alias1_DPad_Down,
		DPadLeft = WVR_InputId.WVR_InputId_Alias1_DPad_Left,
		VolumeUp = WVR_InputId.WVR_InputId_Alias1_Volume_Up,
		VolumeDown = WVR_InputId.WVR_InputId_Alias1_Volume_Down,
		//DigitalTrigger = WVR_InputId.WVR_InputId_Alias1_Digital_Trigger,
		Back = WVR_InputId.WVR_InputId_Alias1_Back,
		Enter = WVR_InputId.WVR_InputId_Alias1_Enter,
		Touchpad = WVR_InputId.WVR_InputId_Alias1_Touchpad,
		Trigger = WVR_InputId.WVR_InputId_Alias1_Trigger,
		Thumbstick = WVR_InputId.WVR_InputId_Alias1_Thumbstick
	}

	public EButtons GetEButtonsType(WVR_InputId button)
	{
		EButtons btn_type = EButtons.Unavailable;
		switch (button)
		{
		case WVR_InputId.WVR_InputId_Alias1_Menu:
			btn_type = EButtons.Menu;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Grip:
			btn_type = EButtons.Grip;
			break;
		case WVR_InputId.WVR_InputId_Alias1_DPad_Up:
			btn_type = EButtons.DPadUp;
			break;
		case WVR_InputId.WVR_InputId_Alias1_DPad_Right:
			btn_type = EButtons.DPadRight;
			break;
		case WVR_InputId.WVR_InputId_Alias1_DPad_Down:
			btn_type = EButtons.DPadDown;
			break;
		case WVR_InputId.WVR_InputId_Alias1_DPad_Left:
			btn_type = EButtons.DPadLeft;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Volume_Up:
			btn_type = EButtons.VolumeUp;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Volume_Down:
			btn_type = EButtons.VolumeDown;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Back:
			btn_type = EButtons.Back;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Enter:
			btn_type = EButtons.Enter;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Touchpad:
			btn_type = EButtons.Touchpad;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Trigger:
			btn_type = EButtons.Trigger;
			break;
		case WVR_InputId.WVR_InputId_Alias1_Thumbstick:
			btn_type = EButtons.Thumbstick;
			break;
		default:
			btn_type = EButtons.Unavailable;
			break;
		}

		return btn_type;
	}

	public enum EHmdButtons
	{
		Menu = EButtons.Menu,
		DPadUp = EButtons.DPadUp,
		DPadRight = EButtons.DPadRight,
		DPadDown = EButtons.DPadDown,
		DPadLeft = EButtons.DPadLeft,
		VolumeUp = EButtons.VolumeUp,
		VolumeDown = EButtons.VolumeDown,
		Enter = EButtons.Enter,
		Touchpad = EButtons.Touchpad
	}

	private List<EButtons> ToEButtons(List<EHmdButtons> eList)
	{
		List<EButtons> _list = new List<EButtons> ();
		for (int i = 0; i < eList.Count; i++)
		{
			if (!_list.Contains ((EButtons)eList [i]))
				_list.Add ((EButtons)eList [i]);
		}

		return _list;
	}

	public enum EControllerButtons
	{
		Menu = EButtons.Menu,
		Grip = EButtons.Grip,
		DPadUp = EButtons.DPadUp,
		DPadRight = EButtons.DPadRight,
		DPadDown = EButtons.DPadDown,
		DPadLeft = EButtons.DPadLeft,
		VolumeUp = EButtons.VolumeUp,
		VolumeDown = EButtons.VolumeDown,
		Touchpad = EButtons.Touchpad,
		Trigger = EButtons.Trigger,
		Thumbstick = EButtons.Thumbstick
	}

	private List<EButtons> ToEButtons(List<EControllerButtons> eList)
	{
		List<EButtons> _list = new List<EButtons> ();
		for (int i = 0; i < eList.Count; i++)
		{
			if (!_list.Contains ((EButtons)eList [i]))
				_list.Add ((EButtons)eList [i]);
		}

		return _list;
	}

	public List<EHmdButtons> HmdButtons;
	private WVR_InputAttribute_t[] inputAttributes_hmd;
	private List<WVR_InputId> usableButtons_hmd = new List<WVR_InputId> ();
	private bool hmd_connected = false;

	public List<EControllerButtons> DominantButtons;
	private WVR_InputAttribute_t[] inputAttributes_Dominant;
	private List<WVR_InputId> usableButtons_dominant = new List<WVR_InputId> ();
	private bool dominant_connected = false;

	public List<EControllerButtons> NonDominantButtons;
	private WVR_InputAttribute_t[] inputAttributes_NonDominant;
	private List<WVR_InputId> usableButtons_nonDominant = new List<WVR_InputId> ();
	private bool nodomint_connected = false;

	private const uint inputTableSize = (uint)WVR_InputId.WVR_InputId_Max;
	private WVR_InputMappingPair_t[] inputTableHMD = new WVR_InputMappingPair_t[inputTableSize];
	private uint inputTableHMDSize = 0;
	private WVR_InputMappingPair_t[] inputTableDominant = new WVR_InputMappingPair_t[inputTableSize];
	private uint inputTableDominantSize = 0;
	private WVR_InputMappingPair_t[] inputTableNonDominant = new WVR_InputMappingPair_t[inputTableSize];
	private uint inputTableNonDominantSize = 0;

	private static WaveVR_ButtonList instance = null;
	public static WaveVR_ButtonList Instance {
		get
		{
			return instance;
		}
	}

	#region MonoBehaviour overrides
	void Awake()
	{
		if (instance == null)
			instance = this;
	}

	void Start ()
	{
		INFO ("Start()");
		ResetAllInputRequest ();
	}

	void Update ()
	{
		bool _hmd_connected = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).connected;
		bool _dominant_connected = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Dominant).connected;
		bool _nodomint_connected = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.NonDominant).connected;

		/***
		 * Consider a situation:
		 * Only 1 controller connected but trying to SetInputRequest of both controllers.
		 * There is only 1 controller SetInputRequest succeeds because another controller is disconnected.
		 * In order to apply new input attribute, it needs to SetInputRequest when controller is connected.
		 ***/
		if (this.hmd_connected != _hmd_connected)
		{
			this.hmd_connected = _hmd_connected;
			if (this.hmd_connected)
			{
				DEBUG ("Update() HMD is connected.");
				//ResetInputRequest (WaveVR_Controller.EDeviceType.Head);
			}
		}
		if (this.dominant_connected != _dominant_connected)
		{
			this.dominant_connected = _dominant_connected;
			if (this.dominant_connected)
			{
				DEBUG ("Update() Dominant is connected.");
				//ResetInputRequest (WaveVR_Controller.EDeviceType.Dominant);
			}
		}
		if (this.nodomint_connected != _nodomint_connected)
		{
			this.nodomint_connected = _nodomint_connected;
			if (this.nodomint_connected)
			{
				DEBUG ("Update() NonDominant is connected.");
				//ResetInputRequest (WaveVR_Controller.EDeviceType.NonDominant);
			}
		}
	}
	#endregion

	public bool GetInputMappingPair(WaveVR_Controller.EDeviceType device, ref EButtons destination)
	{
		bool _result = false;
		WVR_InputId _wbtn = (WVR_InputId)destination;

		_result = GetInputMappingPair(device, ref _wbtn);
		if (_result)
			destination = GetEButtonsType (_wbtn);

		return _result;
	}

	public bool GetInputMappingPair(WaveVR_Controller.EDeviceType device, ref WVR_InputId destination)
	{
		if (!WaveVR.Instance.Initialized)
			return false;

		// Default true in editor mode, destination will be equivallent to source.
		bool result = false;
		int index = 0;

		switch (device)
		{
		case WaveVR_Controller.EDeviceType.Head:
			if (inputTableHMDSize > 0)
			{
				for (index = 0; index < (int)inputTableHMDSize; index++)
				{
					if (inputTableHMD [index].destination.id == destination)
					{
						destination = inputTableHMD [index].source.id;
						result = true;
					}
				}
			}
			break;
		case WaveVR_Controller.EDeviceType.Dominant:
			if (inputTableDominantSize > 0)
			{
				for (index = 0; index < (int)inputTableDominantSize; index++)
				{
					if (inputTableDominant [index].destination.id == destination)
					{
						destination = inputTableDominant [index].source.id;
						result = true;
					}
				}
			}
			break;
		case WaveVR_Controller.EDeviceType.NonDominant:
			if (inputTableNonDominantSize > 0)
			{
				for (index = 0; index < (int)inputTableNonDominantSize; index++)
				{
					if (inputTableNonDominant [index].destination.id == destination)
					{
						destination = inputTableNonDominant [index].source.id;
						result = true;
					}
				}
			}
			break;
		default:
			break;
		}

		return result;
	}

	private void setupButtonAttributes(WaveVR_Controller.EDeviceType device, List<EButtons> buttons, WVR_InputAttribute_t[] inputAttributes, int count)
	{
		WVR_DeviceType _type = WaveVR_Controller.Input (device).DeviceType;

		for (int _i = 0; _i < count; _i++)
		{
			switch (buttons [_i])
			{
			case EButtons.Menu:
			case EButtons.Grip:
			case EButtons.DPadLeft:
			case EButtons.DPadUp:
			case EButtons.DPadRight:
			case EButtons.DPadDown:
			case EButtons.VolumeUp:
			case EButtons.VolumeDown:
			case EButtons.Back:
			case EButtons.Enter:
				inputAttributes [_i].id = (WVR_InputId)buttons [_i];
				inputAttributes [_i].capability = (uint)WVR_InputType.WVR_InputType_Button;
				inputAttributes [_i].axis_type = WVR_AnalogType.WVR_AnalogType_None;
				break;
			case EButtons.Touchpad:
			case EButtons.Thumbstick:
				inputAttributes [_i].id = (WVR_InputId)buttons [_i];
				inputAttributes [_i].capability = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);
				inputAttributes [_i].axis_type = WVR_AnalogType.WVR_AnalogType_2D;
				break;
			case EButtons.Trigger:
				inputAttributes [_i].id = (WVR_InputId)buttons [_i];
				inputAttributes [_i].capability = (uint)(WVR_InputType.WVR_InputType_Button | WVR_InputType.WVR_InputType_Touch | WVR_InputType.WVR_InputType_Analog);
				inputAttributes [_i].axis_type = WVR_AnalogType.WVR_AnalogType_1D;
				break;
			default:
				break;
			}

			DEBUG ("setupButtonAttributes() " + device + " (" + _type + ") " + buttons [_i]
				+ ", capability: " + inputAttributes [_i].capability
				+ ", analog type: " + inputAttributes [_i].axis_type);
		}
	}

	private void createHmdRequestAttributes()
	{
		INFO ("createHmdRequestAttributes()");

		List<EButtons> _list = ToEButtons (this.HmdButtons);
		if (!_list.Contains (EButtons.Enter))
			_list.Add (EButtons.Enter);

		int _count = _list.Count;
		inputAttributes_hmd = new WVR_InputAttribute_t[_count];
		setupButtonAttributes (WaveVR_Controller.EDeviceType.Head, _list, inputAttributes_hmd, _count);
	}

	private void createDominantRequestAttributes()
	{
		INFO ("createDominantRequestAttributes()");

		List<EButtons> _list = ToEButtons (this.DominantButtons);

		int _count = _list.Count;
		inputAttributes_Dominant = new WVR_InputAttribute_t[_count];
		setupButtonAttributes (WaveVR_Controller.EDeviceType.Dominant, _list, inputAttributes_Dominant, _count);
	}

	private void createNonDominantRequestAttributes()
	{
		INFO ("createNonDominantRequestAttributes()");

		List<EButtons> _list = ToEButtons (this.NonDominantButtons);

		int _count = _list.Count;
		inputAttributes_NonDominant = new WVR_InputAttribute_t[_count];
		setupButtonAttributes (WaveVR_Controller.EDeviceType.NonDominant, _list, inputAttributes_NonDominant, _count);
	}

	public bool IsButtonAvailable(WaveVR_Controller.EDeviceType device, EButtons button)
	{
		return IsButtonAvailable (device, (WVR_InputId)button);
	}

	public bool IsButtonAvailable(WaveVR_Controller.EDeviceType device, WVR_InputId button)
	{
		if (device == WaveVR_Controller.EDeviceType.Head)
			return this.usableButtons_hmd.Contains (button);
		if (device == WaveVR_Controller.EDeviceType.Dominant)
			return this.usableButtons_dominant.Contains (button);
		if (device == WaveVR_Controller.EDeviceType.NonDominant)
			return this.usableButtons_nonDominant.Contains (button);

		return false;
	}

	private void SetHmdInputRequest()
	{
		this.usableButtons_hmd.Clear ();
		if (!WaveVR.Instance.Initialized)
			return;

		WVR_DeviceType _type = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Head).DeviceType;
		bool _ret = Interop.WVR_SetInputRequest (_type, this.inputAttributes_hmd, (uint)this.inputAttributes_hmd.Length);
		if (_ret)
		{
			inputTableHMDSize = Interop.WVR_GetInputMappingTable (_type, inputTableHMD, WaveVR_ButtonList.inputTableSize);
			if (inputTableHMDSize > 0)
			{
				for (int _i = 0; _i < (int)inputTableHMDSize; _i++)
				{
					if (inputTableHMD [_i].source.capability != 0)
					{
						this.usableButtons_hmd.Add (inputTableHMD [_i].destination.id);
						DEBUG ("SetHmdInputRequest() " + _type
							+ " button: " + inputTableHMD [_i].source.id + "(capability: " + inputTableHMD [_i].source.capability + ")"
							+ " is mapping to HMD input ID: " + inputTableHMD [_i].destination.id);
					} else
					{
						DEBUG ("SetHmdInputRequest() " + _type
							+ " source button " + inputTableHMD [_i].source.id + " has invalid capability.");
					}
				}
			}
		}
	}

	private void SetDominantInputRequest()
	{
		this.usableButtons_dominant.Clear ();
		if (!WaveVR.Instance.Initialized)
			return;

		WVR_DeviceType _type = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.Dominant).DeviceType;
		bool _ret = Interop.WVR_SetInputRequest (_type, this.inputAttributes_Dominant, (uint)this.inputAttributes_Dominant.Length);
		if (_ret)
		{
			inputTableDominantSize = Interop.WVR_GetInputMappingTable (_type, inputTableDominant, WaveVR_ButtonList.inputTableSize);
			if (inputTableDominantSize > 0)
			{
				for (int _i = 0; _i < (int)inputTableDominantSize; _i++)
				{
					if (inputTableDominant [_i].source.capability != 0)
					{
						this.usableButtons_dominant.Add (inputTableDominant [_i].destination.id);
						DEBUG ("SetDominantInputRequest() " + _type
							+ " button: " + inputTableDominant [_i].source.id + "(capability: " + inputTableDominant [_i].source.capability + ")"
							+ " is mapping to Dominant input ID: " + inputTableDominant [_i].destination.id);
					} else
					{
						DEBUG ("SetDominantInputRequest() " + _type
							+ " source button " + inputTableDominant [_i].source.id + " has invalid capability.");
					}
				}
			}
		}
	}

	private void SetNonDominantInputRequest()
	{
		this.usableButtons_nonDominant.Clear ();
		if (!WaveVR.Instance.Initialized)
			return;

		WVR_DeviceType _type = WaveVR_Controller.Input (WaveVR_Controller.EDeviceType.NonDominant).DeviceType;
		bool _ret = Interop.WVR_SetInputRequest (_type, this.inputAttributes_NonDominant, (uint)this.inputAttributes_NonDominant.Length);
		if (_ret)
		{
			inputTableNonDominantSize = Interop.WVR_GetInputMappingTable (_type, inputTableNonDominant, WaveVR_ButtonList.inputTableSize);
			if (inputTableNonDominantSize > 0)
			{
				for (int _i = 0; _i < (int)inputTableNonDominantSize; _i++)
				{
					if (inputTableNonDominant [_i].source.capability != 0)
					{
						this.usableButtons_nonDominant.Add (inputTableNonDominant [_i].destination.id);
						DEBUG ("SetNonDominantInputRequest() " + _type
							+ " button: " + inputTableNonDominant [_i].source.id + "(capability: " + inputTableNonDominant [_i].source.capability + ")"
							+ " is mapping to NonDominant input ID: " + inputTableNonDominant [_i].destination.id);
					} else
					{
						DEBUG ("SetNonDominantInputRequest() " + _type
							+ " source button " + inputTableNonDominant [_i].source.id + " has invalid capability.");
					}
				}
			}
		}
	}

	private void ResetInputRequest(WaveVR_Controller.EDeviceType device)
	{
		DEBUG ("ResetInputRequest() " + device);
		switch (device)
		{
		case WaveVR_Controller.EDeviceType.Head:
			createHmdRequestAttributes ();
			SetHmdInputRequest ();
			break;
		case WaveVR_Controller.EDeviceType.Dominant:
			createDominantRequestAttributes ();
			SetDominantInputRequest ();
			break;
		case WaveVR_Controller.EDeviceType.NonDominant:
			createNonDominantRequestAttributes ();
			SetNonDominantInputRequest ();
			break;
		default:
			break;
		}
	}

	public void SetupHmdButtonList(List<EHmdButtons> list)
	{
		DEBUG ("SetupHmdButtonList()");

		this.HmdButtons = list;
		ResetInputRequest (WaveVR_Controller.EDeviceType.Head);
	}

	public void SetupControllerButtonList(WaveVR_Controller.EDeviceType device, List<WaveVR_ButtonList.EControllerButtons> list)
	{
		DEBUG ("SetupControllerButtonList() " + device);
		switch (device)
		{
		case WaveVR_Controller.EDeviceType.Dominant:
			this.DominantButtons = list;
			ResetInputRequest (WaveVR_Controller.EDeviceType.Dominant);
			break;
		case WaveVR_Controller.EDeviceType.NonDominant:
			this.NonDominantButtons = list;
			ResetInputRequest (WaveVR_Controller.EDeviceType.NonDominant);
			break;
		default:
			break;
		}
	}

	public void ResetAllInputRequest()
	{
		DEBUG ("ResetAllInputRequest()");
		ResetInputRequest (WaveVR_Controller.EDeviceType.Head);
		ResetInputRequest (WaveVR_Controller.EDeviceType.Dominant);
		ResetInputRequest (WaveVR_Controller.EDeviceType.NonDominant);
	}
}
