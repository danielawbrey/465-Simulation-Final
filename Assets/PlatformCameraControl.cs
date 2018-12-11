using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCameraControl : MonoBehaviour
{
    public float centerX = 0f;
    public float centerZ = 0f;

    public float speed = 10.0f;

    protected float fDistance = 1;
    protected float fSpeed = 1;

    Vector3 targetPosition;

    private void OnEnable()
    {
        UIManager.BuildPlatformOnClicked += UIManager_BuildPlatformOnClicked;
        UIManager.OnUpdateComeraPosition += UIManager_BuildPlatformOnClicked;
    }

    private void UIManager_BuildPlatformOnClicked(PlatformConfigurationData pcd)
    {
        centerX = pcd.M / 2;
        centerZ = pcd.N / 2;

        transform.position = new Vector3(pcd.M, 6, pcd.N);
        targetPosition = new Vector3(centerX, 3, centerZ);
    }

    private void OnDisable()
    {
        UIManager.BuildPlatformOnClicked -= UIManager_BuildPlatformOnClicked;
        UIManager.OnUpdateComeraPosition -= UIManager_BuildPlatformOnClicked;
    }

    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3(6, 6, 6);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(targetPosition);

        #region ZOOM IN/OUT
        if (Input.GetKey(KeyCode.LeftShift))
            if(Input.GetKey(KeyCode.UpArrow))
                transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftShift))
            if (Input.GetKey(KeyCode.DownArrow))
                transform.Translate(-Vector3.forward * speed * Time.deltaTime);
        #endregion

        #region ROTATE VERTICAL/HORIZONTAL
        if (Input.GetKey(KeyCode.RightArrow))
            transform.RotateAround(targetPosition, -Vector3.up, Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.RotateAround(targetPosition, Vector3.up, Time.deltaTime * speed);


        if (Input.GetKey(KeyCode.UpArrow))
            transform.RotateAround(targetPosition, Vector3.right, Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.DownArrow))
            transform.RotateAround(targetPosition, -Vector3.right, Time.deltaTime * speed);
        #endregion
    }
}
