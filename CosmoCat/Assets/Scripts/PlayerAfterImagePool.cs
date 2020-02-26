using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImagePool : MonoBehaviour
{   
    [SerializeField]
    private GameObject afterImagePrefab; // store our gameObject.

    private Queue<GameObject> availableObjects = new Queue<GameObject>(); // store all object we made, witch are not curretly active.

    public static PlayerAfterImagePool Instance { get; private set; }

    private void Awake() 
        {
            Instance = this;
            GrowPool();
        }
    
    private void GrowPool()
    {
        for (int i = 0; i < 10; i++)
        {
            var instanceToAdd = Instantiate(afterImagePrefab); // var tells Unity to figure out the type data.
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);                       
        }
    }

    public void AddToPool(GameObject instance) 
    {
        instance.SetActive(false); 
        availableObjects.Enqueue(instance); // add gameObject to queue
    }

    public GameObject GetFromPool()
    {
        if(availableObjects.Count == 0) // if GameObject its not avaiable, make more.
        {
            GrowPool();
        }

        var instance = availableObjects.Dequeue();
        instance.SetActive(true);
        return instance;
    }
}
