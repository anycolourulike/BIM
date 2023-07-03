using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.Events;

public class FadeMenuImage : MonoBehaviour
{
    // the image you want to fade, assign in inspector
    [SerializeField] TextMeshProUGUI img;
    [SerializeField] UnityEvent ambient;

    private void Start()
    {
        ambient.Invoke();
    }

    void Update()
    {        
        StartCoroutine(FadeImage(true));
    }

    IEnumerator FadeImage(bool fadeAway)
    {
        while (true)
        {
            // loop over 1 second backwards
            for (float i = 1f; i >= 0; i -= Time.deltaTime)
            {
                // set color with i as alpha
                img.color = new Color(1, 1, 1, i);
                yield return null;
            }
        }        
    }
}