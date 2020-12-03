using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveRoomDataHolder{
    public enum PlayType
    {
        _180_3D = 0,
        _360 = 1,
        _Plane = 2,
        _360_3D = 3
    }

    public enum StreamType
    {
        //直播
        live = 0,
        //点播
        vod = 1,
        //本地播放
        local = 2
    }   

    //直播流地址
	public static string streamUrl;
    //播放类型
	public static PlayType playType;
    //流类型
    public static StreamType streamType;

    //设置播放数据
    //需要在跳转播放场景之前调用
    public static void SetLiveRoomData(PlayType playType1, string streamUrl1, StreamType streamType1)
    {
        playType = playType1;
        streamUrl = streamUrl1;
        streamType = streamType1;
    }

}
