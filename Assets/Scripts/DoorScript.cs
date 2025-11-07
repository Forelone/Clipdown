using UnityEngine;
using UnityEngine.Events;

public class DoorScript : MonoBehaviour
{
    [SerializeField] UnityEvent OnClose, OnOpen;
    bool Open;
    public void Toggle()
    {
        if (Open)
            OnClose.Invoke();
        else
            OnOpen.Invoke();

        Open = !Open;
    }
}
