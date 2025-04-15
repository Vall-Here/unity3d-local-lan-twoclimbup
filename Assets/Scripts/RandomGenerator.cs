using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class RandomScript : MonoBehaviour
{


	[SerializeField] TextMeshProUGUI ipAddressText;
	[SerializeField] TMP_InputField ip;

	[SerializeField] string ipAddress;
	[SerializeField] UnityTransport transport;

	void Start()
	{
		ipAddress = "0.0.0.0";
		SetIpAddress(); 
		InvokeRepeating("assignPlayerController", 0.1f, 0.1f);
	}
	public void StartHost() {
		NetworkManager.Singleton.StartHost();
		GetLocalIPAddress();
	}


	public void StartClient() {
		ipAddress = ip.text;
		SetIpAddress();
		NetworkManager.Singleton.StartClient();
	}


	public string GetLocalIPAddress() {
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
				ipAddressText.text = ip.ToString();
				ipAddress = ip.ToString();
				return ip.ToString();
			}
		}
		throw new System.Exception("No network adapters with an IPv4 address in the system!");
	}


	public void SetIpAddress() {
		transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		transport.ConnectionData.Address = ipAddress;
	}

}