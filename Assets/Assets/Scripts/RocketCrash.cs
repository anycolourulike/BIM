using UnityEngine;
using UnityEngine.Events;

public class RocketCrash : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] ParticleSystem RocketCrashFX;
    public UnityEvent OnCrash;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == Player)
        {           
            OnCrash.Invoke();
            Instantiate(RocketCrashFX, Player.transform.position, Quaternion.identity);
            Player.gameObject.SetActive(false);
            Player.GetComponent<Movement>().enabled = false;
        }    
    }
}
