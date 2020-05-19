using UnityEngine;

public class headMouseLook : MonoBehaviour
{
    // Represent mouse position, modified by sensitivity.
    private float mouseX, mouseY;

    // Represent the current rotation along the y and x axes in degrees.
    private float horizontalLook, verticalLook;

    // Mouse sensitivity: 100 = 100%.
    [SerializeField]
    private float sensitivity = 100f;

    public Transform horizontal;
    public Transform vertical;

    public bikeBrain bb;

    private void Awake()
    {
        horizontalLook = horizontal.eulerAngles.y;
        verticalLook = vertical.eulerAngles.x;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        horizontalLook += mouseX;
        if (horizontalLook > 360)
            horizontalLook -= 360;
        else if (horizontalLook < -360)
            horizontalLook += 360;
        horizontal.localRotation = Quaternion.Euler(horizontalLook * Vector3.up);

        verticalLook -= mouseY;
        if (verticalLook > 90)
            verticalLook = 90;
        else if (verticalLook < -90)
            verticalLook = -90;
        vertical.localRotation = Quaternion.Euler(verticalLook * Vector3.right);
    }
}
