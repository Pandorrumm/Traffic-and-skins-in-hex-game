using UnityEngine;
using InputSystem;
using System.Collections;
using Singleton;

[RequireComponent(typeof(Camera))]

public class CameraControl : MonoBehaviour {
    [Space]
    public float ScreenEdgeBorderThickness = 5.0f; // distance from screen edge. Used for mouse movement

    [Header("Camera Mode")]
    [Space]
    public bool RTSMode = true;
    public bool FlyCameraMode = false;

    [Header("Movement Speeds")]
    [Space]
    public float minPanSpeed;
    public float maxPanSpeed;
    public float secToMaxSpeed; //seconds taken to reach max speed;
    public float zoomSpeed;

    [Header("Movement Limits")]
    [Space]
    public bool enableMovementLimits;
    public Vector2 heightLimit;
    public Vector2 lenghtLimit;
    public Vector2 widthLimit;
    private Vector2 zoomLimit;

    private float panSpeed;
    private Vector3 initialPos;
    private Vector3 panMovement;
    private Vector3 pos;
    private Quaternion rot;
    private bool rotationActive = false;
    private Vector3 lastMousePosition;
    private Quaternion initialRot;
    private float panIncrease = 0.0f;

    private Vector3 direction;
    private Vector2 startPosition;
    private Coroutine touchCoroutine;
    private InputManager input;
    private GameMode gameMode;

    [Header("Rotation")]
    [Space]
    public bool rotationEnabled;
    public float rotateSpeed;

    private void Awake()
    {
        input = InputManager.Instance;
        gameMode = GameMode.Instance;

        input.OnStartPrimaryTouch += OnStartTouch;
        input.OnEndPrimaryTouch += OnEndTouch;
        input.OnStartZoomIn += OnZoomIn;
        input.OnStartZoomOut += OnZoomOut;
    }

    private void OnDestroy()
    {
        input.OnStartPrimaryTouch -= OnStartTouch;
        input.OnEndPrimaryTouch -= OnEndTouch;
        input.OnStartZoomIn -= OnZoomIn;
        input.OnStartZoomOut -= OnZoomOut;
    }

    private IEnumerator cr_TouchInProcess()
    {
        while (true) {
            if (input.isZooming == false && gameMode.currentMode == GameMode.Mode.CameraControl) {
                direction = input.PrimoryTouchScreenPosition() - startPosition;
                direction = direction.normalized;
            }

            yield return null;
        }
    }

    private void OnStartTouch(Vector2 position, float time)
    {
        touchCoroutine = StartCoroutine(cr_TouchInProcess());
        startPosition = position;
    }

    private void OnEndTouch(Vector2 position, float time)
    {
        StopCoroutine(touchCoroutine);
        direction = Vector2.zero;
    }

    // Use this for initialization
    private void Start() {
        initialPos = transform.position;
        initialRot = transform.rotation;
        zoomLimit.x = 15;
        zoomLimit.y = 65;
    }

    private bool isPlatformHasMouseAndKeyBoard() {
        return Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WebGLPlayer;
    }

    private bool isPlatformHasTouchScreen() {
        return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor;
    }

    private bool upButtonOrAxis() {
        return isPlatformHasTouchScreen() 
            && Mathf.Abs(direction.y) > Mathf.Abs(direction.x) 
            && direction.y > 0f;
    }

    private bool downButtonOrAxis() {
        return isPlatformHasTouchScreen()
            && Mathf.Abs(direction.y) > Mathf.Abs(direction.x)
            && direction.y < 0f;
    }

    private bool leftButtonOrAxis() {
        return isPlatformHasTouchScreen()
            && Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
            && direction.x < 0f;
    }

    private bool rightButtonOrAxis() {
        return isPlatformHasTouchScreen()
            && Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
            && direction.x > 0f;
    }

    private bool isIncreasePanSpeed() {
        return isPlatformHasTouchScreen() &&
                (direction.y > 0
                || direction.y < 0
                || direction.x > 0
                || direction.x < 0);
    }

