$ErrorActionPreference = 'Stop'
$tablePath = Join-Path $PSScriptRoot '../src/PotionRoulette/RouletteTable.cs'
$source = Get-Content -LiteralPath $tablePath -Raw
$match = [regex]::Match($source, 'Weights\s*=\s*\{([^}]+)\}')
if (-not $match.Success) { throw 'Cannot read weights.' }
$weights = @($match.Groups[1].Value.Split(',') | ForEach-Object { [int]$_.Trim() })
if ($weights.Count -ne 9) { throw 'Expected nine outcomes.' }
if (($weights | Measure-Object -Sum).Sum -ne 1000000) { throw 'Invalid total.' }
if ($weights[2] -ne 1) { throw 'Meowmere must occupy exactly one of one million slots.' }
if (@($weights | Where-Object { $_ -le 0 }).Count -ne 0) { throw 'Weights must be positive.' }
Write-Output 'PASS: nine positive weights total 1,000,000; Meowmere weight is exactly 1.'

# Compile the real selector, independent of Terraria, and exercise every possible draw.
$harness = @'
public static class RouletteChecks
{
    public static void Run()
    {
        int[] expected = { 30000, 200000, 1, 150000, 120000, 150000, 140000, 80000, 129999 };
        int[] actual = new int[9];
        for (int draw = 0; draw < 1000000; draw++)
            actual[RouletteTable.Select(draw)]++;
        for (int i = 0; i < actual.Length; i++)
            if (actual[i] != expected[i]) throw new System.Exception("Incorrect result count: " + i);
        foreach (int invalid in new[] { -1, 1000000 }) {
            bool rejected = false;
            try { RouletteTable.Select(invalid); }
            catch (System.ArgumentOutOfRangeException) { rejected = true; }
            if (!rejected) throw new System.Exception("Out-of-range draw accepted.");
        }
    }
}
'@
Add-Type -TypeDefinition ($source + [Environment]::NewLine + $harness)
[PotionRoulette.RouletteChecks]::Run()
Write-Output 'PASS: actual C# selector checked for all 1,000,000 draws and invalid boundaries.'
