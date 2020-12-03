/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_Playform.cs
 *   
*************************************************************/

public abstract class NoloVR_Playform
{
    public abstract bool InitDevice();
   
    public abstract void DisconnectDevice();
   
    public abstract void ReconnectDeviceCallBack(); 
    
    public abstract void DisConnectedCallBack();

    public abstract void Authentication(string appKey);

    public abstract void ReportError(string msg);

    public abstract bool IsInstallServer();

    public abstract bool IsStartUpServer();

    protected static NoloError playformError = NoloError.UnKnow;
    protected static bool isAuthentication = false;
    private static NoloVR_Playform instance;
    protected NoloVR_Playform()
    {
        if (playformError == NoloError.UnKnow)
        {
            InitDevice();
        }
    }
    public static NoloVR_Playform GetInstance()
    {
        if (instance == null)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            instance = new NoloVR_WinPlayform();
#elif UNITY_ANDROID
            instance = new NoloVR_AndroidPlayform();
#else
           instance = new NoloVR_OtherPlayform();
#endif
        }
        return instance;
    }
    public NoloError GetPlayformError()
    {
        return playformError;
    }
    public bool GetAuthentication() {
        return isAuthentication;

    }
    ~NoloVR_Playform()
    {
        if (instance != null)
        {
            //DisconnectDevice();
            instance = null;
        }
    }

}