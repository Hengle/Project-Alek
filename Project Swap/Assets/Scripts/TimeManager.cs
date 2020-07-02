using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static int _moveSpeed = 45;
    public static bool _slowTime;
    public static bool _slowTimeCrit;

    private void Start()
    {
        _moveSpeed = 45;
        _slowTime = false;
        _slowTimeCrit = false;
    }

    private void Update()
    {
        if (_slowTime)
        {
            Time.timeScale = 0.05f;
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
            _moveSpeed = 20;
        }
            
        else if (_slowTimeCrit) {
            Time.timeScale = 0.50f;
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