    private void OnZoomIn()
    {
        if (gameMode.currentMode == GameMode.Mode.CameraControl) {
            // Stop camera movement
            direction = Vector3.zero;

            Camera.main.fieldOfView -= zoomSpeed;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, zoomLimit.x, zoomLimit.y);
        }
    }

    private void OnZoomOut()
    {
        if (gameMode.currentMode == GameMode.Mode.CameraControl) {
            // Stop camera movement
            direction = Vector3.zero;

            Camera.main.fieldOfView += zoomSpeed;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, zoomLimit.x, zoomLimit.y);
        }
    }

    Vector3 tmpDir;

    void Update() {
        if (enabled) {

            #region Camera Mode

            //check that ony one mode is choosen
            if (RTSMode == true) FlyCameraMode = false;
            if (FlyCameraMode == true) RTSMode = false;

            #endregion

            #region Movement

            panMovement = Vector3.zero;

            if (upButtonOrAxis() ||
                downButtonOrAxis() ||
                leftButtonOrAxis() ||
                rightButtonOrAxis()) {

                tmpDir = new Vector3(direction.x, 0, direction.y);

                panMovement += tmpDir * panSpeed * Time.deltaTime;
            }

            //if (upButtonOrAxis()) {
            //    panMovement += Vector3.forward * panSpeed * Time.deltaTime;
            //}
            //if (downButtonOrAxis()) {
            //    panMovement -= Vector3.forward * panSpeed * Time.deltaTime;
            //}
            //if (leftButtonOrAxis()) {
            //    panMovement += Vector3.left * panSpeed * Time.deltaTime;
            //}
            //if (rightButtonOrAxis()) {
            //    panMovement += Vector3.right * panSpeed * Time.deltaTime;
            //    //pos.x += panSpeed * Time.deltaTime;
            //}

            //if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - ScreenEdgeBorderThickness) {
            //    //panMovement += Vector3.forward * panSpeed * Time.deltaTime;
            //    panMovement += Vector3.right * panSpeed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= ScreenEdgeBorderThickness) {
            //    //panMovement -= Vector3.forward * panSpeed * Time.deltaTime;
            //    panMovement += Vector3.left * panSpeed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= ScreenEdgeBorderThickness) {
            //    //panMovement += Vector3.left * panSpeed * Time.deltaTime;
            //    panMovement -= Vector3.forward * panSpeed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - ScreenEdgeBorderThickness) {
            //    //panMovement += Vector3.right * panSpeed * Time.deltaTime;
            //    panMovement += Vector3.forward * panSpeed * Time.deltaTime;

            //    //pos.x += panSpeed * Time.deltaTime;
            //}

            //if (Input.GetKey(KeyCode.Q)) {
            //    panMovement += Vector3.up * panSpeed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.E)) {
            //    panMovement += Vector3.down * panSpeed * Time.deltaTime;
            //}

            if (RTSMode) transform.Translate(panMovement, Space.World);
            else if (FlyCameraMode) transform.Translate(panMovement, Space.Self);


            //increase pan speed
            if (isIncreasePanSpeed()) {
                panIncrease += Time.deltaTime / secToMaxSpeed;
                panSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, panIncrease);
            } else {
                panIncrease = 0;
                panSpeed = minPanSpeed;
            }

            #endregion

            #region Zoom

            //Camera.main.fieldOfView -= Input.mouseScrollDelta.y * zoomSpeed;
            //Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, zoomLimit.x, zoomLimit.y);

            #endregion

            #region mouse rotation

            //if (rotationEnabled) {
            //    // Mouse Rotation
            //    if (Input.GetMouseButton(0)) {
            //        rotationActive = true;
            //        Vector3 mouseDelta;
            //        if (lastMousePosition.x >= 0 &&
            //            lastMousePosition.y >= 0 &&
            //            lastMousePosition.x <= Screen.width &&
            //            lastMousePosition.y <= Screen.height)
            //            mouseDelta = Input.mousePosition - lastMousePosition;
            //        else {
            //            mouseDelta = Vector3.zero;
            //        }
            //        var rotation = Vector3.up * Time.deltaTime * rotateSpeed * mouseDelta.x;
            //        rotation += Vector3.left * Time.deltaTime * rotateSpeed * mouseDelta.y;

            //        transform.Rotate(rotation, Space.World);

            //        // Make sure z rotation stays locked
            //        rotation = transform.rotation.eulerAngles;
            //        rotation.z = 0;
            //        transform.rotation = Quaternion.Euler(rotation);
            //    }

            //    if (Input.GetMouseButtonUp(0)) {
            //        rotationActive = false;
            //        if (RTSMode) transform.rotation = Quaternion.Slerp(transform.rotation, initialRot, 0.5f * Time.time);
            //    }

            //    lastMousePosition = Input.mousePosition;

            //}


            #endregion


            #region boundaries

            if (enableMovementLimits == true) {
                //movement limits
                pos = transform.position;
                pos.y = Mathf.Clamp(pos.y, heightLimit.x, heightLimit.y);
                pos.z = Mathf.Clamp(pos.z, lenghtLimit.x, lenghtLimit.y);
                pos.x = Mathf.Clamp(pos.x, widthLimit.x, widthLimit.y);
                transform.position = pos;
            }

            #endregion
        }
    }

}