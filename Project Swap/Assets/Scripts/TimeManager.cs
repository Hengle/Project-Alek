using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static int moveSpeed = 45;
    public static bool slowTime;
    public static bool slowTimeCrit;

    private void Start()
    {
        moveSpeed = 45;
        slowTime = false;
        slowTimeCrit = false;
    }

    private void Update()
    {
        if (slowTime)
        {
            Time.timeScale = 0.05f;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            moveSpeed = 20;
        }
            
        else if (slowTimeCrit) {
            Time.timeScale = 0.50f;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
        }
            
        else
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            moveSpeed = 45;
        }
    }
}