/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_RotateScene.cs
 *   
*************************************************************/

using UnityEngine;

public class NoloVR_RotateScene : MonoBehaviour {
    //sence object
    public Transform objectParents;
    //Whether to change the scale
    public bool isChangeScale;
    //Whether to change the rotation
    public bool isChangeRotation;
    
    Transform leftController;
    Transform rightController;
    GameObject controllerCenter;

    float originDistance = -1;
    float distance = 0;

    float scaling = 1f;
    float preScaling = 1f;

    Vector3 preVetor = Vector3.zero;
    Vector3 vetor = Vector3.zero;
    Vector3 prerotation = Vector3.zero;
    private float maxAngel = 140;
    void Start()
    {
        foreach (NoloVR_TrackedDevice item in NoloVR_System.GetInstance().objects)
        {
            if (item.deviceType == NoloDeviceType.LeftController)
            {
                leftController = item.gameObject.transform;
            }
            if (item.deviceType == NoloDeviceType.RightController)
            {
                rightController = item.gameObject.transform;
            }
        }
        controllerCenter = new GameObject("controllerCenter");
    }
    void Update () {
        //Change nolovrmanager's rotation
        //Double controller press the grip button at the same time
        if (NoloVR_System.GetInstance().realTrackDevices == 6)
        {
            if (NoloVR_Controller.GetDevice(NoloDeviceType.LeftController).GetNoloButtonPressed(NoloButtonID.Grip)
                    && NoloVR_Controller.GetDevice(NoloDeviceType.RightController).GetNoloButtonPressed(NoloButtonID.Grip))
            {
                //Record the middle of the two controller
                controllerCenter.transform.position = new Vector3((rightController.position.x + leftController.position.x) / 2,
                                                                    (rightController.position.y + leftController.position.y) / 2,
                                                                        (rightController.position.z + leftController.position.z) / 2);
                //Set the parent of the sence object
                objectParents.SetParent(controllerCenter.transform);

                //Change scale
                if (isChangeScale)
                {
                    distance = Vector3.Distance(leftController.position, rightController.position);
                    if (originDistance < 0)
                    {
                        originDistance = distance;
                    }
                    scaling = preScaling + distance - originDistance;
                    //Min scale 0.1
                    if (scaling < 0.1f)
                    {
                        scaling = 0.1f;
                    }
                    controllerCenter.transform.localScale = new Vector3(scaling, scaling, scaling);
                }

                //Change rotation
                if (isChangeRotation)
                {

                    vetor = new Vector3(rightController.localPosition.x - leftController.localPosition.x, 0, rightController.localPosition.z - leftController.localPosition.z);
                    if (preVetor == Vector3.zero)
                    {
                        preVetor = vetor;
                    }
                    float angle = Mathf.Acos(Vector3.Dot(preVetor.normalized, vetor.normalized)) * Mathf.Rad2Deg;
                    //Max rotation angle
                    if (angle > maxAngel)
                    {
                        return;
                    }
                    //Filter illegal numbers
                    if (float.IsNaN(angle))
                    {
                        return;
                    }
                    if (rightController.localPosition.z - leftController.localPosition.z - preVetor.z > 0)
                    {
                        angle = -angle;
                    }
                    controllerCenter.transform.rotation = Quaternion.Euler(prerotation + new Vector3(0, angle, 0));
                }
            }
            else
            {
                objectParents.SetParent(null);
                originDistance = -1;
                preScaling = controllerCenter.transform.localScale.x;
                preVetor = Vector3.zero;
                prerotation = controllerCenter.transform.localRotation.eulerAngles;
            }
        }
    }
}

