using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    [SerializeField] private Material[] _material;


    
    public void RandomizeMaterial()
    {
        int randomIndex = Random.Range(0, _material.Length);
        _skinnedMeshRenderer.material = _material[randomIndex];
    }
}
