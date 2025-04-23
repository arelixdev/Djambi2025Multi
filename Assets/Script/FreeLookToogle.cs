using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FreeLookToogle : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public string horizontalAxis = "Mouse X";
    public string verticalAxis = "Mouse Y";
    public float sensitivityX = 300f;
    public float sensitivityY = 2f;

    private float xAxisValue = 0f;
    private float yAxisValue = 0.5f; // 0 = bottom rig, 1 = top rig

    void Update()
    {
        if (Input.GetMouseButton(2)) // Middle Mouse Button
        {
            xAxisValue += Input.GetAxis(horizontalAxis) * sensitivityX * Time.deltaTime;
            yAxisValue += Input.GetAxis(verticalAxis) * sensitivityY * Time.deltaTime;
            yAxisValue = Mathf.Clamp01(yAxisValue);
        }

        freeLookCamera.m_XAxis.Value = xAxisValue;
        freeLookCamera.m_YAxis.Value = yAxisValue;
    }
}
