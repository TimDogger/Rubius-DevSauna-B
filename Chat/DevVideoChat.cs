using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevVideoChat : MonoBehaviour
{
    public static bool grab;
    public static Texture2D texture2D;
    public GameObject requestController;
    public float touchTimer;
    private float timer;
    private bool canTouch;
    private bool isTouching = false;

    private void OnPostRender()
    {
        if (grab)
        {
            Debug.Log("grab");
            //Create a new texture with the width and height of the screen
            texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
            texture2D.Apply();
            grab = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > touchTimer)
        {
            canTouch = true;
        }
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
                        isTouching = true;
                        break;
                    }
                case TouchPhase.Ended:
                    {
                        isTouching = false;
                        break;
                    }
            }
        }
        if (canTouch & !isTouching)
        {
            canTouch = false;
            timer = 0;
            foreach (Touch touch in Input.touches)
            {
                switch (touch.fingerId)
                {
                    // Если коснулись 1 пальцем
                    case 0:
                        {
                            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                            RaycastHit raycastHit;
                            if (Physics.Raycast(raycast, out raycastHit))
                            {
                                requestController.GetComponent<RequestExample>().TooglePanel(raycastHit.collider.gameObject);
                                Debug.Log(raycastHit.collider.gameObject.name);
                            }
                            break;
                        }                  
                }
            }
        }
    }
}