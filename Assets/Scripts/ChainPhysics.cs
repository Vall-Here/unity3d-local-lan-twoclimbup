using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

public class ChainPhysics : NetworkBehaviour
{
    public Rigidbody[] characters;  
    public float pullDistance = 5f; 
    public float pullStrength = 50f;
    public float maxDistance = 10f; 
    public float gravityStrength = 9.81f; 
    public float instantPullMultiplier = 2f; 

    private Vector3[] previousPositions; 

    void Start()
    {
        characters = new Rigidbody[2];
        previousPositions = new Vector3[characters.Length];
        if(characters[0] == null || characters[1] == null) return;
        for (int i = 0; i < characters.Length; i++)
        {
            previousPositions[i] = characters[i].position;
        }
    }

    void Update()
    {
        if (characters[0] == null || characters[1] == null) return;
        for (int i = 0; i < characters.Length - 1; i++)
        {
            
            float distance = Vector3.Distance(characters[i].position, characters[i + 1].position);
            if (distance > pullDistance){
                ApplyPullForce(i, distance, false);
            }
            if (distance > maxDistance)
            {
                ApplyMaxDistanceCorrection(i);
            }
            if (characters[i].position.y < -10f) 
            {
                ApplyGravityPull(i, true); 
            }

            previousPositions[i] = characters[i].position;
        }
    }

    void ApplyPullForce(int i, float distance, bool isFalling)
    {
        Vector3 direction = (characters[i + 1].position - characters[i].position).normalized;
        float forceMagnitude = (distance - pullDistance) * pullStrength;

        if (isFalling)
        {
            forceMagnitude *= instantPullMultiplier; 
        }

        characters[i].AddForce(direction * forceMagnitude);
        characters[i + 1].AddForce(-direction * forceMagnitude);
    }

    void ApplyMaxDistanceCorrection(int i)
    {
        Vector3 direction = (characters[i + 1].position - characters[i].position).normalized;
        characters[i].velocity = Vector3.zero;  
        characters[i + 1].velocity = Vector3.zero;
        
        float correctionForce = (Vector3.Distance(characters[i].position, characters[i + 1].position) - maxDistance) * pullStrength;
        characters[i].AddForce(-direction * correctionForce);
        characters[i + 1].AddForce(direction * correctionForce);
    }

    void ApplyGravityPull(int i, bool isFalling)
    {
        Vector3 fallDirection = (characters[i].position - characters[i + 1].position).normalized;
        characters[i].AddForce(fallDirection * gravityStrength);
        characters[i + 1].AddForce(-fallDirection * gravityStrength);

        if (isFalling)
        {
            ApplyPullForce(i, Vector3.Distance(characters[i].position, characters[i + 1].position), true);
        }
    }

    void PreventChainTangle()
    {
        for (int i = 0; i < characters.Length - 1; i++)
        {
            if (Vector3.Distance(characters[i].position, characters[i + 1].position) > maxDistance)
            {
                characters[i].velocity = Vector3.zero;
                characters[i + 1].velocity = Vector3.zero;
            }
        }
    }

    public void SetChainObject(NetworkObject player1, NetworkObject player2)
    {
        
        characters[0] = player1.GetComponent<Rigidbody>();
        characters[1] = player2.GetComponent<Rigidbody>();
    }
}
