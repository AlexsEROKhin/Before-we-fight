$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$projectRoot = Split-Path -Parent $PSScriptRoot
$importedDir = Join-Path $projectRoot 'Assets/Art/Imported/Pickups'
$sourceDir = Join-Path $projectRoot 'Assets/Art/Source/Pickups'
New-Item -ItemType Directory -Path $importedDir, $sourceDir -Force | Out-Null
$pngPath = Join-Path $importedDir 'healing_potion.png'
$asepritePath = Join-Path $sourceDir 'healing_potion.aseprite'

$pixels = @(
    '                '
    '      KKKK      '
    '      K55K      '
    '      K55K      '
    '     KK11KK     '
    '    K111111K    '
    '   K14411111K   '
    '   K14133331K   '
    '   K11333331K   '
    '   K11333331K   '
    '   K11333331K   '
    '    K133331K    '
    '    K111111K    '
    '     KKKKKK     '
    '                '
    '                '
)
$palette = @{
    ' ' = [System.Drawing.Color]::Transparent
    K = [System.Drawing.Color]::FromArgb(255, 20, 17, 27)
    '1' = [System.Drawing.Color]::FromArgb(255, 93, 109, 122)
    '3' = [System.Drawing.Color]::FromArgb(255, 200, 45, 70)
    '4' = [System.Drawing.Color]::FromArgb(255, 249, 204, 174)
    '5' = [System.Drawing.Color]::FromArgb(255, 149, 100, 63)
}

$image = [System.Drawing.Bitmap]::new(16, 16, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
try {
    for ($y = 0; $y -lt 16; $y++) {
        if ($pixels[$y].Length -ne 16) { throw "Row $y is not 16 pixels wide" }
        for ($x = 0; $x -lt 16; $x++) {
            $symbol = [string]$pixels[$y][$x]
            $image.SetPixel($x, $y, $palette[$symbol])
        }
    }
    $image.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally { $image.Dispose() }

$aseprite = 'C:/Program Files/Aseprite/Aseprite.exe'
if (Test-Path -LiteralPath $aseprite) {
    $asepriteProcess = Start-Process -FilePath $aseprite -ArgumentList @(
        '-b', ('"' + $pngPath + '"'), '--save-as', ('"' + $asepritePath + '"')
    ) -WindowStyle Hidden -PassThru -Wait
    if ($asepriteProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $asepritePath)) {
        throw "Aseprite export failed: $($asepriteProcess.ExitCode)"
    }
}
Write-Output "Created $pngPath"
if (Test-Path -LiteralPath $asepritePath) { Write-Output "Created $asepritePath" }
