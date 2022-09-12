using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    Camera cam;

    [SerializeField] GameObject cameraPoint1;
    [SerializeField] GameObject cameraPoint2;
    // Start is called before the first frame update
    void Start()
    {
        //assign camera
        cam = GetComponent<Camera>();

        int loopLimit = 100;

        for(int i = 0; i < loopLimit; i++)
        {
            //get current position
            Vector3 currentCameraPosition = gameObject.transform.position;
            //move current camera position back to increase frame size
            Vector3 nextCameraPosition = new Vector3(currentCameraPosition.x, currentCameraPosition.y, currentCameraPosition.z - 0.1f);
            //set camera position
            gameObject.transform.position.Set(nextCameraPosition.x, nextCameraPosition.y, nextCameraPosition.z);

            cam.fieldOfView += 1.0f;

            if (CheckIfCamPointsInFrame())
            {
                break;
            }
        }
    }

    bool CheckIfCamPointsInFrame()
    {
        //set bools for checking if camera points are in frame.  default to false because true indicates point is in frame
        bool point1InFrame = false;
        bool point2InFrame = false;

        //check if point viewport coords are within frame and set pointInframe to true if so
        Vector3 point1Viewport = cam.WorldToViewportPoint(cameraPoint1.transform.position);
        if (point1Viewport.x > 0 && point1Viewport.x < 1 && point1Viewport.y > 0 && point1Viewport.y < 1 && point1Viewport.z > 0)
        {
            point1InFrame = true;
        }

        Vector3 point2Viewport = cam.WorldToViewportPoint(cameraPoint2.transform.position);
        if (point2Viewport.x > 0 && point2Viewport.x < 1 && point2Viewport.y > 0 && point2Viewport.y < 1 && point2Viewport.z > 0)
        {
            point2InFrame = true;
        }

        //both points are within frame
        if(point1InFrame && point2InFrame)
        {
            return true;
        }

        //both points are NOT in frame
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
