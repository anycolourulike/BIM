using UnityEngine;
using UnityEngine.Events;

public class RocketLanding : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] UnityEvent onWin;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {            
            onWin.Invoke();
            Player.GetComponent<MeshRenderer>().enabled = false;
            Player.GetComponent<Movement>().enabled = false;            
        }
    }
}
