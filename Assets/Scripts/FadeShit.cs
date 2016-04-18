using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FadeShit : MonoBehaviour 
{
    public float FadeTime;

    public List<string> IgnoreNames = new List<string>();

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.E))
        {
            if(GetComponent<CanvasRenderer>().GetAlpha() > 0.0f)
            {
                FadeOut();
            }

            else
            {
                FadeIn();
            }
        }
	}

    public void FadeIn()
    {
        StartCoroutine(fadeIn());
    }

    public void FadeOut()
    {
        StartCoroutine(fadeOut());
    }

    IEnumerator fadeIn()
    {
        float t = 0.0f;

        float seconds = FadeTime;

        float start = GetComponent<CanvasRenderer>().GetAlpha();

        float end = 1.0f;

        while (t <= 1.0f)
        {
            t += Time.deltaTime / seconds;

            float alpha = Mathf.Lerp(start, end, t);

            CanvasRenderer[] renderers = GetComponentsInChildren<CanvasRenderer>();

            foreach(CanvasRenderer renderer in renderers)
            {
                if(IgnoreNames.Contains(renderer.gameObject.name) == false)
                {
                    renderer.SetAlpha(alpha);
                }
                
            }

            yield return new WaitForFixedUpdate();
        }

        yield return null;
    }

    IEnumerator fadeOut()
    {
        float t = 0.0f;

        float seconds = FadeTime;

        float start = GetComponent<CanvasRenderer>().GetAlpha();

        float end = 0f;

        while (t <= 1.0f)
        {
            t += Time.deltaTime / seconds;

            float alpha = Mathf.Lerp(start, end, t);

            CanvasRenderer[] renderers = GetComponentsInChildren<CanvasRenderer>();

            foreach (CanvasRenderer renderer in renderers)
            {
                if (IgnoreNames.Contains(renderer.gameObject.name) == false)
                {
                    renderer.SetAlpha(alpha);
                }
            }

            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }
}
