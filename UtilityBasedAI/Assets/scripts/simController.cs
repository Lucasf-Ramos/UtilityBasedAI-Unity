using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class simController : MonoBehaviour
{
    public string houseName;
    [Space]
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
    int friendIndex;
    int Friendrandomness; //adiciona uma aleatoriedade para aonde eles v�o conversar

    Coroutine incNeedsRoutine = null;

    nav agent;
   
    void Start()
    {
        agent = GetComponent<nav>();
        StartCoroutine(decNeeds());
        StartCoroutine(sortPrioritys());
    }
    private void Update()
    {
        agent.target = actualObject != null ? actualObject.transform : inConversation && Friendrandomness==0 ? friendInConv.gameObject.transform : transform;
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
        List<Object> filter = gameController.controller.objects.Where(o => o.transform.parent.name == houseName && o.effector == actualneed && o.inUse == false ).ToList(); //da procura quais v�o de encontro com o que � ele quer suprir

        if (filter.Count > 1)
        {
            //tem mais de um objeto com essa fun��o
          
            filter.Sort((a, b) => b.points.CompareTo(a.points) + Vector2.Distance(b.gameObject.transform.position, transform.position).CompareTo(Vector2.Distance(a.gameObject.transform.position, transform.position)) / 2);

            if (filter[0] != null)
            {
                agent.target = filter[0].gameObject.transform;
                actualObject = filter[0];
                actualObject.inUse = true;
                StartIncNeeds();
            }
            else
            {
                actualneed = needs.none;
            }

        }
        else if (filter.Count == 1)
        {
            //tem somente 1

            if (filter[0] != null)
            {
                actualObject = filter[0];
                actualObject.inUse = true;
                agent.target = filter[0].gameObject.transform;
                StartIncNeeds();
            }
            else
            {
                actualneed = needs.none;
            }

        }
        else
        {
            actualneed = needs.none;
        }

    }



    IEnumerator decNeeds()
    {
        while (true)
        {
            //needs
            for (int i = 0; i < needsList.Count; i++)
            {
                if (needsList[i].value > -99 && needsList[i].need != actualneed)
                {
                    needsList[i].value -= 1 + needsList[i].decCurve.Evaluate(needsList[i].value);

                    if (needsList[i].value < -50 && needsList[actualneedIndex].value > 0 && needsList[i].value * needsList[i].decCurve.Evaluate(needsList[i].value) < needsList[actualneedIndex].value * needsList[actualneedIndex].decCurve.Evaluate(needsList[actualneedIndex].value))
                    {
                        actualneed = needs.none;

                        if (actualObject != null)
                        {
                            actualObject.inUse = false;
                            actualObject = null;

                        }

                        StopIncNeeds();
                        sortByUrgency();
                    }
                }
                else if(needsList[i].value < -99 && needsList[i].need != needs.none)
                {
                    sortByUrgency();
                    Debug.Log("aos " + Time.time + " " + gameObject.name + " chegou a 0 na necessidade " + needsList[i].need.ToString());
                }

            }

            //friendship
            for (int i = 0; i < friendsList.Count; i++)
            {
                if (!inConversation)
                {
                    friendsList[i].missingValue += friendsList[i].friendship_Level * 0.1f;


                    if (friendsList[i].missingValue > 100 && talkRequest(friendsList[i].friendData))
                    {
                        //Debug.Log(gameObject.name + " gostaria de conversar com " + friendsList[i].friendData.name);
                        inConversation = friendsList[i].friendData.talkRequest(this);
                        if (inConversation)
                        {
                            friendsList[i].missingValue = 0;
                            friendIndex = i;
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
            if (actualObject != null)
            {
                if (Vector2.Distance(actualObject.gameObject.transform.position, transform.position) < 0.5f)
                {
                    //Doing something
                    needsList[actualneedIndex].value += actualObject.points * 2f;
                    if (!agent.agent.isStopped)
                        agent.anim.Play(actualneed.ToString());
                    agent.agent.isStopped = true;

                    if (needsList[actualneedIndex].value >= 100)
                    {
                        needsList[actualneedIndex].value = 100;
                        StopIncNeeds();
                    }

                }
                else
                {
                    if (agent != null)
                    {
                        if (agent.anim != null)
                            agent.anim.Play("None");
                        agent.agent.isStopped = false;
                    }
                       
                }

            }
            if (inConversation && friendInConv != null)
            {
                Vector2 direction = (friendInConv.transform.position - transform.position).normalized;
                Debug.DrawRay(transform.position, direction, Color.green, 1);

                if (Vector2.Distance(friendInConv.gameObject.transform.position, transform.position) < 3f)
                {
                    //conversation Actions
                    needsList[actualneedIndex].value += friendsList[friendIndex].friendship_Level / 10f;
                    if (!agent.agent.isStopped)
                        agent.anim.Play("Social");
                    agent.agent.isStopped = true;
                    agent.agent.stoppingDistance = Vector2.Distance(friendInConv.gameObject.transform.position, transform.position);


                    agent.anim.SetFloat("x", direction.x);
                    agent.anim.SetFloat("y", direction.y);


                    if (needsList[actualneedIndex].value >= 100)
                    {
                        StopIncNeeds();
                    }

                }
                else
                {
                    if (agent != null)
                    {
                        if (agent.anim != null)
                            agent.anim.Play("None");
                        agent.agent.isStopped = false;
                    }
                }

            }
            yield return new WaitForSeconds(coroutineWaitTime);

        }


    }

    void StopIncNeeds()
    {
        if (incNeedsRoutine != null)
        {

            StopCoroutine(incNeedsRoutine);
            incNeedsRoutine = null;

            if (inConversation)
            {
                exitConversation();
                if (friendInConv != null)
                    friendInConv.exitConversation();
            }

           

            if (agent != null)
            {
                agent.agent.stoppingDistance = 0;
                agent.agent.isStopped = false;
                actualneed = needs.none;
                agent.anim.Play("None");
            }
         

            if(actualObject!= null)
            {
                actualObject.inUse = false;
                actualObject = null;
            }
           

           // Debug.Log("coroutine parada");
        }
       
    }

    void StartIncNeeds()
    {
        if(incNeedsRoutine == null)
        {
            StopIncNeeds(); // Para qualquer coroutine ativa antes de iniciar uma nova
            
            incNeedsRoutine = StartCoroutine(incNeeds());
           // Debug.Log("coroutine iniciada");
        }
      
    }

    public bool talkRequest(simController p)
    {
        Friend pProfile = friendsList.Where(o => o.friendData == p).First();
        currentNeeds n = needsList.Where(o => o.need == needs.Social).First();

        Friendrandomness = Random.Range(0, 2);
        pProfile.friendData.Friendrandomness = Random.Range(0, 2);

        if (pProfile.friendData.Friendrandomness + Friendrandomness == 2)
            return false;


        if (pProfile != null)
        {
            if (actualneed == needs.none)
            {

                //Debug.Log(transform.name + " quer conversar tambem");

                conversation(p);
                p.conversation(this);
                pProfile.missingValue = 0;
                friendIndex = friendsList.IndexOf(pProfile);
                return true;
            }
            else
            {
                if (n.value*n.urgency.Evaluate(n.value) > needsList[actualneedIndex].urgency.Evaluate(needsList[actualneedIndex].value))
                {
                    // Debug.Log(transform.name + " est� ocupado(a) mas pode conversar");

                    conversation(p);
                    p.conversation(this);
                    pProfile.missingValue = 0;
                    return true;
                }
                else
                {
                    // Debug.Log(transform.name + " est� ocupado(a), conversem mais tarde");
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


        StartIncNeeds();

    }
    public void exitConversation()
    {
        inConversation = false;
        friendInConv = null;
        StartCoroutine(sortPrioritys());

        actualneed = needs.none;
        StopIncNeeds();

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