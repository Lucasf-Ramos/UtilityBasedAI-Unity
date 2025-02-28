using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class Object : MonoBehaviour
{
    public bool inUse;
    public needs effector;
    public int points = 10;


    void Start()
    {
        gameController.controller.objects.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
