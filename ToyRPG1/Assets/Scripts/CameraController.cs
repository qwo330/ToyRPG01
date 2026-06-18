using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Actor target;
    [SerializeField] Vector3 offset = new Vector3(0, 2f, -5f);
    [SerializeField] float sensitivity = 3f;
    
    float minVerticalAngle = -20f;
    float maxVerticalAngle = 60f;

    float cx = 0;
    float cy = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        var angles = transform.eulerAngles;
        cx = angles.y;
        cy = angles.x;
    }

    void LateUpdate()
    {
        if (target == null)
            return;
        
        cx += Input.GetAxisRaw("Mouse X") * sensitivity;
        cy -= Input.GetAxisRaw("Mouse Y") * sensitivity;
        
        cy = Mathf.Clamp(cy, minVerticalAngle, maxVerticalAngle);
        var rotation = Quaternion.Euler(cy, cx, 0);
        var pos = target.transform.position + (rotation * offset);
        
        transform.position = pos;
        transform.rotation = rotation;
    }

    public void SetTarget(Actor actor)
    {
        target = actor;
    }
}
