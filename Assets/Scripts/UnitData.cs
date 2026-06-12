using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "RNGod/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string unitName = "Unit";
    public Rarity rarity = Rarity.Common;
    public UnitType unitType = UnitType.Ranged;

    [Header("Combat")]
    public float attackDamage = 1f;
    public float attackRange = 3f;
    public float attackCooldown = 1f;

    [Header("Prefab")]
    public GameObject prefab;
}

public enum Rarity
{
    Common = 0,
    Rare = 1,
    Epic = 2,
    Legendary = 3
}

public enum UnitType
{
    Melee = 0,
    Ranged = 1
}