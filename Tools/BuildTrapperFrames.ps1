param(
    [Parameter(Mandatory = $true)][string]$SourceSheet,
    [string[]]$Only = @()
)

# Export the approved pose sheet to native-resolution, point-filtered Unity sprites.
# Re-running this script only changes the exported PNGs; Unity GUIDs remain stable.
Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path $PSScriptRoot -Parent
$outputRoot = Join-Path $projectRoot 'Assets/Art/Imported/Enemies/Trapper'
$bitmap = [System.Drawing.Bitmap]::new($SourceSheet)
$palette = @(
    [System.Drawing.Color]::FromArgb(255, 22, 17, 14),
    [System.Drawing.Color]::FromArgb(255, 51, 40, 27),
    [System.Drawing.Color]::FromArgb(255, 82, 72, 31),
    [System.Drawing.Color]::FromArgb(255, 114, 104, 40),
    [System.Drawing.Color]::FromArgb(255, 154, 137, 54),
    [System.Drawing.Color]::FromArgb(255, 109, 81, 51),
    [System.Drawing.Color]::FromArgb(255, 180, 145, 74),
    [System.Drawing.Color]::FromArgb(255, 204, 119, 14),
    [System.Drawing.Color]::FromArgb(255, 255, 193, 30)
)
$poses = @{}
try {
    for ($pose = 0; $pose -lt 5; $pose++) {
        $small = [System.Drawing.Bitmap]::new(32, 32, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $canvas = [System.Drawing.Graphics]::FromImage($small)
        try {
            $canvas.Clear([System.Drawing.Color]::Transparent)
            $canvas.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $canvas.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
            $canvas.DrawImage($bitmap, [System.Drawing.Rectangle]::new(0, 0, 32, 32),
                [System.Drawing.Rectangle]::new(($pose * 396), 180, 396, 396),
                [System.Drawing.GraphicsUnit]::Pixel)
        } finally { $canvas.Dispose() }
        for ($x = 0; $x -lt 32; $x++) {
            for ($y = 0; $y -lt 32; $y++) {
                $pixel = $small.GetPixel($x, $y)
                if ($pixel.A -lt 128) { $small.SetPixel($x, $y, [System.Drawing.Color]::Transparent); continue }
                $best = $palette[0]
                $score = [int]::MaxValue
                foreach ($candidate in $palette) {
                    $distance = [Math]::Pow($pixel.R - $candidate.R, 2) +
                        [Math]::Pow($pixel.G - $candidate.G, 2) +
                        [Math]::Pow($pixel.B - $candidate.B, 2)
                    if ($distance -lt $score) { $score = $distance; $best = $candidate }
                }
                $small.SetPixel($x, $y, $best)
            }
        }
        $poses[$pose] = $small
    }

    $frames = @(
        @{ Name='idle_0'; Pose=0; X=0; Y=0; Flip=$false; Dim=$false },
        @{ Name='idle_1'; Pose=0; X=0; Y=-1; Flip=$false; Dim=$false },
        @{ Name='idle_2'; Pose=0; X=0; Y=0; Flip=$false; Dim=$true },
        @{ Name='idle_3'; Pose=0; X=0; Y=1; Flip=$false; Dim=$false },
        @{ Name='walk_0'; Pose=1; X=-1; Y=0; Flip=$false; Dim=$false },
        @{ Name='walk_1'; Pose=1; X=0; Y=-1; Flip=$false; Dim=$false },
        @{ Name='walk_2'; Pose=1; X=1; Y=0; Flip=$false; Dim=$false },
        @{ Name='walk_3'; Pose=1; X=1; Y=0; Flip=$true; Dim=$false },
        @{ Name='walk_4'; Pose=1; X=0; Y=-1; Flip=$true; Dim=$false },
        @{ Name='walk_5'; Pose=1; X=-1; Y=0; Flip=$true; Dim=$false },
        @{ Name='cast_0'; Pose=0; X=0; Y=0; Flip=$false; Dim=$false },
        @{ Name='cast_1'; Pose=2; X=0; Y=0; Flip=$false; Dim=$false },
        @{ Name='cast_2'; Pose=2; X=0; Y=-1; Flip=$false; Dim=$false },
        @{ Name='cast_3'; Pose=2; X=0; Y=0; Flip=$false; Dim=$true },
        @{ Name='cast_4'; Pose=2; X=0; Y=0; Flip=$false; Dim=$false },
        @{ Name='hit_0'; Pose=3; X=0; Y=0; Flip=$false; Dim=$false },
        @{ Name='hit_1'; Pose=3; X=-1; Y=0; Flip=$false; Dim=$false },
        @{ Name='death_0'; Pose=3; X=-1; Y=0; Flip=$false; Dim=$false },
        @{ Name='death_1'; Pose=3; X=-2; Y=1; Flip=$false; Dim=$false },
        @{ Name='death_2'; Pose=4; X=0; Y=0; Flip=$false; Dim=$false },
        @{ Name='death_3'; Pose=4; X=1; Y=1; Flip=$false; Dim=$false }
    )

    foreach ($frame in $frames) {
        if ($Only.Count -gt 0 -and $frame.Name -notin $Only) { continue }
        $target = [System.Drawing.Bitmap]::new(32, 32, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $poseBitmap = $poses[[int]$frame.Pose]
        for ($x = 0; $x -lt 32; $x++) {
            for ($y = 0; $y -lt 32; $y++) {
                $fromX = if ($frame.Flip) { 31 - $x } else { $x }
                $pixel = $poseBitmap.GetPixel($fromX, $y)
                if ($frame.Dim -and $pixel.A -gt 0 -and $pixel.R -gt 195 -and $pixel.G -gt 100) {
                    $pixel = $palette[6]
                }
                $toX = $x + [int]$frame.X
                $toY = $y + [int]$frame.Y
                if ($toX -ge 0 -and $toX -lt 32 -and $toY -ge 0 -and $toY -lt 32)
                { $target.SetPixel($toX, $toY, $pixel) }
            }
        }
        $path = Join-Path $outputRoot ('trapper_' + $frame.Name + '.png')
        $target.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        $target.Dispose()
        Write-Output $path
    }
} finally {
    foreach ($poseBitmap in $poses.Values) { $poseBitmap.Dispose() }
    $bitmap.Dispose()
}
