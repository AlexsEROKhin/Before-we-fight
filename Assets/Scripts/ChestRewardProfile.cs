using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ChestRewardChoice
{
    public ChestUpgradeKind kind;
    public Sprite cardSprite;
    public string displayName;
    [TextArea] public string description;
}

[CreateAssetMenu(fileName = "Reward Profile", menuName = "Before We Fight/Reward Profile")]
public sealed class ChestRewardProfile : ScriptableObject
{
    [SerializeField] private ChestRewardChoice[] choices = new ChestRewardChoice[3];

    public ChestRewardChoice GetChoice(int index) =>
        choices != null && index >= 0 && index < choices.Length ? choices[index] : null;

    public ChestRewardChoice[] PickChoices(int count)
    {
        var result = new ChestRewardChoice[count];
        if (choices == null || choices.Length == 0) return result;
        var pool = new List<ChestRewardChoice>();
        foreach (ChestRewardChoice choice in choices)
            if (choice != null && !pool.Exists(item => item.kind == choice.kind)) pool.Add(choice);
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int selected = UnityEngine.Random.Range(0, pool.Count);
            result[i] = pool[selected];
            pool.RemoveAt(selected);
        }
        return result;
    }
}
