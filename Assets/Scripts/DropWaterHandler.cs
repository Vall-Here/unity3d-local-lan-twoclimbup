
using UnityEngine;

public class DropWaterHandler : MonoBehaviour
{
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other) {
        // print("Triggered");
        // if (other.gameObject.CompareTag("Player")) {
        //     other.gameObject.transform.position = spawnPoint.position;
        // }
    }
}
