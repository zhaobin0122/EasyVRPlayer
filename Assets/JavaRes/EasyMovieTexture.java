package com.EasyMovieTexture;

import android.annotation.TargetApi;
import android.app.Activity;
import android.content.res.AssetManager;
import android.graphics.SurfaceTexture;
import android.graphics.SurfaceTexture.OnFrameAvailableListener;
import android.media.MediaPlayer;
import android.opengl.GLES20;
import android.os.Handler;
import android.os.Message;
import android.util.Log;
import android.view.Surface;

import com.meelive.meelivevideo.VideoEngine;
import com.meelive.meelivevideo.VideoEvent;
import com.meelive.meelivevideo.VideoPlayer;
import com.meelive.meelivevideo.device_adapt.AdaptConfigMgr;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayList;

import tv.danmaku.ijk.media.player.IMediaPlayer;

/**
 * 将此类放在com.EasyMovieTexture包下
 * 必须放在此路径下
 */
public class EasyMovieTexture implements OnFrameAvailableListener {

    public class PlayType {
        //直播
        public static final int live = 0;
        //点播
        public static final int vod = 1;
        //本地播放
        public static final int local = 2;
    }

    public class PlayModelType {
        public static final int _180_3D = 0;
        public static final int _360 = 1;
        public static final int _Plane = 2;
        public static final int _360_3D = 3;
    }

    public enum MEDIAPLAYER_STATE {
        NOT_READY(0),
        READY(1),
        END(2),
        PLAYING(3),
        PAUSED(4),
        STOPPED(5),
        ERROR(6);

        private int iValue;

        MEDIAPLAYER_STATE(int i) {
            iValue = i;
        }

        public int GetValue() {
            return iValue;
        }
    }

    private final static String TAG = "EasyMovieTexture";
    private Activity m_UnityActivity = null;
    private VideoPlayer videoPlayer = null;
    private int m_iUnityTextureID = -1;
    private int m_iSurfaceTextureID = -1;
    private SurfaceTexture m_SurfaceTexture = null;
    private Surface m_Surface = null;
    private int m_iCurrentSeekPercent = 0;
    private int m_iCurrentSeekPosition = 0;
    public int m_iNativeMgrID;
    private String m_strFileName;
    private int m_iErrorCode;
    private int m_iErrorCodeExtra;
    private boolean m_bRockchip = true;
    private boolean m_bSplitOBB = false;
    private String m_strOBBName;
    public boolean m_bUpdate = false;
    private int mPlayType;
    public static int mPlayModelType;

    public static ArrayList<EasyMovieTexture> m_objCtrl = new ArrayList<EasyMovieTexture>();

    public static EasyMovieTexture GetObject(int iID) {
        for (int i = 0; i < m_objCtrl.size(); i++) {
            if (m_objCtrl.get(i).m_iNativeMgrID == iID) {
                return m_objCtrl.get(i);
            }
        }
        return null;

    }


    private static final int GL_TEXTURE_EXTERNAL_OES = 0x8D65;


    public native int InitNDK(Object obj);

    public native void SetAssetManager(AssetManager assetManager);

    public native int InitApplication();

    public native void QuitApplication();

    public native void SetWindowSize(int iWidth, int iHeight, int iUnityTextureID, boolean bRockchip);

    public native void RenderScene(float[] fValue, int iTextureID, int iUnityTextureID);

    public native void SetManagerID(int iID);

    public native int GetManagerID();

    public native int InitExtTexture();

    public native void SetUnityTextureID(int iTextureID);


    static {
        System.loadLibrary("BlueDoveMediaRender");
        VideoEngine.loadLibraries();
    }

    MEDIAPLAYER_STATE m_iCurrentState = MEDIAPLAYER_STATE.NOT_READY;

    public void Destroy() {
        Log.i("zb123", "Destroy");
        handler1.removeCallbacksAndMessages(null);
        if (m_iSurfaceTextureID != -1) {
            int[] textures = new int[1];
            textures[0] = m_iSurfaceTextureID;
            GLES20.glDeleteTextures(1, textures, 0);
            m_iSurfaceTextureID = -1;
        }

        SetManagerID(m_iNativeMgrID);
        QuitApplication();

        m_objCtrl.remove(this);

        UnLoad();
    }

