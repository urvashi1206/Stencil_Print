using Obi;
using Obi.Samples;
using System.Collections;
using System.Collections.Generic;
//using System.Drawing;
using UnityEngine;
//using static UnityEngine.UI.Image;
using TMPro;

public class CameraSwitch : MonoBehaviour
{
    public List<Camera> cameraGameObjects;
    private List<Camera> cameras;
    private List<Vector3> cameraPositions;
    private List<Quaternion> cameraRotations;
    int index;

    public GameObject solverObject;
    public ObiSolver solver;
    //public ObiEmitter solderEmitter;

    public GameObject blade;
    private Vector3 bladeStartPosition;

    //All objects that will have collision turned off when fluid solidifies
    //public List<GameObject> stencilAndBlade;

    //camera settings
    public float turnSensitivity;
    public float turnSpeed;
    public float moveSpeed;

    //used for rotating cameras
    private List<Vector2> cameraRotationOffsets;
    private List<Quaternion> centerRotations;

    //maximum angle a camera can be turned
    public float maxAngleDeviation = 45f;
    //damping angle to slowly stop the camera
    public float dampingAngleBuffer = 10f;

    //Used for camera zooming
    public float zoomSpeed = 10f; // Speed of zooming
    public float minFOV = 15f; // Minimum field of view
    public float maxFOV = 90f; // Maximum field of view

    //Used for displaying the machine names 
    public UIHoverLabel hoverLabel1;
    public UIHoverLabel hoverLabel2;
    public Camera raycastCamera;


    //Used for Circuit Board Movement
    //public GameObject transferResults;
    //public float transferMinHeight;
    //public float transferMoveSpeed;

    // Store every particle’s original radius
    //private float[] originalRadii;

    private void Start()
    {
        index = 1;
        cameras = new List<Camera>();
        cameraPositions = new List<Vector3>();
        cameraRotations = new List<Quaternion>();

        centerRotations = new List<Quaternion>();
        cameraRotationOffsets = new List<Vector2>();

        for (int i = 0; i < cameraGameObjects.Count; i++)
        {
            cameras.Add(cameraGameObjects[i].GetComponent<Camera>());
            cameraPositions.Add(cameraGameObjects[i].transform.position);
            cameraRotations.Add(cameraGameObjects[i].transform.rotation);

            centerRotations.Add(cameraGameObjects[i].transform.rotation);
            cameraRotationOffsets.Add(Vector2.zero);
        }
        EnableCamera(index);
        //solverObject.GetComponent<SolidifyOnContact>().enabled = false;

        bladeStartPosition = blade.transform.position;


    }

