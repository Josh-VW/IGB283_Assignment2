using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearAvatar : MonoBehaviour
{
    public GameObject rootObject;

    // Start is called before the first frame update
    void Start()
    {
        AssembleAvatar();
    }

    // Update is called once per frame
    void Update()
    {
        Limb baseLimb = Base();
        baseLimb.Translate(new Vector3(3 * Time.deltaTime, 0, 0));
        Limb upperArmLimb = UpperArm();
        upperArmLimb.Scale(new Vector3(1.0f, 0.99f, 1.0f), upperArmLimb.jointLocation);
        upperArmLimb.Rotate(-180 * Time.deltaTime, upperArmLimb.jointLocation);
    }

    // Retrieve the limb component of the Base object
    private Limb Base()
    {
        return rootObject.GetComponent<Limb>(); // Base
    }

    // Retrieve the limb component of the Lower Arm object
    private Limb LowerArm()
    {
        return rootObject.GetComponent<Limb>() // Base
            .children[0].GetComponent<Limb>(); // Lower Limb
    }

    // Retrieve the limb component of the Upper Arm object
    private Limb UpperArm()
    {
        return rootObject.GetComponent<Limb>() // Base
            .children[0].GetComponent<Limb>() // Lower Limb
            .children[0].GetComponent<Limb>(); // Upper Limb
    }

    // Retrieve the limb component of the Head object
    private Limb Head()
    {
        return rootObject.GetComponent<Limb>() // Base
            .children[0].GetComponent<Limb>() // Lower Limb
            .children[0].GetComponent<Limb>() // Upper Limb
            .children[0].GetComponent<Limb>(); // Head
    }

    private void AssembleAvatar()
    {
        Base().GetComponent<Limb>().AssembleChildren();
    }
}