    public void UnLoad() {
        if (videoPlayer != null) {
            if (m_iCurrentState != MEDIAPLAYER_STATE.NOT_READY) {
                try {
                    videoPlayer.stop();
                    videoPlayer.release();
                } catch (SecurityException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                } catch (IllegalStateException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                }
                videoPlayers.remove(videoPlayer);
                videoPlayer = null;

            } else {
                try {
                    videoPlayer.release();


                } catch (SecurityException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                } catch (IllegalStateException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                }
                videoPlayers.remove(videoPlayer);
                videoPlayer = null;
            }

            if (m_Surface != null) {
                m_Surface.release();
                m_Surface = null;
            }

            if (m_SurfaceTexture != null) {
                m_SurfaceTexture.release();
                m_SurfaceTexture = null;
            }

            if (m_iSurfaceTextureID != -1) {
                int[] textures = new int[1];
                textures[0] = m_iSurfaceTextureID;
                GLES20.glDeleteTextures(1, textures, 0);
                m_iSurfaceTextureID = -1;
            }
        }

        m_iCurrentState = MEDIAPLAYER_STATE.NOT_READY;

    }

    public boolean Load() throws SecurityException, IllegalStateException, IOException, InterruptedException {
        UnLoad();

        m_iCurrentState = MEDIAPLAYER_STATE.NOT_READY;

        //todo  videoPlayer.setAudioStreamType(AudioManager.STREAM_MUSIC);

        m_bUpdate = false;

        Log.i("zb123", "Load m_strFileName = " + m_strFileName);

        if (m_strFileName.contains("://") == true) {
            try {
//                videoPlayer.setStreamUrl(m_strFileName, false);

            } catch (Exception e) {
                // TODO Auto-generated catch block
                Log.e("Unity", "Error videoPlayer.setDataSource() : " + m_strFileName);
                e.printStackTrace();

                m_iCurrentState = MEDIAPLAYER_STATE.ERROR;

                return false;
            }
        }

        if (m_iSurfaceTextureID == -1) {
            m_iSurfaceTextureID = InitExtTexture();
        }
        Log.i("zb123", "m_iSurfaceTextureID = " + m_iSurfaceTextureID);
        m_SurfaceTexture = new SurfaceTexture(m_iSurfaceTextureID);
        m_SurfaceTexture.setOnFrameAvailableListener(this);
        m_Surface = new Surface(m_SurfaceTexture);
        mHandler.sendEmptyMessage(1);
        return true;
    }

    private Handler handler1 = new Handler();
    private ArrayList<VideoPlayer> videoPlayers = new ArrayList<>();
    /**
     * 主线程中初始化VideoPlayer
     */
    private Handler mHandler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            super.handleMessage(msg);

            //解决265编码播放问题
            AdaptConfigMgr.getInstance().getDebugHelper().setHevcDecodeModeForDebug(1);
            for (VideoPlayer videoPlayer2 : videoPlayers) {
                if (videoPlayer2 != null) {
                    videoPlayer2.release();
                    videoPlayer2 = null;
                }
            }
            videoPlayers.clear();

            videoPlayer = new VideoPlayer(m_UnityActivity);
            videoPlayers.add(videoPlayer);
            videoPlayer.setDisplay(m_Surface);
            videoPlayer.ijkMediaPlayer.setOnPreparedListener(mOnPreparedListener);
            videoPlayer.ijkMediaPlayer.setOnCompletionListener(mOnCompletionListener);
            videoPlayer.ijkMediaPlayer.setOnErrorListener(mOnErrorListener);
            videoPlayer.setEventListener(mEventListener);

