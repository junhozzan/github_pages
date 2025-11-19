using UnityEngine;

[CreateAssetMenu(fileName = "ExplosionSO", menuName = "Scriptable Objects/ExplosionSO")]
public class ExplosionSO : ScriptableObject
{
    // 礎熱還
    public Vector3Int[] evenCoords = null;
    // �汝鶬�
    public Vector3Int[] oddCoords = null;
}
