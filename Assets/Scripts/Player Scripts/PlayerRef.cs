using UnityEngine;

public class PlayerRef : MonoBehaviour
{
    public static PlayerRef Instance { get; private set; }
    public static Transform Transform => Instance?.transform;
    public static IHasHealth Health => Instance?.GetComponent<IHasHealth>();

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }
}