using System;

namespace PotionRoulette;

// Ordering matches the effect IDs in PotionRoulette.ApplyLocal.
internal static class RouletteTable
{
    internal const int TotalWeight = 1000000;
    internal static readonly int[] Weights = { 30000, 200000, 1, 150000, 120000, 150000, 140000, 80000, 129999 };

    internal static int Select(int roll)
    {
        if (roll < 0 || roll >= TotalWeight)
            throw new ArgumentOutOfRangeException(nameof(roll));
        for (int result = 0; result < Weights.Length; result++) {
            if (roll < Weights[result]) return result;
            roll -= Weights[result];
        }
        throw new InvalidOperationException("Roulette weights must total 1000000.");
    }
}
