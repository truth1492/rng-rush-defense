using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnitDatabase", menuName = "RNGod/UnitDatabase")]
public class UnitDatabase : ScriptableObject
{
    [Header("Common Units (88.9%) — 2 Melee, 3 Ranged")]
    public UnitData Common_Melee_1;
    public UnitData Common_Melee_2;
    public UnitData Common_Ranged_1;
    public UnitData Common_Ranged_2;
    public UnitData Common_Ranged_3;

    [Header("Rare Units (10.0%) — 2 Melee, 3 Ranged")]
    public UnitData Rare_Melee_1;
    public UnitData Rare_Melee_2;
    public UnitData Rare_Ranged_1;
    public UnitData Rare_Ranged_2;
    public UnitData Rare_Ranged_3;

    [Header("Epic Units (1.0%) — 2 Melee, 3 Ranged")]
    public UnitData Epic_Melee_1;
    public UnitData Epic_Melee_2;
    public UnitData Epic_Ranged_1;
    public UnitData Epic_Ranged_2;
    public UnitData Epic_Ranged_3;

    [Header("Legendary Units (0.1%) — 2 Melee, 2 Ranged")]
    public UnitData Legendary_Melee_1;
    public UnitData Legendary_Melee_2;
    public UnitData Legendary_Ranged_1;
    public UnitData Legendary_Ranged_2;

    // ── Summon logic ──────────────────────────────────────────────────────────
    public UnitData GetRandomUnit()
    {
        float roll = Random.Range(0f, 100f);

        Rarity rarity;
        if (roll < 0.1f) rarity = Rarity.Legendary;
        else if (roll < 1.1f) rarity = Rarity.Epic;
        else if (roll < 11.1f) rarity = Rarity.Rare;
        else rarity = Rarity.Common;

        return GetRandomUnitOfRarity(rarity);
    }

    public UnitData GetRandomUnitOfRarity(Rarity rarity)
    {
        UnitData[] pool = GetPoolForRarity(rarity);

        List<UnitData> valid = new List<UnitData>();
        foreach (UnitData u in pool)
            if (u != null) valid.Add(u);

        if (valid.Count == 0)
        {
            Debug.LogWarning($"[UnitDatabase] No units assigned for rarity {rarity}!");
            return null;
        }

        return valid[Random.Range(0, valid.Count)];
    }

    UnitData[] GetPoolForRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Legendary:
                return new[] { Legendary_Melee_1, Legendary_Melee_2,
                                Legendary_Ranged_1, Legendary_Ranged_2 };
            case Rarity.Epic:
                return new[] { Epic_Melee_1, Epic_Melee_2,
                                Epic_Ranged_1, Epic_Ranged_2, Epic_Ranged_3 };
            case Rarity.Rare:
                return new[] { Rare_Melee_1, Rare_Melee_2,
                                Rare_Ranged_1, Rare_Ranged_2, Rare_Ranged_3 };
            default:
                return new[] { Common_Melee_1, Common_Melee_2,
                                Common_Ranged_1, Common_Ranged_2, Common_Ranged_3 };
        }
    }
}