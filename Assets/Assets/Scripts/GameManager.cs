using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public enum PlayerType
{
    None,
    X,
    O
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI turnText;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private TextMeshProUGUI waitingForOpponentText;
    [SerializeField] private Button returnToMenuButton;
    
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuScene = "Main Menu Scene";


    
    private PlayerType currentTurn = PlayerType.X;
    private PlayerType[,] boardState = new PlayerType[3, 3];
    private bool isGameOver = false;
    private int movesMade = 0;

    private int playersReadyToPlayAgain = 0;
    private bool hostIsX = true;

    private GridSquare[,] gridSquares = new GridSquare[3, 3];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        if (IsServer)
        {
            ResetGameState();
            UpdateTurnClientRpc(currentTurn, hostIsX);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        isGameOver = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (winnerText != null) winnerText.text = "Opponent Left!";
        
        if (playAgainButton != null) 
        {
            playAgainButton.gameObject.SetActive(true);
            playAgainButton.interactable = false;
        }

        if (waitingForOpponentText != null)
        {
            waitingForOpponentText.gameObject.SetActive(true);
            waitingForOpponentText.text = "Player Left";
        }
    }

    public void RegisterSquare(int x, int y, GridSquare square)
    {
        gridSquares[x, y] = square;
    }

    public void TryPlaceMark(int x, int y)
    {
        if (isGameOver) return;

        TryPlaceMarkServerRpc(x, y);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPlaceMarkServerRpc(int x, int y, ServerRpcParams serverRpcParams = default)
    {
        if (isGameOver) return;

        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        PlayerType requestingPlayer;
        if (hostIsX)
        {
            requestingPlayer = (senderClientId == NetworkManager.ServerClientId) ? PlayerType.X : PlayerType.O;
        }
        else
        {
            requestingPlayer = (senderClientId == NetworkManager.ServerClientId) ? PlayerType.O : PlayerType.X;
        }

        if (requestingPlayer != currentTurn)
        {
            Debug.LogWarning("It is not your turn!");
            return;
        }

        if (boardState[x, y] == PlayerType.None)
        {
            boardState[x, y] = currentTurn;
            movesMade++;

            UpdateBoardClientRpc(x, y, currentTurn);

            CheckWinCondition();

            if (!isGameOver)
            {
                currentTurn = (currentTurn == PlayerType.X) ? PlayerType.O : PlayerType.X;
                UpdateTurnClientRpc(currentTurn, hostIsX);
            }
        }
    }

    [ClientRpc]
    private void UpdateBoardClientRpc(int x, int y, PlayerType player)
    {
        if (gridSquares[x, y] != null)
        {
            gridSquares[x, y].UpdateVisual(player);
        }
    }

    [ClientRpc]
    private void UpdateTurnClientRpc(PlayerType newTurn, bool currentHostIsX)
    {
        currentTurn = newTurn;
        if (turnText != null)
        {
            PlayerType localPlayer = (IsServer == currentHostIsX) ? PlayerType.X : PlayerType.O;
            
            if (localPlayer == currentTurn)
            {
                turnText.text = $"<color=#00FF00>Your Turn ({currentTurn})</color>";
            }
            else
            {
                turnText.text = $"<color=#FF0000>Opponent's Turn ({currentTurn})</color>";
            }
        }
    }

    private void CheckWinCondition()
    {
        if (!IsServer) return;

        for (int i = 0; i < 3; i++)
        {
            if (boardState[i, 0] != PlayerType.None && boardState[i, 0] == boardState[i, 1] && boardState[i, 1] == boardState[i, 2])
            {
                EndGameClientRpc(boardState[i, 0], hostIsX);
                return;
            }
            
            if (boardState[0, i] != PlayerType.None && boardState[0, i] == boardState[1, i] && boardState[1, i] == boardState[2, i])
            {
                EndGameClientRpc(boardState[0, i], hostIsX);
                return;
            }
        }

        if (boardState[0, 0] != PlayerType.None && boardState[0, 0] == boardState[1, 1] && boardState[1, 1] == boardState[2, 2])
        {
            EndGameClientRpc(boardState[0, 0], hostIsX);
            return;
        }

        if (boardState[0, 2] != PlayerType.None && boardState[0, 2] == boardState[1, 1] && boardState[1, 1] == boardState[2, 0])
        {
            EndGameClientRpc(boardState[0, 2], hostIsX);
            return;
        }

        if (movesMade >= 9)
        {
            EndGameClientRpc(PlayerType.None, hostIsX);
        }
    }

    [ClientRpc]
    private void EndGameClientRpc(PlayerType winner, bool currentHostIsX)
    {
        isGameOver = true;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (playAgainButton != null) 
        {
            playAgainButton.gameObject.SetActive(true);
            playAgainButton.interactable = true;
        }
        if (waitingForOpponentText != null) waitingForOpponentText.gameObject.SetActive(false);

        if (winnerText != null)
        {
            if (winner == PlayerType.None)
            {
                winnerText.text = "Draw!";
            }
            else
            {
                PlayerType localPlayer = (IsServer == currentHostIsX) ? PlayerType.X : PlayerType.O;
                if (winner == localPlayer)
                {
                    winnerText.text = "You Win!";
                }
                else
                {
                    winnerText.text = "You Lose!";
                }
            }
        }
    }

    public void OnPlayAgainClicked()
    {
        if (playAgainButton != null) playAgainButton.gameObject.SetActive(false);
        if (waitingForOpponentText != null) 
        {
            waitingForOpponentText.gameObject.SetActive(true);
            waitingForOpponentText.text = "Waiting for opponent...";
        }

        PlayAgainServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayAgainServerRpc(ServerRpcParams rpcParams = default)
    {
        playersReadyToPlayAgain++;

        if (playersReadyToPlayAgain == 1)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            ulong otherId = senderId;
            foreach (var id in NetworkManager.ConnectedClientsIds)
            {
                if (id != senderId)
                {
                    otherId = id;
                    break;
                }
            }
            
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { otherId } }
            };
            
            ShowOpponentWantsToPlayClientRpc(clientRpcParams);
        }
        else if (playersReadyToPlayAgain == 2)
        {
            ResetGameServerRpc();
        }
    }

    [ClientRpc]
    private void ShowOpponentWantsToPlayClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (waitingForOpponentText != null)
        {
            waitingForOpponentText.gameObject.SetActive(true);
            waitingForOpponentText.text = "Opponent wants to play again!";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetGameServerRpc()
    {
        hostIsX = !hostIsX;
        ResetGameState();
        ResetGameClientRpc(hostIsX);
    }

    private void ResetGameState()
    {
        isGameOver = false;
        currentTurn = PlayerType.X;
        movesMade = 0;
        playersReadyToPlayAgain = 0;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                boardState[x, y] = PlayerType.None;
            }
        }
    }

    [ClientRpc]
    private void ResetGameClientRpc(bool currentHostIsX)
    {
        isGameOver = false;
        currentTurn = PlayerType.X;
        UpdateTurnClientRpc(currentTurn, currentHostIsX);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (gridSquares[x, y] != null)
                {
                    gridSquares[x, y].UpdateVisual(PlayerType.None);
                }
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public async void OnReturnToMenuClicked()
    {
        if (returnToMenuButton != null) returnToMenuButton.interactable = false;

        if (MultiplayerManager.Instance != null)
        {
            await MultiplayerManager.Instance.LeaveSession();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
    }
}
