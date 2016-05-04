using UnityEngine;
using System.Collections;

public class ToggleUI : MonoBehaviour 
{
    public GameObject MenuPanel;

    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController FPSController;
	void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.E))
        {
            MenuPanel.SetActive(!MenuPanel.active);

            FPSController.UIActive = MenuPanel.active;

            // spawn panel in front of player
            Vector3 newPosition = FPSController.transform.position + (FPSController.Cam.transform.forward * 2.0f);

            transform.position = newPosition;

            // look at player

            transform.LookAt(FPSController.transform);

            // rotate 180 y

            transform.Rotate(0.0f, 180.0f, 0.0f);

            
            
        }
	}
}
