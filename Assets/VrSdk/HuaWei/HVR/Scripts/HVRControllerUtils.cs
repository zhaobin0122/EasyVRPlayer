using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HVRCORE {
	enum KeyStatus {
		KeyDown,
		KeyUp,
		KeyPressed,
		KeyRelease
	}
	public class HVRControllerUtils {

		private const float mLongPressDeltaTime = 1.0f;

		private IController mController = null;

		private float mBackKeyDownTime = 0.0f;
		private float mConfirmKeyDownTime = 0.0f;
		private float mTouchPadDownTime = 0.0f;
		private float mTriggerKeyDownTime = 0.0f;

		private KeyStatus mBackKeyStatus = KeyStatus.KeyRelease;
		private KeyStatus mConfirmKeyStatus = KeyStatus.KeyRelease;
		private KeyStatus mTouchPadStatus = KeyStatus.KeyRelease;
		private KeyStatus mTriggerKeyStatus = KeyStatus.KeyRelease;

		public HVRControllerUtils(IController controller) {
			mController = controller;
		}

		/// <summary>
		///  is the button short pressed.
		///  ButtonType only support  ButtonBack, ButtonConfirm, ButtonTouchPad, ButtonTrigger.
		///  Must be called after HVRControllerUtils Update().
		/// </summary>
		public bool IsShortPressed(ButtonType type) {
			return IsButtonPressed (type , true);
		}

		/// <summary>
		///  is the button long pressed.
		///  ButtonType only support  ButtonBack, ButtonConfirm, ButtonTouchPad, ButtonTrigger.
		///  Must be called after HVRControllerUtils Update().
		/// </summary>
		public bool IsLongPressed(ButtonType type) {
			return IsButtonPressed (type , false);
		}
			
		/// <summary>
		///  Update must be called once per frame
		/// </summary>
		public void Update() {
			if (null == mController) {
				return;
			}

			if (!mController.IsAvailable()) {
				return;
			}

			UpdateButtonStatus (ButtonType.ButtonBack, ref mBackKeyDownTime, ref mBackKeyStatus);
			UpdateButtonStatus (ButtonType.ButtonConfirm, ref mConfirmKeyDownTime, ref mConfirmKeyStatus);
			UpdateButtonStatus (ButtonType.ButtonTouchPad, ref mTouchPadDownTime, ref mTouchPadStatus);
			UpdateButtonStatus (ButtonType.ButtonTrigger, ref mTriggerKeyDownTime, ref mTriggerKeyStatus);
		}

		private void UpdateButtonStatus(ButtonType type, ref float keyDownTime, ref KeyStatus status) {
			if (null == mController) {
				return;
			}

			if (mController.IsButtonDown (type)) {
				keyDownTime = Time.realtimeSinceStartup;
				status = KeyStatus.KeyDown;
			} else if (mController.IsButtonPressed (type)) {
				status = KeyStatus.KeyPressed;
			} else if (mController.IsButtonUp (type)) {
				status = KeyStatus.KeyUp;
			} else {
				status = KeyStatus.KeyRelease;
			}
		}

		private bool IsShortOrLongPressed(KeyStatus status, float keyDownTime,bool isShort) {
			if (KeyStatus.KeyUp == status) {
				if (isShort) {
					if (Time.realtimeSinceStartup - keyDownTime < mLongPressDeltaTime) {
						return true;
					}
				} else {
					if (Time.realtimeSinceStartup - keyDownTime >= mLongPressDeltaTime) {
						return true;
					}
				}
			}
			return false;
		}

		private bool IsButtonPressed(ButtonType type, bool isShort) {
			if (ButtonType.ButtonBack == type) {
				return IsShortOrLongPressed (mBackKeyStatus, mBackKeyDownTime ,isShort);
			} else if (ButtonType.ButtonConfirm == type) {
				return IsShortOrLongPressed (mConfirmKeyStatus, mConfirmKeyDownTime ,isShort);
			} else if (ButtonType.ButtonTouchPad == type) {
				return IsShortOrLongPressed (mTouchPadStatus, mTouchPadDownTime ,isShort);
			} else if (ButtonType.ButtonTrigger == type) {
				return IsShortOrLongPressed (mTriggerKeyStatus, mTriggerKeyDownTime ,isShort);
			} else {
				Debug.LogError ("ButtonType not support " + type);
				return false;
			}
		}
	}
}