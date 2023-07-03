using UnityEngine;
using UnityEngine.Events;

public class EndGame : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] UnityEvent onWin;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            onWin.Invoke();            
            Player.GetComponent<BiffoMover>().enabled = false;

            DialogUI.Instance
             .SetTitle("Most")
             .SetMessage("amazing!")
             .OnClose(LevelManager.loadMenu)
             .Show();
        }
    }
}