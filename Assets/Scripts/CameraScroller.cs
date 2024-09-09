using UnityEngine;

public class CameraScroller : MonoBehaviour
{
    public float Sensitivity = 2f;

    private Vector2 mouseLook;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            var input = new Vector2(Input.GetTouch(0).deltaPosition.x, Input.GetTouch(0).deltaPosition.y);
            mouseLook += input * Sensitivity;
            transform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up);
        }
        else
            transform.localRotation = Quaternion.identity;
    }
}