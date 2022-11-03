using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class AdvancedAvatar : MonoBehaviour
{
    public GameObject rootObject;

    public float walkSpeed = 5.0f;
    public float[] walkBounds = new float[] { -20.0f, 20.0f };
    public float nodSpeed = 60.0f;
    public float[] nodAngleRange = new float[] { -30.0f, 30.0f };
    public float jumpSpeed = 15.0f;
    public float[] jumpRange = new float[] { 3.0f, 7.0f };

    private bool nodUp = true;
    private bool jumping = false;
    private bool up = true;
    private bool moving = true;
    private bool forward = false;
    private bool collapsing = false;
    private bool fall = true;
    private bool stepUp = true;
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
        Walk();
        HeadNod();
        JumpUp();
        Collapse();
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
        Limb torsoLimb = Torso();
        Limb leftUpperLegLimb = LeftUpperLeg();
        Limb leftLowerLegLimb = LeftLowerLeg();
        Limb rightUpperLegLimb = RightUpperLeg();
        Limb rightLowerLegLimb = RightLowerLeg();

        //Move back and forth automatically or can adjust direction
        if (moving)
        {
            if (forward)
            {
                torsoLimb.TranslateTowardsPoint(new Vector3(walkBounds[1], torsoLimb.position.y, 1.0f), walkSpeed, out _);

                if(stepUp && !jumping)
                {
                    bool stepComplete;
                    leftUpperLegLimb.RotateTowardsAngle(-50.0f, 360, out _);
                    leftLowerLegLimb.RotateTowardsAngle(-60.0f, 360, out _);
                    rightUpperLegLimb.RotateTowardsAngle(50.0f, 180, out stepComplete);
                    rightLowerLegLimb.RotateTowardsAngle(-10.0f, 360, out _);
                    if(stepComplete)
                    {
                        stepUp = false;
                    }
                    
                }
                if(!stepUp && !jumping)
                {
                    bool stepComplete;
                    leftUpperLegLimb.RotateTowardsAngle(50.0f, 180, out stepComplete);
                    leftLowerLegLimb.RotateTowardsAngle(-10.0f, 360, out _);
                    rightUpperLegLimb.RotateTowardsAngle(-50.0f, 360, out _);
                    rightLowerLegLimb.RotateTowardsAngle(-60.0f, 360, out _);
                    if(stepComplete)
                    {
                        stepUp = true;
                    }
                }
            }

            if (!forward)
            {
                torsoLimb.TranslateTowardsPoint(new Vector3(walkBounds[0], torsoLimb.position.y, 1.0f), walkSpeed, out _);

                if(stepUp && !jumping)
                {
                    bool stepComplete;
                    leftUpperLegLimb.RotateTowardsAngle(50.0f, 360, out _);
                    leftLowerLegLimb.RotateTowardsAngle(60.0f, 360, out _);
                    rightUpperLegLimb.RotateTowardsAngle(-50.0f, 180, out stepComplete);
                    rightLowerLegLimb.RotateTowardsAngle(10.0f, 360, out _);
                    if(stepComplete)
                    {
                        stepUp = false;
                    }
                }
                if(!stepUp && !jumping)
                {
                    bool stepComplete;
                    leftUpperLegLimb.RotateTowardsAngle(-50.0f, 180, out stepComplete);
                    leftLowerLegLimb.RotateTowardsAngle(10.0f, 360, out _);
                    rightUpperLegLimb.RotateTowardsAngle(50.0f, 360, out _);
                    rightLowerLegLimb.RotateTowardsAngle(60.0f, 360, out _);
                    if(stepComplete)
                    {
                        stepUp = true;
                    }
                }
            }

            if (Input.GetKey("l") || torsoLimb.position.x <= walkBounds[0])
            {
                forward = true;
            }

            if (Input.GetKey("j")|| torsoLimb.position.x >= walkBounds[1])
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
        Limb torsoLimb = Torso();
        Limb leftUpperLegLimb = LeftUpperLeg();
        Limb leftLowerLegLimb = LeftLowerLeg();
        Limb rightUpperLegLimb = RightUpperLeg();
        Limb rightLowerLegLimb = RightLowerLeg();
        Limb headLimb = Head();

        // Jump straight upwards
        if (Input.GetKey("i") && !jumping && moving)
        {
            jumping = true;
            up = true;
            moving = false;

            //Reset to original positions
            leftUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            leftLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            rightUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            rightLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
        }

        //Jump while moving in a direction
        if (Input.GetKey("k") && !jumping && moving)
        {
            jumping = true;
            up = true;

            //Reset to original positions
            leftUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            leftLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            rightUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            rightLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
        }
        
        //Upwards translation and limb rotations
        if (jumping && up)
        {
            bool heightReached;
            torsoLimb.TranslateTowardsPoint(new Vector3(torsoLimb.position.x, jumpRange[1], 1.0f), jumpSpeed, out heightReached);
            leftUpperLegLimb.RotateTowardsAngle(-50.0f, 180, out _);
            leftLowerLegLimb.RotateTowardsAngle(-20.0f, 360, out _);
            rightUpperLegLimb.RotateTowardsAngle(50.0f, 180, out _);
            rightLowerLegLimb.RotateTowardsAngle(20.0f, 360, out _);
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
            torsoLimb.TranslateTowardsPoint(new Vector3(torsoLimb.position.x, jumpRange[0], 1.0f), jumpSpeed, out heightReached);
            leftUpperLegLimb.RotateTowardsAngle(0.0f, 180, out _);
            leftLowerLegLimb.RotateTowardsAngle(0.0f, 360, out _);
            rightUpperLegLimb.RotateTowardsAngle(0.0f, 180, out _);
            rightLowerLegLimb.RotateTowardsAngle(0.0f, 360, out _);
            headLimb.RotateTowardsAngle(0.0f, 180, out _);
            if (heightReached)
            {
                jumping = false;
                moving = true;

                // Return to original state
                leftUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                leftLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                rightUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                rightLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                headLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            }
        }
    }

    // Handles avatar collapse controls and animation
    private void Collapse()
    {
        Limb torsoLimb = Torso();
        Limb leftUpperLegLimb = LeftUpperLeg();
        Limb leftLowerLegLimb = LeftLowerLeg();
        Limb rightUpperLegLimb = RightUpperLeg();
        Limb rightLowerLegLimb = RightLowerLeg();
        Limb headLimb = Head();

        //Collapse while not jumping and still moving
        if (Input.GetKey("m") && !jumping && moving)
        {
            collapsing = true;
            fall = true;
            moving = false;

            //Reset to original positions
            leftUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            leftLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            rightUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            rightLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
        }
        //Fall backwards and limb movement
        if (collapsing && fall)
        {
            torsoLimb.RotateTowardsAngle(90.0f, 90.0f, out bool collapseReached);
            leftUpperLegLimb.RotateTowardsAngle(140.0f, 70.0f, out _);
            leftLowerLegLimb.RotateTowardsAngle(80.0f, 120.0f, out _);
            rightUpperLegLimb.RotateTowardsAngle(160.0f, 90.0f, out _);
            rightLowerLegLimb.RotateTowardsAngle(80.0f, 120.0f, out _);
            headLimb.RotateTowardsAngle(120.0f, 50.0f, out _);
            torsoLimb.TranslateTowardsPoint(new Vector3(torsoLimb.position.x, torsoLimb.position.y - 5.0f, 5.0f), 3, out _);

            //if (torsoLimb.rotation >= 90.0f)
            if (collapseReached)
            {
                wait = 1.0f + Time.time;                
                fall = false;                
            }
        }
        //Rise forwards and limb movement
        if (collapsing && !fall && wait <= Time.time)
        {
            torsoLimb.RotateTowardsAngle(0.0f, 90.0f, out bool riseReached);
            leftUpperLegLimb.RotateTowardsAngle(0.0f, 60.0f, out _);
            leftLowerLegLimb.RotateTowardsAngle(0.0f, 180.0f, out _);
            rightUpperLegLimb.RotateTowardsAngle(0.0f, 60.0f, out _);
            rightLowerLegLimb.RotateTowardsAngle(0.0f, 180.0f, out _);
            headLimb.RotateTowardsAngle(0.0f, 60.0f, out _);
            torsoLimb.TranslateTowardsPoint(new Vector3(torsoLimb.position.x, torsoLimb.position.y + 5.0f, 5.0f), 3, out _);

            //if (torsoLimb.rotation <= 0.0f)
            if (riseReached)
            {
                collapsing = false;
                moving = true;

                // Return to original state
                torsoLimb.TranslateTowardsPoint(new Vector3(torsoLimb.position.x, 3.0f, 1.0f), float.MaxValue, out _);
                leftUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                leftLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                rightUpperLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                rightLowerLegLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
                headLimb.RotateTowardsAngle(0.0f, float.MaxValue, out _);
            }
        }
    }

    #endregion
}