            if (mPlayType == PlayType.live) {
                m_strFileName = m_strFileName.replaceAll("(" + "ikFastRate" + "=[^&]*)", "ikFastRate" + "=" + 1.1);
                m_strFileName = m_strFileName.replaceAll("(" + "ikMinBuf" + "=[^&]*)", "ikMinBuf" + "=" + 3000);
                m_strFileName = m_strFileName.replaceAll("(" + "ikMaxBuf" + "=[^&]*)", "ikMaxBuf" + "=" + 4000);
                videoPlayer.setStreamType(VideoPlayer.PlayerStreamType.PLAYER_STREAM_TYPE_LIVE);
            } else if (mPlayType == PlayType.vod) {
                videoPlayer.setStreamType(VideoPlayer.PlayerStreamType.PLAYER_STREAM_TYPE_LOCAL_FILE);
            } else if (mPlayType == PlayType.local) {
                videoPlayer.setStreamType(VideoPlayer.PlayerStreamType.PLAYER_STREAM_TYPE_LOCAL_FILE);
            }

            if (m_strFileName.contains("ikPullHevc")) {
                m_strFileName = m_strFileName.replaceAll("(" + "ikPullHevc" + "=[^&]*)", "ikPullHevc" + "=" + 1);
            } else {
                String s = "&";
                if (!m_strFileName.contains("?")) {
                    s = "?";
                }
                m_strFileName = m_strFileName + s + "ikPullHevc=1";
            }

            Log.i("zb123", "m_strFileName = " + m_strFileName + ", mPlayType = " + mPlayType);

            videoPlayer.setForceHardDecode(true);

            videoPlayer.setStreamUrl(m_strFileName, false);

            videoPlayer.start();

