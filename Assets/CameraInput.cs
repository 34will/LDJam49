using UnityEngine;

public class CameraInput : MonoBehaviour
{
    public GameObject CameraOrbit;
    public float RotateSpeed = 8.0f;

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetMouseButton(0))
        {
            float h = RotateSpeed * Input.GetAxis("Mouse X");
            float v = RotateSpeed * Input.GetAxis("Mouse Y");

            if (CameraOrbit.transform.eulerAngles.z + v <= 0.1f || CameraOrbit.transform.eulerAngles.z + v >= 179.9f)
                v = 0;

            CameraOrbit.transform.eulerAngles = new Vector3(CameraOrbit.transform.eulerAngles.x, CameraOrbit.transform.eulerAngles.y + h, CameraOrbit.transform.eulerAngles.z + v);
        }

        float scrollFactor = Input.GetAxis("Mouse ScrollWheel");
        if (scrollFactor != 0.0f)
            CameraOrbit.transform.localScale = CameraOrbit.transform.localScale * (1.0f - scrollFactor);
    }
}
