using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GPSManager : MonoBehaviour
{
    string message = "Initialising GPS...";

    public Transform trackedImageTransform;
    public bool pointerMarkerDetection;

    public float accuracy = 5f;
    public float displacement = 0f;
    public GameObject needle;

    public static float heading;
    public static float distance;


    private MultiImageTracker get_trackedImageTransform;
    public GameObject GetMultiImageTracker;



        // Start is called before the first frame update
    void Start() {
        StartCoroutine(BeginGPS());
        InvokeRepeating("CustomUpdate", 1, 0.3f);
        get_trackedImageTransform = GetMultiImageTracker.GetComponent<MultiImageTracker>();
    }

    void CustomUpdate()
    {
        if(pointerMarkerDetection == true) {
            heading = Input.compass.trueHeading;
            needle.transform.localRotation = Quaternion.Euler(0, 0, trackedImageTransform.eulerAngles.y + Input.compass.trueHeading);
            message = Quaternion.Euler(0, 0, Input.compass.trueHeading + trackedImageTransform.eulerAngles.y).ToString();
        }
        else if(pointerMarkerDetection == false) {
            heading = Input.compass.trueHeading;
            needle.transform.localRotation = Quaternion.Euler(0, 0, heading);
        }
    }


    IEnumerator BeginGPS() {

        message = "Connecting to Location Services...";

        if (!Input.location.isEnabledByUser) {
            message = "Location Services Not Enabled";
            yield break;
        } 
        else {
            message = "Location Services Enabled"; 
        }

        // Start service before querying location
        Input.location.Start(accuracy, displacement);

        // Wait until service initializes
        int maxWait = 10;

        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 10 seconds
        if (maxWait < 1) {
            message = "Timed out";
            yield break; 
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed) {
            message = "Unable to determine device location";
            yield break;
        }
        else {
            Input.compass.enabled = true; // Used to enable or disable compass
        }

        // Stop service if there is no need to query location updates continuously
        //Input.location.Stop();
    }

    private void OnGUI()
    {
        GUI.skin.label.fontSize = 60;
        GUI.Label(new Rect(30, 30, 1000, 1000), message);
    }
}
