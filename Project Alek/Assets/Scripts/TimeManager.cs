using UnityEngine;

//TODO: Convert to instance
public class TimeManager : MonoBehaviour
{
    public static int _moveSpeed = 45;
    public static bool _slowTimeCounter;

    private void Start()
    {
        _moveSpeed = 45;
    }

    public static void PauseTime()
    {
        Time.timeScale = 0.0f;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    public static void ResumeTime()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    private void Update()
    {
        if (_slowTimeCounter)
        {
            Time.timeScale = 0.35f;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
        }

        else
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            _moveSpeed = 45;
        }
    }
}