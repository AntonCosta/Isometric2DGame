using UnityEngine;

public class VFXController : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Play()
    {
        gameObject.SetActive(true);
    }
    
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
