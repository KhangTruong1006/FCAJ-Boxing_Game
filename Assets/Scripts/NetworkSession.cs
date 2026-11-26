using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSession : MonoBehaviour
{
    [Header("Scene Name")]
    [SerializeField] private string mainScene = "Mainscreen";
    [SerializeField] private string matchScene = "Matching";

    [Header("Server Configuration")]
    [SerializeField] private string serverAddress = "127.0.0.1";
    [SerializeField] private ushort serverPort = 7777;

    [Header("Testing")]
    [SerializeField] private bool runAsServerInEditor = true;

    private NetworkManager network;
    private bool serverMode;

    private readonly Dictionary<ulong, int> reservedSlots = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        network = GetComponent<NetworkManager>();
    }

    private System.Collections.IEnumerator Start()
    {
    #if UNITY_SERVER
        serverMode = true;
    #elif UNITY_EDITOR
        serverMode = runAsServerInEditor;
    #else
        serverMode = false;
    #endif

        network.OnClientDisconnectCallback += handleDisconnect;
        network.OnClientStopped += handleClientStopped;

        UnityTransport transport = GetComponent<UnityTransport>();

        transport.UseWebSockets = true;

        transport.SetConnectionData(
            serverAddress,
            serverPort,
            "127.0.0.1"
        );

        if (serverMode)
        {
            Application.runInBackground = true;

            yield return SceneManager.LoadSceneAsync(matchScene);

            network.NetworkConfig.ConnectionApproval = true;
            network.ConnectionApprovalCallback += approveConnection;

            if (network.StartServer())
            {
                network.SceneManager.SetClientSynchronizationMode(
                    LoadSceneMode.Single
                );

                Debug.Log("Server started successfully.");
            }
        }
        else
        {
            SceneManager.LoadScene(mainScene);
        }
    }

    public void joinMatch()
    {
        if(network.IsListening || network.ShutdownInProgress)
        {
            return;
        }

        if (!network.StartClient())
        {
            Debug.LogError("Failed to start client");
        }
    }

    public void leaveMatch()
    {
        if(!serverMode || network.IsListening)
        {
            network.Shutdown();
        }
    }

    private void handleDisconnect(ulong clientId) 
    {
        if (serverMode) 
        { 
            reservedSlots.Remove(clientId);
            Debug.Log($"Client {clientId} disconnected.");
        }

        else
        {
            Debug.Log(network.DisconnectReason);
        }
    }

    private void handleClientStopped(bool wasHost)
    {
        if (!serverMode)
        {
            StartCoroutine(ReturnToMainSceneCoroutine());
        }
    }

    private System.Collections.IEnumerator ReturnToMainSceneCoroutine()
    {
        yield return null;

        while (network.ShutdownInProgress || network.IsListening)
        {
            yield return null;
        }

        SceneManager.LoadScene(mainScene);
    }

    private void approveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        GameManager gm = FindAnyObjectByType<GameManager>();
        bool canJoin = canPlayerJoin(gm);
        
        response.Approved = canJoin;
        response.CreatePlayerObject = canJoin;
        response.Pending = false;

        if (!canJoin)
        {
            response.Reason = "Match is full or unavailable.";
            return;
        }

        int slot = reservedSlots.ContainsValue(0) ? 1 : 0;

        reservedSlots[request.ClientNetworkId] = slot;

        Transform spawnPoint = gm.getSpawnPoint(slot);

        response.Position = spawnPoint.position;
        response.Rotation = spawnPoint.rotation;

        Debug.Log(
            $"Client {request.ClientNetworkId} assigned slot {slot}"
        );
    }

    public int getReservedSlot(ulong clientId)
    {
        return reservedSlots.TryGetValue(clientId, out int slot) ? slot : -1;
    }

    private bool canPlayerJoin(GameManager gm)
    {
        return gm != null && gm.IsSpawned && gm.isGracePeriod.Value && !gm.isGameEnded.Value && reservedSlots.Count < 2; 
    }
}
