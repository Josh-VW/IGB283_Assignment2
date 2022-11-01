using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearAvatar : MonoBehaviour
{
    public GameObject rootObject;
    private bool jumping = false;
    private bool up = true;
    private bool moving = true;
    private bool forward = true;
    private bool collapsing = false;
    private bool fall = true;
    private float wait;

    // Start is called before the first frame update
    void Start()
    {
        AssembleAvatar();
    }

    // Update is called once per frame
    void Update()
    {
        Limb baseLimb = Base();
        //baseLimb.Translate(new Vector3(3 * Time.deltaTime, 0, 0));
        Limb upperArmLimb = UpperArm();
        //upperArmLimb.Scale(new Vector3(1.0f, 0.99f, 1.0f), upperArmLimb.jointLocation);
        //upperArmLimb.Rotate(-180 * Time.deltaTime, upperArmLimb.jointLocation);

        Walk();
        JumpUp();
        Collapse();
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

    private void Walk()
    {
        Limb baseLimb = Base();

        //Move back and forth automatically or can adjust direction
        if (moving)
        {
            if (forward)
            {
                baseLimb.Translate(new Vector3(5 * Time.deltaTime, 0, 0));
            }

            if (!forward)
            {
                baseLimb.Translate(new Vector3(-5 * Time.deltaTime, 0, 0));
            }

            if (Input.GetKey("d") || baseLimb.position.x <= -10.0f)
            {
                forward = true;
            }

            if (Input.GetKey("a")|| baseLimb.position.x >= 10.0f)
            {
                forward = false;
            }
            
        }
    }

    private void JumpUp()
    {
        Limb baseLimb = Base();
        Limb lowerArmLimb = LowerArm();
        Limb upperArmLimb = UpperArm();
        Limb headLimb = Head();

        // Jump straight upwards
        if (Input.GetKey("w") && !jumping && moving)
        {
            jumping = true;
            up = true;
            moving = false;
        }

        //Jump while moving in a direction
        if (Input.GetKey("s") && !jumping && moving)
        {
            jumping = true;
            up = true;
        }
        
        //Upwards translation and limb rotations
        if (jumping && up)
        {
            baseLimb.Translate(new Vector3(0, 15 * Time.deltaTime, 0));
            lowerArmLimb.Rotate(180 * Time.deltaTime, upperArmLimb.jointLocation);
            upperArmLimb.Rotate(-360 * Time.deltaTime, upperArmLimb.jointLocation);
            headLimb.Rotate(180 * Time.deltaTime, headLimb.jointLocation);
            if (baseLimb.position.y >= 5.0f)
            {
                up = false;
            }
        }

        //Downwards translation and limb rotations
        if (jumping && !up)
        {
            baseLimb.Translate(new Vector3(0, -15 * Time.deltaTime, 0));
            lowerArmLimb.Rotate(-180 * Time.deltaTime, upperArmLimb.jointLocation);
            upperArmLimb.Rotate(360 * Time.deltaTime, upperArmLimb.jointLocation);
            headLimb.Rotate(-180 * Time.deltaTime, headLimb.jointLocation);
             if (baseLimb.position.y <= 0.0f)
            {
                jumping = false;
                moving = true;
            }
        }
    }

    private void Collapse()
    {
        Limb baseLimb = Base();
        Limb lowerArmLimb = LowerArm();
        Limb upperArmLimb = UpperArm();
        Limb headLimb = Head();

        //Collapse while not jumping and still moving
        if (Input.GetKey("z") && !jumping && moving)
        {
            collapsing = true;
            fall = true;
            moving = false;
        }
        //Fall backwards and limb movement
        if (collapsing && fall)
        {
            baseLimb.Rotate(90 * Time.deltaTime, baseLimb.jointLocation);
            lowerArmLimb.Rotate(-30 * Time.deltaTime, upperArmLimb.jointLocation);
            upperArmLimb.Rotate(30 * Time.deltaTime, upperArmLimb.jointLocation);
            headLimb.Rotate(-60 * Time.deltaTime, headLimb.jointLocation);
            baseLimb.Translate(new Vector3(0, 1 * Time.deltaTime, 0));
            if (baseLimb.rotation >= 90.0f)
            {
                wait = 1.0f + Time.time;                
                fall = false;                
            }
        }
        //Rise forwards and limb movement
        if (collapsing && !fall && wait <= Time.time)
        {
            baseLimb.Rotate(-90 * Time.deltaTime, baseLimb.jointLocation);
            lowerArmLimb.Rotate(30 * Time.deltaTime, upperArmLimb.jointLocation);
            upperArmLimb.Rotate(-30 * Time.deltaTime, upperArmLimb.jointLocation);
            headLimb.Rotate(60 * Time.deltaTime, headLimb.jointLocation);
            baseLimb.Translate(new Vector3(0, -1 * Time.deltaTime, 0));
            if (baseLimb.rotation <= 0.0f)
            {
                collapsing = false;
                moving = true;
            }
        }
    }
}
