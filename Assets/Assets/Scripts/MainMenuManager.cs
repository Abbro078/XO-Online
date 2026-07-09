using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject lobbyPanel;

    [Header("Main Menu Elements")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinByCodeButton;

    [Header("Lobby Elements")]
    [SerializeField] private TextMeshProUGUI joinCodeTextDisplay;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;

    [Header("Scene Management")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        ShowPanel(mainMenuPanel);
        InitializeButtons();
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.OnSessionEstablished += HandleSessionEstablished;
            MultiplayerManager.Instance.OnSessionEnded += HandleSessionEnded;
            MultiplayerManager.Instance.OnError += HandleError;
        }
    }

    private void OnDestroy()
    {
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.OnSessionEstablished -= HandleSessionEstablished;
            MultiplayerManager.Instance.OnSessionEnded -= HandleSessionEnded;
            MultiplayerManager.Instance.OnError -= HandleError;

            if (MultiplayerManager.Instance.CurrentSession != null)
            {
                MultiplayerManager.Instance.CurrentSession.PlayerJoined -= OnPlayerJoined;
                MultiplayerManager.Instance.CurrentSession.PlayerHasLeft -= OnPlayerLeft;
            }
        }
    }


    public void InitializeButtons()
    {
        hostButton.onClick.AddListener(OnHostButtonClicked);
        quickJoinButton.onClick.AddListener(OnQuickJoinButtonClicked);
        joinByCodeButton.onClick.AddListener(OnJoinByCodeButtonClicked);
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        if (leaveLobbyButton != null) leaveLobbyButton.onClick.AddListener(OnLeaveLobbyButtonClicked);
    }

    public async void OnHostButtonClicked()
    {
        ShowPanel(loadingPanel);
        await MultiplayerManager.Instance.HostGame();
    }

    public async void OnJoinByCodeButtonClicked()
    {
        if (string.IsNullOrEmpty(joinCodeInput.text)) return;
        
        ShowPanel(loadingPanel);
        await MultiplayerManager.Instance.JoinByCode(joinCodeInput.text);
    }

    public async void OnQuickJoinButtonClicked()
    {
        ShowPanel(loadingPanel);
        await MultiplayerManager.Instance.QuickJoin();
    }

    public void OnStartGameButtonClicked()
    {
        MultiplayerManager.Instance.StartGame(gameSceneName);
    }

    public async void OnLeaveLobbyButtonClicked()
    {
        ShowPanel(loadingPanel);
        await MultiplayerManager.Instance.LeaveSession();
        ShowPanel(mainMenuPanel);
    }


    private void HandleSessionEstablished()
    {
        ShowPanel(lobbyPanel);

        var session = MultiplayerManager.Instance.CurrentSession;

        if (joinCodeTextDisplay != null)
        {
            joinCodeTextDisplay.text = $"{session.Code}";
        }

        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(session.IsHost);
        }

        session.PlayerJoined += OnPlayerJoined;
        session.PlayerHasLeft += OnPlayerLeft;

        UpdatePlayerList();
    }

    private void HandleSessionEnded()
    {
        ShowPanel(mainMenuPanel);
    }

    private void HandleError(string errorMessage)
    {
        ShowPanel(mainMenuPanel);
        
        Debug.LogError("Main Menu caught error: " + errorMessage);
    }


    private void OnPlayerJoined(string playerId)
    {
        UpdatePlayerList();
    }

    private void OnPlayerLeft(string playerId)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        if (playerListContainer == null || playerListItemPrefab == null) return;

        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        var session = MultiplayerManager.Instance.CurrentSession;
        if (session == null) return;

        foreach (var player in session.Players)
        {
            GameObject listItem = Instantiate(playerListItemPrefab, playerListContainer);
            PlayerListItem itemScript = listItem.GetComponent<PlayerListItem>();
            if (itemScript != null)
            {
                bool isLocal = AuthenticationService.Instance.IsSignedIn && player.Id == AuthenticationService.Instance.PlayerId;
                string displayName = isLocal ? "You" : "Opponent";
                bool isHost = player.Id == session.Host;
                string symbol = isHost ? "X" : "O";
                
                itemScript.Setup($"{displayName} ({player.Id.Substring(0, 5)}...)", symbol);
            }
        }
    }


    private void ShowPanel(GameObject panelToShow)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(panelToShow == mainMenuPanel);
        if (loadingPanel != null) loadingPanel.SetActive(panelToShow == loadingPanel);
        if (lobbyPanel != null) lobbyPanel.SetActive(panelToShow == lobbyPanel);
    }
}
