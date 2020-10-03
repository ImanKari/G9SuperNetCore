using System.Collections;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GameCharacterHandler : MonoBehaviour
{
    private bool _enableMove;

    public CanvasFormIpAndPort AccessToCanvasFormIpAndPort;

    public GameObject AccessToCanvasLoading;

    public GameAccount AccessToGameAccount;

    public G9SuperNetCoreClient4UnityInitializeGameClient Client;

    private NavMeshAgent m_Agent;

    public AudioSource RunAudioSource;

    public TextMeshProUGUI PlayerName;

    private bool _setPlayerName;

    // Start is called before the first frame update
    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_setPlayerName)
            if (AccessToGameAccount.PlayerIdentity != 0)
            {
                PlayerName.text = $"Player {AccessToGameAccount.PlayerIdentity}";
                _setPlayerName = true;
            }

        if (_enableMove && Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out var m_HitInfo))
            {
                MoveCharacter(m_HitInfo.point);
                AccessToGameAccount.LastPlayerPosition = new SimpleVector3
                {
                    X = m_HitInfo.point.x,
                    Y = m_HitInfo.point.y,
                    Z = m_HitInfo.point.z
                };
                AccessToGameAccount.Session.SendCommandAsync<CPlayerMove, G9DtPlayerMove>(
                    new G9DtPlayerMove
                    {
                        PlayerIdentity = AccessToGameAccount.PlayerIdentity,
                        NewPosition = AccessToGameAccount.LastPlayerPosition
                    }
                );
                if (!RunAudioSource.isPlaying)
                    RunAudioSource.Play();
            }
        }
    }

    public void MoveCharacter(Vector3 position)
    {
        m_Agent.SetDestination(position);
        //if (!RunAudioSource?.isPlaying ?? false)
        //    RunAudioSource.Play();
    }

    public void StartConnection()
    {
        var ipAddress = AccessToCanvasFormIpAndPort.IpAddress.text;
        var port = int.Parse(AccessToCanvasFormIpAndPort.Port.text);
        AccessToCanvasFormIpAndPort.gameObject.SetActive(false);
        AccessToCanvasLoading.SetActive(true);
        Client.StartConnection(ipAddress, port).Start();
    }

    public void OnConnected()
    {
        AccessToCanvasLoading.SetActive(false);
        _enableMove = true;
        Invoke(nameof(DoSomething), 1);
    }

    private void DoSomething()
    {
        AccessToGameAccount.Session.SendCommandAsync<CPlayerConnected, bool>(true);
    }
}