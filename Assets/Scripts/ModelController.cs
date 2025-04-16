using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ModelController : NetworkBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    [SerializeField] private Material[] _material;


    private NetworkVariable<int> materialIndex = new NetworkVariable<int>(-1);


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Jika ini server, pilih material acak dan sync ke semua client
        if (IsServer)
        {
            materialIndex.Value = Random.Range(0, _material.Length);
        }
        
        // Update material ketika nilai berubah
        materialIndex.OnValueChanged += OnMaterialIndexChanged;
        
        // Apply material awal
        ApplyMaterial(materialIndex.Value);
    }

    private void OnMaterialIndexChanged(int oldValue, int newValue)
    {
        ApplyMaterial(newValue);
    }

    private void ApplyMaterial(int index)
    {
        if (index >= 0 && index < _material.Length)
        {
            _skinnedMeshRenderer.material = _material[index];
        }
    }
}
