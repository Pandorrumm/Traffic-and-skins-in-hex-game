/*

Set this on an empty game object positioned at (0,0,0) and attach your active camera.
The script only runs on mobile devices or the remote app.

*/

using UnityEngine;
using UnityEngine.EventSystems;
using Singleton;
using System.Collections;
using DG.Tweening;

class ScrollAndPinch : MonoBehaviour
{
#if UNITY_IOS || UNITY_ANDROID
    public Camera Camera;
    public bool Rotate;

    public float minZoom = 1f;
    public float maxZoom = 20f;

    private Vector3 cameraStartPosition;

    protected Plane Plane;


    private void Awake()
    {
        if (Camera == null)
            Camera = Camera.main;

        cameraStartPosition = Camera.transform.position;

        GameMode.OnChangeMode += OnModeChange;
    }

    private void OnModeChange(GameMode.Mode mode)
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(cr_Wait(mode));
        }      
    }

    private IEnumerator cr_Wait(GameMode.Mode mode)
    {
        yield return new WaitForSeconds(0.25f);

        switch (mode) {
            case GameMode.Mode.CameraControl:
                enabled = true;
                break;
            default:
                enabled = false;
                break;
        }

    }

    private void OnDestroy()
    {
        GameMode.OnChangeMode -= OnModeChange;
    }

    private void Update()
    {
        if (enabled) {
            if (GameMode.Instance.currentMode == GameMode.Mode.DrugAndDropStart ||
                GameMode.Instance.currentMode == GameMode.Mode.DrugAndDropRelesed) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;

            //Update Plane
            if (UnityEngine.Input.touchCount >= 1)
                Plane.SetNormalAndPosition(transform.up, transform.position);

            var Delta1 = Vector3.zero;
            var Delta2 = Vector3.zero;


            //Scroll
            if (UnityEngine.Input.touchCount >= 1) {
                Delta1 = PlanePositionDelta(UnityEngine.Input.GetTouch(0));
                if (UnityEngine.Input.GetTouch(0).phase == TouchPhase.Moved)
                    Camera.transform.Translate(Delta1, Space.World);
            }

            //Pinch
            if (UnityEngine.Input.touchCount >= 2)
            {
                var pos1 = PlanePosition(UnityEngine.Input.GetTouch(0).position);
                var pos2 = PlanePosition(UnityEngine.Input.GetTouch(1).position);
                var pos1b = PlanePosition(UnityEngine.Input.GetTouch(0).position - UnityEngine.Input.GetTouch(0).deltaPosition);
                var pos2b = PlanePosition(UnityEngine.Input.GetTouch(1).position - UnityEngine.Input.GetTouch(1).deltaPosition);

                //calc zoom
                var zoom = Vector3.Distance(pos1, pos2) /
                            Vector3.Distance(pos1b, pos2b);

                //zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

                //edge case
                if (zoom == 0 || zoom > 10)
                    return;

                //Move cam amount the mid ray

                Vector3 camPositionBeforeAdjustment = Camera.transform.position;
                Camera.transform.position = Vector3.LerpUnclamped(pos1, Camera.transform.position, 1 / zoom);


                //Upper (ZoomOut)
                if (Camera.transform.position.y > (cameraStartPosition.y + maxZoom))
                {
                    Camera.transform.position = camPositionBeforeAdjustment;
                }
                //Lower (Zoom in)
                if (Camera.transform.position.y < (cameraStartPosition.y - minZoom) || Camera.transform.position.y <= 1)
                {
                    Camera.transform.position = camPositionBeforeAdjustment;
                }

                if (Rotate && pos2b != pos2)
                {
                    Camera.transform.RotateAround(pos1, Plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, Plane.normal));
                }
            }
        }
    }

    protected Vector3 PlanePositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;

        //delta
        var rayBefore = Camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = Camera.ScreenPointToRay(touch.position);
        if (Plane.Raycast(rayBefore, out var enterBefore) && Plane.Raycast(rayNow, out var enterNow))
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);

        //not on plane
        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = Camera.ScreenPointToRay(screenPos);
        if (Plane.Raycast(rayNow, out var enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }
#endif
}