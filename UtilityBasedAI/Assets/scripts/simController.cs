using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class simController : MonoBehaviour
{

    public List<currentNeeds> needsList = new List<currentNeeds>();
    public List<Friend> friendsList = new List<Friend>();

    public float waitTimeToSort = 5f;
    public float coroutineWaitTime = 1f;

    public needs actualneed = needs.none; //necessidade que ira ser suprida agora
    int actualneedIndex = 0;
    Object actualObject;
    bool sort;

    public bool inConversation;
    public simController friendInConv;

    nav agent;
    void Start()
    {
        agent = GetComponent<nav>();
        StartCoroutine(decNeeds());
        StartCoroutine(sortPrioritys());
    }
    private void Update()
    {
        agent.target = actualObject != null ? actualObject.transform : inConversation?friendInConv.gameObject.transform: transform;
    }
    IEnumerator sortPrioritys()
    {
        while (true)
        {
            if (actualneed == needs.none)
                sortByUrgency();


            yield return new WaitForSeconds(waitTimeToSort);
        }

    }
    public void sortByUrgency()
    {
        needsList.Sort((a, b) => (a.value * a.urgency.Evaluate(a.value)).CompareTo((b.value * b.urgency.Evaluate(b.value))));

        actualneedIndex = Random.Range(0, 4);
        actualneed = needsList[actualneedIndex].need;

        findObjectforNeed();
    }
    public void findObjectforNeed()
    {
        List<Object> filter = gameController.controller.objects.Where(o => o.effector == actualneed && o.inUse==false).ToList(); //da procura quais vão de encontro com o que é ele quer suprir

        if (filter.Count > 1)
        {
            //tem mais de um objeto com essa função
            filter.Sort((a, b) => b.points.CompareTo(a.points) + Vector2.Distance(b.gameObject.transform.position, transform.position).CompareTo(Vector2.Distance(a.gameObject.transform.position, transform.position))/2);
           

            if (filter[0] != null)
            {
                agent.target = filter[0].gameObject.transform;
                actualObject = filter[0];
                actualObject.inUse = true;
                StartCoroutine(incNeeds());
            }
            else
            {
                actualneed = needs.none;
            }
         

           
        }
        else if(filter.Count == 1)
        {
            //tem somente 1
            

            if (filter[0] != null)
            {
                actualObject = filter[0];
                actualObject.inUse = true;
                agent.target = filter[0].gameObject.transform;
                StartCoroutine(incNeeds());
            }
            else
            {
                actualneed = needs.none;
            }
            
        }
        else
        {
            //não existe nada para suprir isso

            // Debug.Log($"{gameObject.name}: puxa vida, não tem nada para eu suprir minha {actualneed}");
          
            actualneed = needs.none;
       
        }
        
    }

   

    IEnumerator decNeeds()
    {
        while (true)
        {
            
           

            for(int i = 0; i < needsList.Count; i++)
            {
                if (needsList[i].value > -99 && needsList[i].need != actualneed)
                {
                    needsList[i].value -= 1+ needsList[i].decCurve.Evaluate(needsList[i].value);
                   // needsList[i].bar.value = needsList[i].value;
                   


                    if(needsList[i].value < -50 && needsList[actualneedIndex].value>0 && needsList[i].value * needsList[i].decCurve.Evaluate(needsList[i].value) < needsList[actualneedIndex].value * needsList[actualneedIndex].decCurve.Evaluate(needsList[actualneedIndex].value))
                    {
                        actualneed = needs.none;

                        if (actualObject != null)
                        {
                            actualObject.inUse = false;
                            actualObject = null;

                        }
                       
                        StopCoroutine(incNeeds());
                        sortByUrgency();
                    }
                }
                
            }
            for (int i = 0; i < friendsList.Count; i++)
            {
                if (!inConversation){
                    friendsList[i].missingValue += friendsList[i].friendship_Level * 0.1f;


                    if (friendsList[i].missingValue > 100)
                    {
                        Debug.Log(gameObject.name + " gostaria de conversar com " + friendsList[i].friendData.name);
                        inConversation = friendsList[i].friendData.talkRequest(this);
                        if (inConversation)
                        {
                            friendsList[i].missingValue = 0;
                        }
                    }
                }
               
            }
            yield return new WaitForSeconds(coroutineWaitTime);
         
        }
   
    }

    IEnumerator incNeeds()
    {
        while (true)
        {
          if(actualObject != null)
          {
                if (Vector2.Distance(actualObject.gameObject.transform.position, transform.position) < 0.5f)
                {
                    needsList[actualneedIndex].value += actualObject.points * 0.5f;
                    agent.agent.isStopped = true;
                    // needsList[actualneedIndex].bar.value = needsList[actualneedIndex].value;
                }
                else
                {
                    agent.agent.stoppingDistance = 0;
                    agent.agent.isStopped = false;
                }

                if (needsList[actualneedIndex].value >= 100)
                {
                    if (inConversation)
                    {
                        inConversation = false;
                        friendInConv.inConversation = false;
                        friendInConv = null;
                        StartCoroutine(sortPrioritys());
                    }

                    actualneed = needs.none;
                    actualObject.inUse = false;
                    actualObject = null;
                    StopCoroutine(incNeeds());
                }

             

                
          }
          if (inConversation && friendInConv != null)
          {


                Vector2 direction = (friendInConv.transform.position - transform.position).normalized;
                Debug.DrawRay(transform.position, direction, Color.green, 1);

                if (Vector2.Distance(friendInConv.gameObject.transform.position, transform.position) < 3f)
                {
                    
                   
                   
                        needsList[actualneedIndex].value += 1;
                        agent.agent.isStopped = true;
                        agent.agent.stoppingDistance = Vector2.Distance(friendInConv.gameObject.transform.position, transform.position);


                        agent.anim.SetFloat("x", direction.x);
                        agent.anim.SetFloat("y", direction.y);
                   

                   
                   
                }
                else
                {
                    agent.agent.stoppingDistance = 0;
                    agent.agent.isStopped = false;
                }
                
                

                if (needsList[actualneedIndex].value >= 100)
                {
                    exitConversation();
                    if(friendInConv != null)
                        friendInConv.exitConversation();


                }


            }
            yield return new WaitForSeconds(coroutineWaitTime);
          
        }
       
       
    }

    public bool talkRequest(simController p)
    {
        Friend pProfile = friendsList.Where(o => o.friendData == p).First();

        if (pProfile != null)
        {
            if (actualneed == needs.none)
            {

                //Debug.Log(transform.name + " quer conversar tambem");
                
                conversation(p);
                p.conversation(this);
                pProfile.missingValue = 0;

                return true;
            }
            else
            {
                if(pProfile.friendship_Level/100 > needsList[actualneedIndex].urgency.Evaluate(needsList[actualneedIndex].value))
                {
                   // Debug.Log(transform.name + " está ocupado(a) mas pode conversar");
                  
                    conversation(p);
                    p.conversation(this);
                    pProfile.missingValue = 0;
                    return true;
                }
                else
                {
                   // Debug.Log(transform.name + " está ocupado(a), conversem mais tarde");
                    return false;
                }
            }
        }
        return false;

    }

    public void conversation(simController p)
    {
        StopCoroutine(sortPrioritys());
      

        friendInConv = p;
        inConversation = true; 
     

        if (needsList[actualneedIndex].need != needs.Social)
        {
            actualneedIndex = needsList.IndexOf(needsList.Where(o => o.need == needs.Social).First());
            actualneed = needs.Social;
            
        }
       
        agent.target = friendInConv.gameObject.transform;
      

        if (actualObject != null)
            actualObject.inUse = false;
        actualObject = null;
     

        StartCoroutine(incNeeds());
       
    }
    public void exitConversation()
    {
        inConversation = false;
        friendInConv = null;
        StartCoroutine(sortPrioritys());

        actualneed = needs.none;
        StopCoroutine(incNeeds());
    }

}

[System.Serializable]
public class currentNeeds
{
    public needs need;
    public AnimationCurve urgency; //curva multiplicadora da urgencia dessa necessidade
    [Space]
    public AnimationCurve decCurve; //curva para o decaimento de necessidades
    //public AnimationCurve incCurve; //curva para o incremento da necessidade
    [Space]
    [Range(-100, 100)]
    public float value = 100; // decaimento da necessidade
    
   
}

[System.Serializable]
public class Friend
{
    public simController friendData;

    [Range(0, 100)]
    public int friendship_Level = 0;
    public float missingValue = 0;
}