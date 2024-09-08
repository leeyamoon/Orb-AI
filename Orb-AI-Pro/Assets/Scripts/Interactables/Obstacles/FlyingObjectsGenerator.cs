using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlyingObjectsGenerator : MonoBehaviour
{
    [SerializeField] private float startRespawnOffset;
    [SerializeField] private GameObject[] gameObjectsToCreate;
    [SerializeField] private float respawnSpeed;
    [SerializeField] private float endOfIterationWait;

    [Header("Relevate to the objects")] [SerializeField]
    private float objectLifeTime;
    [SerializeField] private Vector2 movingSpeed;
    [SerializeField] private Vector2 accelerationSpeed;
    [SerializeField, Min(0)] private float rotationRange;
    

    private Vector3 spawnPos;

    private void Awake()
    {
        spawnPos = transform.position;
    }

    private void Start()
    {
        StartCoroutine(ObjectsSpawner());
    }

    private IEnumerator ObjectsSpawner()
    {
        GameObject newObj;
        SelfDestroyerAfterTime newObjProperties;
        yield return new WaitForSeconds(startRespawnOffset);
        while (true)
        {
            foreach (var obj in gameObjectsToCreate)
            {
                newObj = Instantiate(obj, spawnPos, Quaternion.identity);
                newObjProperties = newObj.GetComponent<SelfDestroyerAfterTime>();
                newObjProperties.SetValues(objectLifeTime, movingSpeed, accelerationSpeed,
                    Random.Range(-rotationRange, rotationRange));
                yield return new WaitForSeconds(respawnSpeed);
            }
            yield return new WaitForSeconds(endOfIterationWait);
        }
    }
}
