using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Limb : MonoBehaviour
{
    public GameObject[] children;

    public Vector3 position;
    public Vector3 scale;
    public float rotation;
    public Vector3 jointLocation;
    public Vector3[] childJointOffsets;

    public Vector3[] limbVertexLocations;
    public int[] triangles;
    public Color colour;
    public Mesh mesh;
    public Material material;


    // This will run before Start
    private void Awake()
    {
        CreateLimb();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Calculate initial transform matrices
        TranslateJoint(-jointLocation);
        Scale(scale, jointLocation);
        Rotate(rotation, jointLocation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Create mesh and draw limb for the first time
    private void CreateLimb()
    {
        // Add a MeshFilter and MeshRenderer to the empty GameObject
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        // Get the Mesh from the MeshFilter
        mesh = GetComponent<MeshFilter>().mesh;

        // Set the material to the material we have selected
        GetComponent<MeshRenderer>().material = material;

        // Clear all vertex and index data from the mesh
        mesh.Clear();

        // Create collection of vertices 
        mesh.vertices = limbVertexLocations;

        // Apply colour to vertices
        mesh.colors = new Color[]
        {
            colour,
            colour,
            colour,
            colour
        };

        // Set vertex indices
        mesh.triangles = triangles;
    }

    // Initial translation to align joint to origin
    public void TranslateJoint(Vector3 offset)
    {
        // Get the translation matrix for the offset
        Matrix3x3 T = IGB283Transform.Translate(offset);

        // Apply transform to mesh
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = T.MultiplyPoint(vertices[i]);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        // Apply transform to object properties
        position = T.MultiplyPoint(position);
        jointLocation = T.MultiplyPoint(jointLocation);
        for (int i = 0; i < childJointOffsets.Length; i++) childJointOffsets[i] = T.MultiplyPoint(childJointOffsets[i]);
    }

    // Inital translation operations to translate children to associated child joint
    public void AssembleChildren()
    {
        for (int i = 0; i < children.Length; i++)
        {
            children[i].GetComponent<Limb>().Translate(childJointOffsets[i]);
            children[i].GetComponent<Limb>().AssembleChildren();
        }
    }

    // Performs a translation transform
    public void Translate(Vector3 offset)
    {
        // Get the translation matrix for the offset
        Matrix3x3 T = IGB283Transform.Translate(offset);

        // Apply transform to mesh
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = T.MultiplyPoint(vertices[i]);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        // Apply transform to object properties
        position = T.MultiplyPoint(position);
        jointLocation = T.MultiplyPoint(jointLocation);


        // Apply transform recursivly to children
        foreach (GameObject child in children)
        {
            child.GetComponent<Limb>().Translate(offset);
        }
    }

    // Performs a rotation transform about a point
    public void Rotate(float angle, Vector3 point)
    {
        // Get the translation matrix for the offset
        Matrix3x3 T1 = IGB283Transform.Translate(-point);
        Matrix3x3 R = IGB283Transform.Rotate(angle * Mathf.PI / 180);
        Matrix3x3 T2 = IGB283Transform.Translate(point);
        Matrix3x3 M = T2 * R * T1;

        // Apply transform to mesh
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = M.MultiplyPoint(vertices[i]);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        // Apply transform to object properties
        rotation += angle;
        position = M.MultiplyPoint(position);
        jointLocation = M.MultiplyPoint(jointLocation);
        for (int i = 0; i < childJointOffsets.Length; i++) childJointOffsets[i] = M.MultiplyPoint(childJointOffsets[i]);

        // Apply transform recursivly to children
        foreach (GameObject child in children)
        {
            child.GetComponent<Limb>().Rotate(angle, point);
        }
    }

    // Performs a scaling transform about a point
    // TODO: Works, but displays wrong scale info in editor when applying initial scale
    public void Scale(Vector3 factor, Vector3 point)
    {
        // Pre-scale point location for inverse translation
        Vector3 pointScaled = point;
        pointScaled.x *= factor.x;
        pointScaled.y *= factor.y;
        pointScaled.z *= factor.z;

        // Get the translation matrix for the offset
        Matrix3x3 T1 = IGB283Transform.Translate(-point);
        Matrix3x3 R1 = IGB283Transform.Rotate(-rotation * Mathf.PI / 180);
        Matrix3x3 S = IGB283Transform.Scale(factor);
        Matrix3x3 R2 = IGB283Transform.Rotate(rotation * Mathf.PI / 180);
        Matrix3x3 T2 = IGB283Transform.Translate(point);
        Matrix3x3 M = T2 * R2 * S * R1 * T1;

        // Apply transform to mesh
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = M.MultiplyPoint(vertices[i]);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        // Apply transform to object properties
        position = M.MultiplyPoint(position);
        scale.x *= factor.x;
        scale.y *= factor.y;
        scale.z *= factor.z;
        jointLocation = M.MultiplyPoint(jointLocation);
        for (int i = 0; i < childJointOffsets.Length; i++) childJointOffsets[i] = M.MultiplyPoint(childJointOffsets[i]);
        point = pointScaled;

        // Apply transform recursivly to children
        foreach (GameObject child in children)
        {
            child.GetComponent<Limb>().Scale(factor, point);
        }
    }

    // Performs a translation transform towards and ending at a specified point
    public void TranslateTowardsPoint(Vector3 targetPoint, float speed, out bool destinationReached)
    {
        // Calculate whether destination can be reached in this step
        Vector3 deltaPosition = targetPoint - position;
        if (deltaPosition.magnitude == 0)
        {
            destinationReached = true;
            return;
        }
        float maxStep = speed * Time.deltaTime;
        float distanceRatio = maxStep / deltaPosition.magnitude;
        if (distanceRatio < 0) distanceRatio = -distanceRatio; // Always step towards (+ve) destinatino

        if (distanceRatio < 1.0f) // Destination can't be reached in this step
        {
            // Scale delta position by percentage of travellable distance
            Translate(deltaPosition * distanceRatio);
            destinationReached = false;
            return;
        }
        // Else, Destination can be reached in this step, travel to destination
        Translate(deltaPosition);
        destinationReached = true;
    }

    // Performs a rotation transform towards and ending at a specified angle
    public void RotateTowardsAngle(float targetAngle, float rotationSpeed, out bool rotationReached)
    {
        // Calculate whether rotation can be reached in this step
        float deltaRotation = targetAngle - rotation;
        if (deltaRotation == 0)
        {
            rotationReached = true;
            return;
        }
        float maxStep = rotationSpeed * Time.deltaTime;
        float rotationRatio = maxStep / deltaRotation;
        if (rotationRatio < 0) rotationRatio = -rotationRatio; // Always step towards (+ve) target angle

        if (rotationRatio < 1.0f) // Angle can't be reached in this step
        {
            // Scale delta rotation by percentage of travellable rotation
            Rotate(deltaRotation * rotationRatio, jointLocation);
            rotationReached = false;
            return;
        }
        // Else, Angle can be reached in this step, rotation to angle
        Rotate(deltaRotation, jointLocation);
        rotationReached = true;
    }
}
