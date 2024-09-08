using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private int cameraID;
    [SerializeField] private bool isStaticCamera;

    [SerializeField, ConditionalField(nameof(isStaticCamera))]
    private Vector2 cameraPos;

    [SerializeField, ConditionalField(nameof(isStaticCamera))]
    private float cameraSize;
    private static readonly int CAMERA_ID = Animator.StringToHash("CameraID");

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isStaticCamera)
        {
            GameManager.Shared().GetStaticCamera().gameObject.transform.position = (Vector3)cameraPos + Vector3.back * 10;
            GameManager.Shared().GetStaticCamera().m_Lens.OrthographicSize = cameraSize;

        }
        if (col.CompareTag(PLAYER_TAG))
        {
            cameraAnimator.SetInteger(CAMERA_ID, cameraID);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG))
        {
            cameraAnimator.SetInteger(CAMERA_ID, -1);
        }
    }
}
