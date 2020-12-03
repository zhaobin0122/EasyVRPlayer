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
using System;

public class WaveVR_Resource {
	private static string LOG_TAG = "WaveVR_Resource";

	private static WaveVR_Resource mInstance = null;

	public static WaveVR_Resource instance {
		get
		{
			if (mInstance == null)
			{
				mInstance = new WaveVR_Resource();
			}

			return mInstance;
		}
	}

	public string getString(string stringName)
	{
		Log.d(LOG_TAG, "getString, string " + stringName);

		string retString = "";

		if (useSystemLanguageFlag == true)
		{
			retString = Interop.WVR_GetStringBySystemLanguage(stringName);
		} else
		{
			retString = Interop.WVR_GetStringByLanguage(stringName, mPreferredLanguage, mCountry);
		}
		Log.d(LOG_TAG, "getString, ret string = " + retString);
		return retString;
	}

	public string getStringByLanguage(string stringName, string lang, string country)
	{
		Log.d(LOG_TAG, "getPreferredString, string " + stringName + " language is " + lang + " country is " + country);

		string retString = Interop.WVR_GetStringByLanguage(stringName, lang, country);

		Log.d(LOG_TAG, "getStringByLanguage, ret string = " + retString);
		return retString;
	}
	public string getSystemLanguage()
	{
		string retString = Interop.WVR_GetSystemLanguage();

		Log.d(LOG_TAG, "getSystemLanguage, ret language = " + retString);
		return retString;
	}

	public string getSystemCountry()
	{
		string retString = Interop.WVR_GetSystemCountry();

		Log.d(LOG_TAG, "getSystemCountry, ret country = " + retString);
		return retString;
	}

	public bool setPreferredLanguage(string lang, string country)
	{
		if (lang == "" && country == "")
			return false;

		useSystemLanguageFlag = false;
		mPreferredLanguage = lang;
		mCountry = country;
		return true;
	}

	public void useSystemLanguage()
	{
		mPreferredLanguage = "system";
		mCountry = "system";
		useSystemLanguageFlag = true;
	}
	private string mPreferredLanguage = "system";
	private string mCountry = "system";
	private bool useSystemLanguageFlag = true;
}
