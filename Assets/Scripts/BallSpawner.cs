using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class BallSpawner : MonoBehaviour
{
	[SerializeField] private List<ObjectToSpawn> objectsList = new List<ObjectToSpawn>();
	[SerializeField] private List<SpawnPointsType> spawnPointsType = new List<SpawnPointsType>();

    private System.Random random = new System.Random();

    private void Start() 
    {
        StartCoroutine(SpawnBall());
    }
	
    public IEnumerator SpawnBall()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);         
            yield return new WaitUntil (() => GameManager.Instance.gameState != GameState.Waiting);
            
            SpawnPointsType sp_Type = spawnPointsType[UnityEngine.Random.Range(0, spawnPointsType.Count - 1)];
            int quantity = UnityEngine.Random.Range(0, 3);
            float timeToSpawn = UnityEngine.Random.Range(1, 4);
            
            for (int i = -1; i < quantity; i++)
            {
                int index = UnityEngine.Random.Range(0, sp_Type.spawnPoints.Count);
                
                GameObject objectSpawned = GetObjectWithMaxProb();
                if(!objectSpawned.activeInHierarchy)
                {
                    objectSpawned.transform.position = sp_Type.spawnPoints[index].position;
                    objectSpawned.SetActive(true);
                }

                yield return new WaitForSeconds(timeToSpawn);
            }
        }
    }

    GameObject GetObjectWithMaxProb()
    {
        int totalWeight = objectsList.Sum(t => t.priority);
        int randomNumber = random.Next(0, totalWeight);

        GameObject gameObject = null;
        foreach (ObjectToSpawn item in objectsList)
        {
            if(randomNumber < item.priority){
                gameObject = item.obj;
                break;
            }
            randomNumber -= item.priority;
        }

        return gameObject;
    }
}

[Serializable]
public class ObjectToSpawn
{
    public GameObject obj;
    public int priority;
}

[Serializable]
public class SpawnPointsType
{
	public List<Transform> spawnPoints = new List<Transform>();
}
