using UnityEngine;
using TMPro;

public class HideUntilLight : MonoBehaviour
{
    TextMeshPro tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        if (tmp != null)
            tmp.enabled = RoomState.lightsOn;
    }
}