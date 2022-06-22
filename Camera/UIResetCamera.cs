using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class UIResetCamera : MonoBehaviour
{
    public Camera cam;

    [SerializeField]
    private float moveDelay = 1f;

    private Button buttonReset;

    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private float fieldOfView;

    private void Awake()
    {
        buttonReset = GetComponent<Button>();

        cameraPosition = cam.transform.position;
        cameraRotation = cam.transform.rotation;
        fieldOfView = cam.fieldOfView;

        buttonReset.onClick.AddListener(AlignCamera);
    }

    private void OnDestroy()
    {
        buttonReset.onClick.RemoveListener(AlignCamera);
    }

    private void AlignCamera()
    {
        cam.transform.DOMove(cameraPosition, moveDelay);
        cam.transform.DORotateQuaternion(cameraRotation, moveDelay);

        float currentFieldOfView = cam.fieldOfView;

        DOTween.To(() => currentFieldOfView, x => currentFieldOfView = x, fieldOfView, moveDelay)
            .OnUpdate(() => {
                cam.fieldOfView = currentFieldOfView;
            });

        StartCoroutine(cr_WaitDelay());
    }

    private IEnumerator cr_WaitDelay()
    {
        buttonReset.interactable = false;
        yield return new WaitForSeconds(moveDelay);
        buttonReset.interactable = true;
    }
}
