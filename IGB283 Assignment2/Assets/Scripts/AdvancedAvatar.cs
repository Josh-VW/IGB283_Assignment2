using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class AdvancedAvatar : MonoBehaviour
{
    public GameObject rootObject;

    public float walkSpeed = 5.0f;
    public float[] walkBounds = new float[] { -10.0f, 10.0f };
    public float nodSpeed = 60.0f;
    public float[] nodAngleRange = new float[] { -30.0f, 30.0f };
    public float jumpSpeed = 15.0f;
    public float[] jumpRange = new float[] { 0.0f, 5.0f };

    private bool nodUp = true;
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
        Torso().InitialTransform();
    }

    // Update is called once per frame
    void Update()
    {
        //Walk();
        //HeadNod();
        //JumpUp();
        //Collapse();

        // Test Code: What an abomination this makes
        Torso().Translate(new Vector3(1 * Time.deltaTime, 0.0f, 1.0f)); // Simple translation
        Torso().Rotate(90 * Time.deltaTime, Torso().jointLocation); // Rotate constantly at 90 degress a second clockwise
        Head().Rotate(-180 * Time.deltaTime, Head().jointLocation); // Rotate constantly at 180 degrees a second counter clockwise
        LeftUpperLeg().Rotate(-180 * Time.deltaTime, LeftUpperLeg().jointLocation); // Rotate constantly at 180 degrees a second counter clockwise
        LeftLowerLeg().RotateTowardsAngle(90, 80, out _); // Rotate constantly to try to reach the global angle of 90 degrees at up to 80 degrees a second. Will never reach target as sum of parents rotation (torso 90/s, upper leg -180/s) is greater than rotation speed
        RightUpperLeg().Rotate(180 * Time.deltaTime, RightUpperLeg().jointLocation); // Rotate constantly at 180 degrees a second clockwise
        RightLowerLeg().RotateTowardsAngle(-90, 360, out _); // Rotate constantly to try to reach the global angle of -90 degrees at up to 360 degrees a second

    }

    #region Limb Retrieval Methods

    // Retrieve the limb component of the Torso object
    private Limb Torso()
    {
        return rootObject.GetComponent<Limb>();
    }

    // Retrieve the limb component of the Left Upper Leg object
    private Limb LeftUpperLeg()
    {
        return Torso()
            .children[0].GetComponent<Limb>();
    }

    // Retrieve the limb component of the Left Lower Leg object
    private Limb LeftLowerLeg()
    {
        return LeftUpperLeg()
            .children[0].GetComponent<Limb>();
    }

    // Retrieve the limb component of the Right Upper Leg object
    private Limb RightUpperLeg()
    {
        return Torso()
            .children[1].GetComponent<Limb>();
    }

    // Retrieve the limb component of the Right Lower Leg object
    private Limb RightLowerLeg()
    {
        return RightUpperLeg()
            .children[0].GetComponent<Limb>();
    }

    // Retrieve the limb component of the Head object
    private Limb Head()
    {
        return Torso()
            .children[2].GetComponent<Limb>();
    }

    #endregion

    #region Animation & Control Methods

    // Transform avatar components to inital states
    private void AssembleAvatar()
    {
        Torso().GetComponent<Limb>().AssembleChildren();
    }

    // Translates the avatar from side to side
    private void Walk()
    {
        Limb baseLimb = Torso();

        //Move back and forth automatically or can adjust direction
        if (moving)
        {
            if (forward)
            {
                baseLimb.TranslateTowardsPoint(new Vector3(walkBounds[1], baseLimb.position.y, 1.0f), walkSpeed, out _);
            }

            if (!forward)
            {
                baseLimb.TranslateTowardsPoint(new Vector3(walkBounds[0], baseLimb.position.y, 1.0f), walkSpeed, out _);
            }

            if (Input.GetKey("d") || baseLimb.position.x <= walkBounds[0])
            {
                forward = true;
            }

            if (Input.GetKey("a")|| baseLimb.position.x >= walkBounds[1])
            {
                forward = false;
            }
            
        }
    }

    private void HeadNod()
    {
        // Get the limb component of the head
        Limb headLimb = Head();

        if (collapsing) return;

        if (nodUp)
        {
            headLimb.RotateTowardsAngle(nodAngleRange[1], nodSpeed, out bool rotationReached);
            if (rotationReached) nodUp = false;
        }

        if (!nodUp)
        {
            headLimb.RotateTowardsAngle(nodAngleRange[0], nodSpeed, out bool rotationReached);
            if (rotationReached) nodUp = true;
        }
    }

    // Handles avatar jumping controls and animation
    private void JumpUp()
    {
        Limb baseLimb = Torso();
        Limb lowerArmLimb = LeftUpperLeg();
        Limb upperArmLimb = LeftLowerLeg();
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
            bool heightReached;
            baseLimb.TranslateTowardsPoint(new Vector3(baseLimb.position.x, jumpRange[1], 1.0f), jumpSpeed, out heightReached);
            lowerArmLimb.RotateTowardsAngle(55.0f, 180, out _);
            upperArmLimb.RotateTowardsAngle(-55.0f, 360, out _);
            headLimb.RotateTowardsAngle(0.0f, 180, out _);
            if (heightReached)
            {
                up = false;
            }
        }

        //Downwards translation and limb rotations
        if (jumping && !up)
        {
            bool heightReached;
            baseLimb.TranslateTowardsPoint(new Vector3(baseLimb.position.x, jumpRange[0], 1.0f), jumpSpeed, out heightReached);
            lowerArmLimb.RotateTowardsAngle(0.0f, 180, out _);
            upperArmLimb.RotateTowardsAngle(0.0f, 360, out _);
            headLimb.RotateTowardsAngle(0.0f, 180, out _);
            if (heightReached)
            {
                jumping = false;
                moving = true;

                // Return to original state
                lowerArmLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                upperArmLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                headLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            }
        }
    }

    // Handles avatar collapse controls and animation
    private void Collapse()
    {
        Limb baseLimb = Torso();
        Limb lowerArmLimb = LeftUpperLeg();
        Limb upperArmLimb = LeftLowerLeg();
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
            baseLimb.RotateTowardsAngle(90.0f, 90.0f, out bool collapseReached);
            lowerArmLimb.RotateTowardsAngle(120.0f, 30.0f, out _);
            upperArmLimb.RotateTowardsAngle(60.0f, 90.0f, out _);
            headLimb.RotateTowardsAngle(-60.0f, 60.0f, out _);
            baseLimb.TranslateTowardsPoint(new Vector3(baseLimb.position.x, baseLimb.position.y + 1.0f, 1.0f), 1, out _);

            //if (baseLimb.rotation >= 90.0f)
            if (collapseReached)
            {
                wait = 1.0f + Time.time;                
                fall = false;                
            }
        }
        //Rise forwards and limb movement
        if (collapsing && !fall && wait <= Time.time)
        {
            baseLimb.RotateTowardsAngle(0.0f, 90.0f, out bool riseReached);
            lowerArmLimb.RotateTowardsAngle(0.0f, 30.0f, out _);
            upperArmLimb.RotateTowardsAngle(0.0f, 120.0f, out _);
            headLimb.RotateTowardsAngle(0.0f, 60.0f, out _);
            baseLimb.TranslateTowardsPoint(new Vector3(baseLimb.position.x, baseLimb.position.y - 1.0f, 1.0f), 1, out _);

            //if (baseLimb.rotation <= 0.0f)
            if (riseReached)
            {
                collapsing = false;
                moving = true;

                // Return to original state
                baseLimb.TranslateTowardsPoint(new Vector3(baseLimb.position.x, 0.0f, 1.0f), float.MaxValue, out _);
                lowerArmLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                upperArmLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                headLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            }
        }
    }

    #endregion
}
