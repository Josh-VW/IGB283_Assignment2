using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject linearAvatar;
    public GameObject advancedAvatar;

    public float minOrthographicSize = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AdjustCamera();
    }

    // Adjust camera view so that avatars remain on-screen
    private void AdjustCamera()
    {
        // Get the root limbs of both avatars
        Limb rootLimb1 = linearAvatar.GetComponent<LinearAvatar>().rootObject.GetComponent<Limb>(); ;
        Limb rootLimb2 = advancedAvatar.GetComponent<AdvancedAvatar>().rootObject.GetComponent<Limb>();

        // Find the x distance between both avatars
        float avatarDistance = Mathf.Abs(rootLimb1.position.x - rootLimb2.position.x);

        // Find the x midpoint between the two avatars
        float midpoint = (rootLimb1.position.x + rootLimb2.position.x) / 2;

        // Set the camera's position to the midpoint
        Vector3 position = Camera.main.transform.position;
        position.x = midpoint;
        Camera.main.transform.position = position;

        // Find desired camera width from avatar distance plus padding
        float padding = 0.2f; // 20%
        float cameraWidth = avatarDistance / (1.0f - padding);

        // Get the aspect ratio of the camera view
        float aspectRatio = Camera.main.aspect;

        // Calculate the orthographic size from aspect ration and camera width
        float cameraHeight = cameraWidth / aspectRatio;
        float orthographicSize = cameraHeight / 2.0f;
        if (orthographicSize < minOrthographicSize) orthographicSize = minOrthographicSize;
        Camera.main.orthographicSize = orthographicSize;
    }
}
