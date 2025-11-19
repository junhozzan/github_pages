using UnityEngine;

[CreateAssetMenu(fileName = "SagaGameData", menuName = "Scriptable Objects/SagaGameData")]
public class SagaGameData : ScriptableObject
{
    public int lobbyModeID;
    public int bubbleModeID;
    public string shadowPrefab;
    public string layPrefab;

    private static SagaGameData instance = null;
    public static SagaGameData Instance => instance == null ? instance = Resources.Load<SagaGameData>("saga_game_data") : instance;
}
