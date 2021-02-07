
using Cinemachine;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vCam;
    [SerializeField] private float halfScreenWidth;
    [SerializeField] private float swipeMultiplier = 4f;
    [SerializeField] private GameObject hintBox;
    [SerializeField] private GameObject inputBox;
    [SerializeField] private Vector2 swipe;

    private int leftFingerId;
    private int rightFingerId;
    private Touch touch;

    public void DeleteHint()
    {
        GameObject.Destroy(hintBox);
        inputBox.GetComponentInChildren<Button>().interactable = true;
        inputBox.GetComponentInChildren<TMP_InputField>().interactable = true;
    }

    private void Awake()
    {
        
        leftFingerId = -1;
        rightFingerId = -1;

        halfScreenWidth = Screen.width / 2;
    }
    private void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            touch = Input.GetTouch(i);
            switch (touch.phase)
            {
                case TouchPhase.Began:

                    if (touch.position.x < halfScreenWidth && leftFingerId == -1)
                    {
                        leftFingerId = touch.fingerId;
                    }
                    else if (touch.position.x > halfScreenWidth && rightFingerId == -1)
                    {
                        rightFingerId = touch.fingerId;
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:

                    if (touch.fingerId == leftFingerId)
                    {
                        leftFingerId = -1;
                    }
                    else if (touch.fingerId == rightFingerId)
                    {
                        rightFingerId = -1;
                    }

                    break;
                case TouchPhase.Moved:

                    if (touch.fingerId == rightFingerId || touch.fingerId == leftFingerId)
                    {
                        swipe = Vector2.Lerp(Vector2.zero, touch.deltaPosition, swipeMultiplier * Time.deltaTime);
                    }

                    break;
                case TouchPhase.Stationary:

                    if (touch.fingerId == rightFingerId)
                    {
                        swipe = Vector2.zero;
                    }
                    break;
            }
        }
    }

    private void LateUpdate()
    {
        if (rightFingerId != -1 || leftFingerId != -1)
        {
            vCam.transform.RotateAround(transform.position, Vector3.up, swipe.x);
        }
    }
}
