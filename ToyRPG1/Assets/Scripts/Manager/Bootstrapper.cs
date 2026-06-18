using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    void Awake()
    {
        _ = GameManager.Instance;
        _ = ActorManager.Instance;
    }
}