    // Update is called once per frame
    void Update()
    {
        //navegate camera list using left and right arrows
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            index++;
            EnableCamera(index);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            index--;
            EnableCamera(index);
        }
        //if R is pressed, reset current camera
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCamera(index);
        }

        //if any number keys are pressed, open corresponding camera
        //for (int i = 0; i < cameras.Count; i++)
        //{
        //    KeyCode key = (KeyCode)(i + 48);
        //    if (Input.GetKeyDown(key))
        //    {
        //        index = i;
        //        EnableCamera(i);
        //    }
        //}

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if(scrollInput != 0) 
        {
            CameraZoom(scrollInput);
        }

        if (index == 0)
        {
            HandleFreeCam();
        }
        //if right click is down, allow camera rotation
        else if (Input.GetMouseButton(1))
        {
            HandleCameraRotation();
        }

        //MoveStencilResults();
        //ToggleSolidifyOnContact();
        //ResetSimulation();

        CheckForMachineHover();
    }

    //reset camera rotation and position
    private void ResetCamera(int index)
    {
        //if its the free cam
        if(index == 0)
            cameraGameObjects[index].transform.position = cameraPositions[index];
        cameraGameObjects[index].transform.rotation = cameraRotations[index];
        cameraRotationOffsets[index] = Vector2.zero;
    }

    //set target camera as the main camera
    void EnableCamera(int num)
    {
        if (index < 0)
            index = cameraGameObjects.Count - 1;
        else if (index >= cameraGameObjects.Count)
            index = 0;

        //loop through all other cameras and disable them
        for (int i = 0; i < cameraGameObjects.Count; i++)
        {
            cameraGameObjects[i].enabled = false;
        }
        //enable current camera
        cameraGameObjects[index].enabled = true;
    }

    //Rotate camera and Clamp camera movement
    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * turnSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * turnSensitivity;

        Vector2 offset = cameraRotationOffsets[index];

        //get mouse change
        Vector2 delta = new Vector2(-mouseY, mouseX);

        Vector2 predictedOffset = offset + delta;

        float predictedMagnitude = predictedOffset.magnitude;
        float dampingStart = maxAngleDeviation - dampingAngleBuffer;

        //get dot of the camera offset to the center
        Vector3 toPoint = (Vector2.zero - offset).normalized;
        float dot = Vector3.Dot(toPoint, delta);

        //if the camera is moving away from the center and the camera is moving in the damping zone, slow camera movement
        if (predictedMagnitude > dampingStart && dot < 0)
        {
            float t = Mathf.InverseLerp(dampingStart, maxAngleDeviation, predictedMagnitude);
            float dampingFactor = 1f - t;
            delta *= dampingFactor;
        }

        offset += delta;

        if (offset.magnitude > maxAngleDeviation)
            offset = offset.normalized * maxAngleDeviation;
        cameraRotationOffsets[index] = offset;

        Quaternion offsetRotation = Quaternion.Euler(offset.x, offset.y, 0f);

        cameraGameObjects[index].transform.rotation = centerRotations[index] * offsetRotation;
    }

    void HandleFreeCam()
    {
        // --- Rotation Handling ---
        // Get mouse input for rotation.
        float mouseX = Input.GetAxis("Mouse X") * turnSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * turnSensitivity;

        // Retrieve the current offset for the active camera.
        Vector2 offset = cameraRotationOffsets[index];

        // Update yaw (horizontal rotation) and pitch (vertical rotation)
        offset.y += mouseX;    // Yaw: add horizontal mouse movement.
        offset.x -= mouseY;    // Pitch: subtract vertical mouse movement (inverted input).

        // Clamp pitch to prevent flipping (rotation about X axis)
        offset.x = Mathf.Clamp(offset.x, -90f, 90f);

        // Update the stored offset.
        cameraRotationOffsets[index] = offset;

        // Apply final rotation (pitch, yaw, roll)
        cameraGameObjects[index].transform.localRotation = Quaternion.Euler(offset.x, offset.y, 0f);

        // --- Movement Handling ---
        // Get input axes for movement.
        float xInput = Input.GetAxis("Horizontal");  // Typically A/D or Left/Right arrows.
        float yInput = Input.GetAxis("Vertical");    // Typically W/S or Up/Down arrows.
        float zInput = Input.GetAxis("UpDown");        // For vertical movement (e.g., Q/E).

        // Use the active camera's transform for directional vectors.
        Transform camTransform = cameraGameObjects[index].transform;

        // Calculate movement direction relative to the camera's current orientation.
        Vector3 moveDirection = (camTransform.right * xInput)
                              + (camTransform.forward * yInput)
                              + (camTransform.up * zInput);

        // Apply movement to the camera.
        camTransform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void CameraZoom(float scrollInput)
    {
        cameras[index].fieldOfView -= scrollInput * zoomSpeed;
        cameras[index].fieldOfView = Mathf.Clamp(cameras[index].fieldOfView, minFOV, maxFOV);
    }

    void CheckForMachineHover()
    {
        if (raycastCamera == null)
            raycastCamera = cameras[index];

        Ray ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 10000000f))
        {
            if (hit.collider.CompareTag("ASM-ProcessLens"))
            {
                hoverLabel2.Show("ASM-ProcessLens Machine", Input.mousePosition);
            }
            else if (hit.collider.CompareTag("ASM_SMT_DEK_TQ_AND_TQM"))
            {
                hoverLabel1.Show("ASM_SMT_DEK_TQ_AND_TQM Machine", Input.mousePosition);
            }
            else
            {
                hoverLabel1.Hide();
                hoverLabel2.Hide();
            }
        }
        else
        {
            hoverLabel1.Hide();
            hoverLabel2.Hide();
        }
    }


    //private void MoveStencilResults()
    //{
    //    //if the down arrow is pressed, move the results down
    //    if (Input.GetKey(KeyCode.DownArrow))
    //    {  
    //        transferResults.transform.position -= new Vector3(0,transferMoveSpeed*Time.deltaTime, 0);
    //        if(transferResults.transform.position.y < transferMinHeight)
    //        {
    //            transferResults.transform.position = new Vector3(0, transferMinHeight, 0);
    //        }
    //    }
    //    if (Input.GetKey(KeyCode.UpArrow))
    //    {
    //        transferResults.transform.position += new Vector3(0, transferMoveSpeed * Time.deltaTime, 0);
    //        if (transferResults.transform.position.y > -1.5f)
    //        {
    //            transferResults.transform.position = new Vector3(0, -1.5f, 0);
    //        }
    //    }
    //}

    //private void ToggleSolidifyOnContact()
    //{
    //    if (Input.GetKeyDown(KeyCode.Semicolon))
    //    {
    //        originalRadii = new float[solver.allocParticleCount];
    //        for (int i = 0; i < solver.allocParticleCount; i++)
    //            originalRadii[i] = solver.principalRadii[i].x;
    //        //turn off collision for everything but the transfer plate
    //        for (int i = 0; i < stencilAndBlade.Count; i++)
    //        {
    //            stencilAndBlade[i].layer = LayerMask.NameToLayer("NoCollision");
    //            stencilAndBlade[i].GetComponent<ObiCollider>().enabled = false;
    //        }
    //        solverObject.GetComponent<SolidifyOnContact>().enabled = !solverObject.GetComponent<SolidifyOnContact>().enabled;

    //        solver.particleCollisionConstraintParameters.enabled = false;
    //        solver.PushSolverParameters();

    //        float shrinkFactor = 0.5f;
    //        for (int i = 0; i < solver.allocParticleCount-1; i++)
    //        {
    //            // invMass == 0 means this particle is “solid” in your setup
    //            if (solver.invMasses[i] == 0f)
    //            {
    //                float r = originalRadii[i] * shrinkFactor;
    //                solver.principalRadii[i] = new Vector4(r, r, r, 1f);
    //            }
    //        }
    //    }
    //}


    //private void ResetSimulation()
    //{
    //    if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
    //    {
    //        solderEmitter.KillAll();
    //        blade.transform.position = bladeStartPosition;
    //    }

    //}

}
