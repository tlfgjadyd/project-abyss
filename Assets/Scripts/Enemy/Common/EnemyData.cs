using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Abyss/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public float maxHp = 30f;
    public float moveSpeed = 2f;
    public float contactDamage = 5f;
    public float contactCooldown = 1f;

    [Header("Drop")]
    public float expAmount = 10f;
    public float energyDrop = 5f;
}
