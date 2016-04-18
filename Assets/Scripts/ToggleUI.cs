using UnityEngine;
using System.Collections;

public class ToggleUI : MonoBehaviour 
{
    public GameObject MenuPanel;

	void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.E))
        {
            MenuPanel.SetActive(!MenuPanel.active);
        }
	}
}
