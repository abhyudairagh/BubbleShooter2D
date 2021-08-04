using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraViewControl : MonoBehaviour
{  
    
    [SerializeField]
    SpriteRenderer referenceObject;
    void Start()
    {
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = (float)referenceObject.bounds.size.x / (float)referenceObject.bounds.size.y;
        float cameraSize = 0;
        if (screenRatio >= targetRatio)
        {
            cameraSize = referenceObject.bounds.size.y * 0.5f;
        }
        else
        {
            float diffInSize = targetRatio/screenRatio  ;
            cameraSize = referenceObject.bounds.size.y * 0.5f * diffInSize;
        }
        Camera.main.orthographicSize = cameraSize;
    }

   
}
