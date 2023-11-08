using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { set; get; }

    [TitleGroup("General", Alignment = TitleAlignments.Centered, Indent = true)]
    public GameState gameState = GameState.Standby;

    [TitleGroup("GameOver", Alignment = TitleAlignments.Centered, Indent = true)]
    [SerializeField] private IntVariable collectedCoins = null;

    [HideInInspector] public bool vibrationState = true;

	private void Awake()
	{
        if(Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        vibrationState = PlayerPrefsX.GetBool("Vibration", true);
    }

    private void OnEnable() {
        //Start listening for game events
        Message.AddListener<GameEventMessage>(OnMessage);
    }

    private void OnDisable() {
        //Stop listening for game events
        Message.RemoveListener<GameEventMessage>(OnMessage);
    }

    private void OnMessage(GameEventMessage message) 
    {
        if(message.EventName == "GameOver") { GameOver(); }
        if(message.EventName == "Is Playing") { 
            SetGameState(GameState.Playing);
            TinySauce.OnGameStarted(PlayerPrefs.GetInt("CurrentLevel").ToString());
        }
    }

    public void SetGameState(GameState state) { gameState = state; }

    private void GameOver()
    {
        SetGameState(GameState.GameOver);
        TinySauce.OnGameFinished(PlayerPrefs.GetInt("CurrentLevel").ToString(), collectedCoins.Value);
        PlayerPrefs.SetInt("CurrentLevel", (PlayerPrefs.GetInt("CurrentLevel") + 1));
        PlayerPrefs.SetInt("Coins", (PlayerPrefs.GetInt("Coins") + collectedCoins.Value));
    }
}

public enum GameState { Standby, Playing, GameOver, Waiting }