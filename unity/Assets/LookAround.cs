using UnityEngine;

public class LookAround : MonoBehaviour
{
    public GameObject mainCam;
    public Transform pivot;
    public float speed;
    bool rotating;
    float rotated;

    void Update()
    {
        if (!rotating && Input.GetKeyDown(KeyCode.L))
        {
            StartRotation();
        }

        if (rotating)
        {
            Rotate();
        }
    }

    private void StartRotation()
    {
        rotating = true;
        rotated = 0;
        mainCam.SetActive(false);
    }

    private void Rotate()
    {
        rotated += speed * Time.deltaTime;
        var angles = pivot.eulerAngles;
        angles.y = rotated;
        if (rotated >= 360)
        {
            angles.y = 0;
            rotating = false;
            mainCam.SetActive(true);
        }

        pivot.eulerAngles = angles;
    }
}
