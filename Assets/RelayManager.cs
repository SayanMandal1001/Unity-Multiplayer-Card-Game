using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    public Text CreatePlayerNameText;
    public Text JoinPlayerNameText;
    public Text JoinRoomCodeText;

    public GameManager gameManager;
    public GameFlowManager gameFlowManager;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            gameManager.playerAuthenticationID = AuthenticationService.Instance.PlayerId;

        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public void CreateRelayButton()
    {
        if (CreatePlayerNameText.text != "")
        {
            CreateRelay();
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);     

            Debug.Log(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort) allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            gameManager.RelayCode = joinCode;
            gameManager.SetPlayerName(CreatePlayerNameText.text);
            gameFlowManager.ToWaitingArea();

        }catch(RelayServiceException e) { 
            Debug.Log(e);
        }
    }

    public void JoinRelayButton()
    {
        if (JoinPlayerNameText.text != "" && JoinRoomCodeText.text != "")
        {
            JoinCode(JoinRoomCodeText.text);
        }
    }

    private async void JoinCode(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with code " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort) joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();

            gameManager.RelayCode = joinCode.ToUpper();
            gameManager.SetPlayerName(JoinPlayerNameText.text);
            gameFlowManager.ToWaitingArea();

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void LeaveRelay()
    {
        try
        {
            gameManager.OnClientLeaveRpc(gameManager.playerId);
            NetworkManager.Singleton.Shutdown();
        }catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