            handler1.removeCallbacksAndMessages(null);
            if (mPlayModelType == PlayModelType._Plane) {
                handler1.postDelayed(new Runnable() {
                    @Override
                    public void run() {
                        try {
                            Load();
                        } catch (Exception e) {
                            e.printStackTrace();
                        }
                    }
                }, 3000);
            }
        }
    };

    synchronized public void onFrameAvailable(SurfaceTexture surface) {
        m_bUpdate = true;
    }


    @TargetApi(23)
    public void SetSpeed(float fSpeed) {

        //todo videoPlayer.setPlaybackParams(videoPlayer.getPlaybackParams().setSpeed(fSpeed));
    }


    public void UpdateVideoTexture() {

        if (m_bUpdate == false)
            return;

        if (videoPlayer != null) {
            if (m_iCurrentState == MEDIAPLAYER_STATE.PLAYING || m_iCurrentState == MEDIAPLAYER_STATE.PAUSED) {

                SetManagerID(m_iNativeMgrID);


                boolean[] abValue = new boolean[1];
                GLES20.glGetBooleanv(GLES20.GL_DEPTH_TEST, abValue, 0);
                GLES20.glDisable(GLES20.GL_DEPTH_TEST);
                m_SurfaceTexture.updateTexImage();


                float[] mMat = new float[16];


                m_SurfaceTexture.getTransformMatrix(mMat);

                RenderScene(mMat, m_iSurfaceTextureID, m_iUnityTextureID);


                if (abValue[0]) {
                    GLES20.glEnable(GLES20.GL_DEPTH_TEST);
                } else {

                }

                abValue = null;

            }
        }
    }


    public void SetRockchip(boolean bValue) {
        m_bRockchip = bValue;
    }


    public void SetLooping(boolean bLoop) {
        if (videoPlayer != null) {
//            videoPlayer.setLooping(bLoop);
        }
    }

    public void SetVolume(float fVolume) {

        if (videoPlayer != null) {
            videoPlayer.setVolume(fVolume);
        }


    }

    public void SetVolume2(float fVolume1, float fVolume2) {
        if (videoPlayer != null) {
            videoPlayer.setVolume(fVolume1);
        }
    }


    public void SetSeekPosition(int iSeek) {
        if (videoPlayer != null) {
            if (m_iCurrentState == MEDIAPLAYER_STATE.READY || m_iCurrentState == MEDIAPLAYER_STATE.PLAYING || m_iCurrentState == MEDIAPLAYER_STATE.PAUSED) {
                videoPlayer.ijkMediaPlayer.seekTo(iSeek);
            }
        }
    }

    public int GetSeekPosition() {
        if (videoPlayer != null) {
            if (m_iCurrentState == MEDIAPLAYER_STATE.READY || m_iCurrentState == MEDIAPLAYER_STATE.PLAYING || m_iCurrentState == MEDIAPLAYER_STATE.PAUSED) {
                try {
                    m_iCurrentSeekPosition = (int) videoPlayer.getCurrentPosition();
                } catch (SecurityException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                } catch (IllegalStateException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                }
            }
        }

        return m_iCurrentSeekPosition;
    }

    public int GetCurrentSeekPercent() {
        return m_iCurrentSeekPercent;
    }


    public void Play(int iSeek) {
        Log.i("zb123", "Play");

        if (videoPlayer != null) {
            if (m_iCurrentState == MEDIAPLAYER_STATE.READY || m_iCurrentState == MEDIAPLAYER_STATE.PAUSED || m_iCurrentState == MEDIAPLAYER_STATE.END) {

                //videoPlayer.seekTo(iSeek);
                videoPlayer.start();

                m_iCurrentState = MEDIAPLAYER_STATE.PLAYING;

            }
        }
    }

    public void Reset() {
        Log.i("zb123", "Reset");
        if (videoPlayer != null) {
            if (m_iCurrentState == MEDIAPLAYER_STATE.PLAYING) {
                videoPlayer.reset();

            }

        }
        m_iCurrentState = MEDIAPLAYER_STATE.NOT_READY;
    }

    public void Stop() {
        Log.i("zb123", "Stop");
        if (videoPlayer != null) {
            if (m_iCurrentState == MEDIAPLAYER_STATE.PLAYING) {
                videoPlayer.stop();
                m_iCurrentState = MEDIAPLAYER_STATE.STOPPED;
            }

        }
        m_iCurrentState = MEDIAPLAYER_STATE.NOT_READY;
    }

    public void RePlay(int position) {
        Log.i("zb123", "REPlay");
        try {
            Load();
        } catch (IOException e) {
            e.printStackTrace();
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    public void Pause() {
        Log.i("zb123", "Pause");
        if (videoPlayer != null) {
            if (m_iCurrentState == MEDIAPLAYER_STATE.PLAYING) {
                UnLoad();

//                videoPlayer.pause();
//                m_iCurrentState = MEDIAPLAYER_STATE.PAUSED;
            }
        }
    }

    public void Pause2() {
        Log.i("zb123", "Pause2");
        if (videoPlayer != null) {
            videoPlayer.pause();
        }
    }

    public void Resume() {
        Log.i("zb123", "Resume" + videoPlayer.getCurrentPosition());
        if (videoPlayer != null) {
            if (mPlayType != PlayType.live) {
                RePlay((int) videoPlayer.getCurrentPosition());
            }
        }
    }

    public int GetVideoWidth() {
        if (videoPlayer != null) {
            return videoPlayer.ijkMediaPlayer.getVideoWidth();
        }

        return 0;
    }

    public int GetVideoHeight() {
        if (videoPlayer != null) {
            return videoPlayer.ijkMediaPlayer.getVideoHeight();
        }

        return 0;
    }

    public boolean IsUpdateFrame() {
        return m_bUpdate;
    }

    public void SetUnityTexture(int iTextureID) {
        m_iUnityTextureID = iTextureID;
        SetManagerID(m_iNativeMgrID);
        SetUnityTextureID(m_iUnityTextureID);

    }

    public void SetUnityTextureID(Object texturePtr) {

    }


    public void SetSplitOBB(boolean bValue, String strOBBName) {
        m_bSplitOBB = bValue;
        m_strOBBName = strOBBName;
    }

    public int GetDuration() {
        if (videoPlayer != null) {
            return (int) videoPlayer.getDuration();
        }

        return -1;
    }


    public int InitNative(EasyMovieTexture obj) {


        m_iNativeMgrID = InitNDK(obj);
        m_objCtrl.add(this);

        return m_iNativeMgrID;

    }

    public void SetUnityActivity(Activity unityActivity) {

        SetManagerID(m_iNativeMgrID);
        m_UnityActivity = unityActivity;
        SetAssetManager(m_UnityActivity.getAssets());
    }


    public void NDK_SetFileName(String strFileName) {
        Log.i("zb123", "NDK_SetFileName  = " + strFileName);
        m_strFileName = strFileName;
    }

    public void NDK_SetPlayType(int playType, int playModelType) {
        Log.i("zb123", "NDK_SetPlayType  = " + playType + ", " + playModelType);
        mPlayType = playType;
        mPlayModelType = playModelType;
    }

    public void InitJniManager() {
        SetManagerID(m_iNativeMgrID);
        InitApplication();
    }


    public int GetStatus() {
        return m_iCurrentState.GetValue();
    }

    public void SetNotReady() {
        m_iCurrentState = MEDIAPLAYER_STATE.NOT_READY;
    }

    public void SetWindowSize() {

        SetManagerID(m_iNativeMgrID);
        SetWindowSize(GetVideoWidth(), GetVideoHeight(), m_iUnityTextureID, m_bRockchip);


    }

    public int GetError() {
        return m_iErrorCode;
    }

    public int GetErrorExtra() {
        return m_iErrorCodeExtra;
    }

    private boolean isPrepared = false;
    private IMediaPlayer.OnPreparedListener mOnPreparedListener = new IMediaPlayer.OnPreparedListener() {
        @Override
        public void onPrepared(IMediaPlayer iMediaPlayer) {
            if (isPrepared) {
                return;
            }
            if (videoPlayer == null) {
                return;
            }
            Log.i("zb123", "onPrepared");
            m_iCurrentState = MEDIAPLAYER_STATE.READY;
            SetManagerID(m_iNativeMgrID);
            m_iCurrentSeekPercent = 0;
            videoPlayer.ijkMediaPlayer.setOnBufferingUpdateListener(mOnBufferingUpdateListener);
        }
    };

    private IMediaPlayer.OnBufferingUpdateListener mOnBufferingUpdateListener = new IMediaPlayer.OnBufferingUpdateListener() {
        @Override
        public void onBufferingUpdate(IMediaPlayer iMediaPlayer, int i) {
            if (iMediaPlayer == videoPlayer) {
                m_iCurrentSeekPercent = i;
            }
        }
    };

    private IMediaPlayer.OnCompletionListener mOnCompletionListener = new IMediaPlayer.OnCompletionListener() {
        @Override
        public void onCompletion(IMediaPlayer iMediaPlayer) {
            m_iCurrentState = MEDIAPLAYER_STATE.END;
            if (mPlayType != PlayType.live) {
                RePlay(0);
            }
        }
    };

    private IMediaPlayer.OnErrorListener mOnErrorListener = new IMediaPlayer.OnErrorListener() {
        @Override
        public boolean onError(IMediaPlayer iMediaPlayer, int i, int i1) {
            Log.i("zb123", "onError i = " + i + ", i1 = " + i1);
            if (iMediaPlayer == videoPlayer) {
                String strError;
                switch (i) {
                    case MediaPlayer.MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK:
                        strError = "MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK";
                        break;
                    case MediaPlayer.MEDIA_ERROR_SERVER_DIED:
                        strError = "MEDIA_ERROR_SERVER_DIED";
                        break;
                    case MediaPlayer.MEDIA_ERROR_UNKNOWN:
                        strError = "MEDIA_ERROR_UNKNOWN";
                        break;
                    default:
                        strError = "Unknown error " + i;
                }

                m_iErrorCode = i;
                m_iErrorCodeExtra = i1;

                m_iCurrentState = MEDIAPLAYER_STATE.ERROR;

                return true;
            }
            return false;
        }
    };

    private VideoEvent.EventListener mEventListener = new VideoEvent.EventListener() {
        @Override
        public void onVideoEvent(int i) {
            // Log.i("zb123","OnPlayerEvent  = " + i);
            if (i == VideoEvent.VIDEO_FIRST_RENDERING) {
                handler1.removeCallbacksAndMessages(null);
            }
        }
    };

    private String getUrl() {
        String url = "";
        File file = new File("/mnt/sdcard/vr_url_cfg.txt");
        if (!file.exists()) {
            try {
                file.createNewFile();
            } catch (IOException e) {
                e.printStackTrace();
            }
            return url;
        }
        String str = null;
        try {
            InputStream is = new FileInputStream(file);
            InputStreamReader input = new InputStreamReader(is, "UTF-8");
            BufferedReader reader = new BufferedReader(input);
            while ((str = reader.readLine()) != null) {
                url += str;
            }
            return url;
        } catch (FileNotFoundException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        } catch (IOException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }
        return url;
    }

}
