using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public GameObject left, right;
    public Camera c;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = c.ScreenToWorldPoint(new Vector3(Input.mousePosition.x/Screen.width*c.pixelWidth,Input.mousePosition.y/Screen.height*c.pixelHeight,1F));
        left.transform.LookAt(position);
        right.transform.LookAt(position);
    }
}
