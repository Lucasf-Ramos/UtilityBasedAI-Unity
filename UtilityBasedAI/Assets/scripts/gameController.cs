using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class gameController : MonoBehaviour
{
    public static gameController controller;
    public List<Object> objects = new List<Object>();

   
    public GameObject menu;
    public Slider[] needBar;
    simController selectedSim;

    void Awake()
    {
        controller = this;
        needBar = new Slider[menu.transform.childCount];
        for(int i = 0; i<needBar.Length; i++)
        {
            needBar[i] = menu.transform.GetChild(i).GetComponent<Slider>();
        }
    }
    public void showNeedList(simController s)
    {
        if (!menu.activeInHierarchy || s != selectedSim)
        {
            selectedSim = s;
            menu.SetActive(true);
            StartCoroutine(changeSlider());
        }
        else
        {
            
            menu.SetActive(false);
            StopCoroutine(changeSlider());
            selectedSim = null;
        }
       
    }

    IEnumerator changeSlider()
    {
        while (true)
        {

            for (int i = 0; i < needBar.Length; i++)
            {
                needBar[i].value = selectedSim.needsList.Where(o => o.need.ToString() == needBar[i].name).First().value;
            }
            yield return new WaitForSeconds(selectedSim.coroutineWaitTime);

        }

    }


}
