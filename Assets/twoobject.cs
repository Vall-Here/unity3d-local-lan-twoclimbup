using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class twoobject : MonoBehaviour
{
    [SerializeField] Transform objec1;
    [SerializeField] Transform objec2;


    private ObiSpawner obiSpawner;

    void Awake()
    {
        obiSpawner = GetComponent<ObiSpawner>();
    }

    void Start()
    {
        obiSpawner.SpawnObiRope(objec1, objec2);
    }
}
