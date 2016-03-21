using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaSinLoop : MonoBehaviour {
    
    private Image image;

	void Start()
    {
        image = GetComponent<Image>();
        StartCoroutine(ImageColorLoop());
    }

    IEnumerator ImageColorLoop()
    {
        float x = 0;
        while (true)
        {
            float sin = 0.5f + (Mathf.Sin(x)/2);
            image.color = new Color(sin, 0, 0, 1);
            x += 0.1f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    void OnDestroy()
    {
        StopCoroutine(ImageColorLoop());
        image.color = Color.white;
    }
}
