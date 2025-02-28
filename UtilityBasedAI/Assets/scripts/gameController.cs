using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : MonoBehaviour
{
    public static gameController controller;
    public List<Object> objects = new List<Object>();

    public GameObject menu;
    public GameObject needBar;

    void Awake()
    {
        controller = this;
    }

  
}
