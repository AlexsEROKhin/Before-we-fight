$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$path = Join-Path (Split-Path $PSScriptRoot -Parent) 'Assets\Art\ChestLoot\chest_prompt_panel.png'
$bmp = [System.Drawing.Bitmap]::new(160, 32, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

function Fill([int]$x1, [int]$y1, [int]$x2, [int]$y2, [string]$hex) {
    $c = [System.Drawing.ColorTranslator]::FromHtml($hex)
    for ($y = $y1; $y -le $y2; $y++) {
        for ($x = $x1; $x -le $x2; $x++) { $bmp.SetPixel($x, $y, $c) }
    }
}

# Palette and stepped corners match the game's maroon MENU button.
Fill 4 0 155 31 '#160D13'
Fill 0 4 159 27 '#160D13'
Fill 4 2 155 29 '#B07073'
Fill 2 4 157 27 '#B07073'
Fill 5 4 154 27 '#6C2B35'
Fill 4 6 155 25 '#6C2B35'
Fill 7 6 152 25 '#431923'
Fill 8 7 151 24 '#55202B'
Fill 10 6 149 6 '#A65C5D'
Fill 10 25 149 25 '#28131B'
Fill 11 8 31 23 '#190F15'
Fill 12 9 30 22 '#815133'
Fill 14 11 28 20 '#3E2520'
Fill 34 11 34 20 '#A65C5D'
Fill 36 11 36 20 '#A65C5D'
Fill 142 11 142 20 '#A65C5D'
Fill 144 11 144 20 '#A65C5D'

$glyphs = @{
    'E' = @('11111','10000','10000','11110','10000','10000','11111')
    'O' = @('01110','10001','10001','10001','10001','10001','01110')
    'P' = @('11110','10001','10001','11110','10000','10000','10000')
    'N' = @('10001','11001','10101','10101','10011','10001','10001')
    'C' = @('01111','10000','10000','10000','10000','10000','01111')
    'H' = @('10001','10001','10001','11111','10001','10001','10001')
    'S' = @('01111','10000','10000','01110','00001','00001','11110')
    'T' = @('11111','00100','00100','00100','00100','00100','00100')
    '-' = @('00000','00000','00000','11111','00000','00000','00000')
    ' ' = @('00000','00000','00000','00000','00000','00000','00000')
}
function Letter([char]$char, [int]$left, [int]$top, [string]$hex) {
    $shape = $glyphs[[string]$char]
    $color = [System.Drawing.ColorTranslator]::FromHtml($hex)
    for ($row = 0; $row -lt 7; $row++) {
        for ($col = 0; $col -lt 5; $col++) {
            if ($shape[$row][$col] -eq '1') { $bmp.SetPixel($left + $col, $top + $row, $color) }
        }
    }
}
Letter 'E' 19 13 '#FFE0A0'
$caption = 'OPEN CHEST'
for ($i = 0; $i -lt $caption.Length; $i++) {
    Letter $caption[$i] (57 + $i * 7) 13 '#F3C776'
}
Fill 45 16 50 16 '#D1A06A'
$bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output $path
