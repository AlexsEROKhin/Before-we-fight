param(
    [Parameter(Mandatory = $true)][string]$WarningSource,
    [Parameter(Mandatory = $true)][string]$ActiveSource
)

# Mechanical nearest-neighbour export of the two approved circular pose images.
Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path $PSScriptRoot -Parent
$outputRoot = Join-Path $projectRoot 'Assets/Art/Imported/Enemies/Trapper'
$palette = @(
    [System.Drawing.Color]::FromArgb(255, 47, 25, 10),
    [System.Drawing.Color]::FromArgb(255, 108, 53, 8),
    [System.Drawing.Color]::FromArgb(255, 159, 84, 8),
    [System.Drawing.Color]::FromArgb(255, 211, 132, 10),
    [System.Drawing.Color]::FromArgb(255, 244, 179, 24),
    [System.Drawing.Color]::FromArgb(255, 255, 217, 53)
)

foreach ($item in @(
    @{ Input=$WarningSource; Name='sticky_warning_v2.png' },
    @{ Input=$ActiveSource; Name='sticky_puddle_v2.png' }
)) {
    $source = [System.Drawing.Bitmap]::new($item.Input)
    $small = [System.Drawing.Bitmap]::new(32, 32, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $canvas = [System.Drawing.Graphics]::FromImage($small)
        try {
            $canvas.Clear([System.Drawing.Color]::Transparent)
            $canvas.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
            $canvas.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
            $canvas.DrawImage($source, [System.Drawing.Rectangle]::new(0, 0, 32, 32),
                [System.Drawing.Rectangle]::new(0, 0, $source.Width, $source.Height),
                [System.Drawing.GraphicsUnit]::Pixel)
        } finally { $canvas.Dispose() }
        for ($x=0; $x -lt 32; $x++) {
            for ($y=0; $y -lt 32; $y++) {
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
        $path = Join-Path $outputRoot $item.Name
        $small.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Output $path
    } finally {
        $source.Dispose()
        $small.Dispose()
    }
}
