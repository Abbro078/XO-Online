using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using Unity.Netcode;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance { get; private set; }
    [SerializeField] private string mainMenuScene = "Main Menu Scene";
    
    public ISession CurrentSession { get; private set; }
    private bool isLeaving = false;

    public event Action OnSessionEstablished;
    public event Action<string> OnError;
    public event Action OnSessionEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private async void Start()
    {
        await InitializeUnityServices();
    }

    // private void OnDestroy()
    // {
    //     if (CurrentSession != null)
    //     {
    //     }
    // }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in anonymously as {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            OnError?.Invoke($"Failed to initialize services: {e.Message}");
        }
    }

    public async Task HostGame()
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = 2,
                IsPrivate = false
            }.WithRelayNetwork();
            
            CurrentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Hosted Game! Join Code: {CurrentSession.Code}");
            
            CurrentSession.Deleted += TriggerSessionEnded;
            CurrentSession.PlayerHasLeft += (playerId) => TriggerSessionEnded();

            OnSessionEstablished?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to host game: {e.Message}");
            OnError?.Invoke($"Failed to host: {e.Message}");
        }
    }

    public async Task JoinByCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return;

        try
        {
            var options = new JoinSessionOptions();
            CurrentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options);
            Debug.Log("Joined game via code!");

            CurrentSession.Deleted += TriggerSessionEnded;
            CurrentSession.PlayerHasLeft += (playerId) => TriggerSessionEnded();

            OnSessionEstablished?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to join via code: {e.Message}");
            OnError?.Invoke($"Failed to join: {e.Message}");
        }
    }

    public async Task QuickJoin()
    {
        try
        {
            var options = new QuickJoinOptions();
            CurrentSession = await MultiplayerService.Instance.MatchmakeSessionAsync(options, new SessionOptions().WithRelayNetwork());
            Debug.Log("Quick Joined an existing game!");

            CurrentSession.Deleted += TriggerSessionEnded;
            CurrentSession.PlayerHasLeft += (playerId) => TriggerSessionEnded();

            OnSessionEstablished?.Invoke();
        }
        catch (Exception e)
        {
            Debug.Log($"Quick join didn't find any lobbies, creating a new one instead... ({e.Message})");
            
            try
            {
                // Fallback: Create a new open lobby
                var fallbackOptions = new SessionOptions
                {
                    MaxPlayers = 2,
                    IsPrivate = false
                }.WithRelayNetwork();
                
                CurrentSession = await MultiplayerService.Instance.CreateSessionAsync(fallbackOptions);
                Debug.Log($"Created new Game for Matchmaking! Join Code: {CurrentSession.Code}");
                
                CurrentSession.Deleted += TriggerSessionEnded;
                CurrentSession.PlayerHasLeft += (playerId) => TriggerSessionEnded();

                OnSessionEstablished?.Invoke();
            }
            catch (Exception fallbackEx)
            {
                Debug.LogError($"Failed to quick join or create lobby: {fallbackEx.Message}");
                OnError?.Invoke($"Failed to quick join or create: {fallbackEx.Message}");
            }
        }
    }

    public void StartGame(string gameSceneName)
    {
        Debug.Log($"StartGame called. IsServer: {NetworkManager.Singleton.IsServer}");
        
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"Loading scene: {gameSceneName}");
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("StartGame failed because NetworkManager.Singleton.IsServer is false.");
        }
    }

    public async Task LeaveSession()
    {
        if (isLeaving) return;
        isLeaving = true;

        if (CurrentSession != null)
        {
            try
            {
                await CurrentSession.LeaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error leaving session: {e.Message}");
            }
            finally
            {
                CurrentSession = null;
            }
        }
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != mainMenuScene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }

        isLeaving = false;
    }

    private async void TriggerSessionEnded()
    {
        OnSessionEnded?.Invoke();

        if (CurrentSession != null || NetworkManager.Singleton != null)
        {
            await LeaveSession();
        }
    }
}
