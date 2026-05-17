using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance);
        }
        else
            _instance = this;
    }

    private void Start()
    {
        AudioManager.Ins.PlayBackgroundMusic(1); // Phát nhạc nền chung cho toàn game
    }
}
