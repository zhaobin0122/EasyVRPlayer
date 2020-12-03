using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveFrameModelManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //test
        LiveRoomDataHolder.SetLiveRoomData(LiveRoomDataHolder.PlayType._180_3D, "http://record2.inke.cn/vr.mp4?ikPullHevc=1", LiveRoomDataHolder.StreamType.vod);
        UpdateLiveFrameModel();
    }

    /*
     * 刷新直播画面模型
     */
    public void UpdateLiveFrameModel()
    {
        RemoveAllChilds();
        string liveFrameModelBasePath = "VideoPlayer/Prefabs";
        string liveFrameModelPath = "";


        if (LiveRoomDataHolder.playType == LiveRoomDataHolder.PlayType._180_3D)
        {
            liveFrameModelPath = liveFrameModelBasePath + "/180_3D_model";
        }
        else if (LiveRoomDataHolder.playType == LiveRoomDataHolder.PlayType._360)
        {
            liveFrameModelPath = liveFrameModelBasePath + "/360_model";
        }
        else if (LiveRoomDataHolder.playType == LiveRoomDataHolder.PlayType._Plane)
        {
            liveFrameModelPath = liveFrameModelBasePath + "/plane_model";
        }
        else if (LiveRoomDataHolder.playType == LiveRoomDataHolder.PlayType._360_3D)
        {
            liveFrameModelPath = liveFrameModelBasePath + "/360_3D_model";
        }

        GameObject liveFrameModelObject = (GameObject)Resources.Load(liveFrameModelPath);
        liveFrameModelObject = Instantiate(liveFrameModelObject);

        liveFrameModelObject.transform.parent = gameObject.transform;

    }

    /**
     * 移除原有的直播画面模型
     */
    private void RemoveAllChilds()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private IEnumerator RePlayDelay()
    {
        yield return new WaitForSeconds(1);
        UpdateLiveFrameModel();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

}
