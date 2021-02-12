using UnityEngine;

public class CameraViewportController : MonoBehaviour
{
    private static CameraViewportController instance;
    
    public static CameraViewportController Instance 
    {
        get { if (instance == null)
                Debug.LogError("CameraViewPortController is null");
            return instance; }
    }

    private void Awake() => instance = this;
    
    private void Start () => SetAspect();
    
    public void SetAspect()
    {
        const float targetAspect = 16.0f / 9.0f;
        
        var windowAspect = (float)Screen.width / (float)Screen.height;
        var scaleHeight = windowAspect / targetAspect;
        var cam = GetComponent<Camera>();

        // if scaled height is less than current height, add letterbox
        if (scaleHeight < 1.0f)
        {  
            var rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        
            cam.rect = rect;
        }
        else // add pillar box
        {
            var scaleWidth = 1.0f / scaleHeight;
            var rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
    }
}