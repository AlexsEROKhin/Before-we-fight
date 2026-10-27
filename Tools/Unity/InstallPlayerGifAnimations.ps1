$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$frameRoot = Join-Path $projectRoot 'Assets\Art\Source\PlayerGifUpdates\Frames'
$metaTemplate = [System.IO.File]::ReadAllText((Join-Path $projectRoot 'Assets\Art\Source\Hueta2\Frames\hueta2_0.png.meta'))
$folderTemplate = [System.IO.File]::ReadAllText((Join-Path $projectRoot 'Assets\Art\Source\Hueta2.meta'))
$utf8 = [System.Text.UTF8Encoding]::new($false)

function SaveText([string]$path, [string]$content) {
    [System.IO.File]::WriteAllText($path, $content, $utf8)
}

function EnsureMeta([string]$path, [string]$template, [bool]$isSprite) {
    $metaPath = "$path.meta"
    if (Test-Path -LiteralPath $metaPath) { return }
    $guid = [guid]::NewGuid().ToString('N')
    $content = [regex]::Replace($template, '(?m)^guid: [0-9a-f]+$', "guid: $guid", 1)
    if ($isSprite) {
        $content = $content.Replace('spritePixelsToUnits: 48', 'spritePixelsToUnits: 72')
        $content = $content.Replace('spritePivot: {x: 0.5, y: 0.44}', 'spritePivot: {x: 0.5, y: 0.43}')
    }
    SaveText $metaPath $content
}

EnsureMeta (Join-Path $projectRoot 'Assets\Art\Source\PlayerGifUpdates') $folderTemplate $false
EnsureMeta $frameRoot $folderTemplate $false

$frameGuids = @{}
foreach ($entry in @(@('Idle32px', 10), @('Run32px', 12), @('Jump32px', 5))) {
    $name = [string]$entry[0]
    $count = [int]$entry[1]
    $ids = @()
    for ($i = 0; $i -lt $count; $i++) {
        $path = Join-Path $frameRoot "${name}_$i.png"
        if (-not (Test-Path -LiteralPath $path)) { throw "Missing frame: $path" }
        EnsureMeta $path $metaTemplate $true
        $meta = [System.IO.File]::ReadAllText("$path.meta")
        $match = [regex]::Match($meta, '(?m)^guid: ([0-9a-f]+)$')
        if (-not $match.Success) { throw "Missing GUID: $path.meta" }
        $ids += $match.Groups[1].Value
    }
    $frameGuids[$name] = $ids
}

function UpdateClip([string]$name, [string[]]$guids, [double]$fps, [double]$step) {
    $path = Join-Path $projectRoot "Assets\Generated\Animations\NewHero\$name.anim"
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing clip: $path" }
    $yaml = [System.IO.File]::ReadAllText($path)
    $culture = [System.Globalization.CultureInfo]::InvariantCulture
    $curve = ""
    $mapping = ""
    for ($i = 0; $i -lt $guids.Count; $i++) {
        $time = ($i * $step).ToString('0.######', $culture)
        $ref = "{fileID: 21300000, guid: $($guids[$i]), type: 3}"
        $curve += "    - time: $time`n      value: $ref`n"
        $mapping += "    - $ref`n"
    }
    $curvePattern = '(?s)(  m_PPtrCurves:\r?\n  - serializedVersion: 2\r?\n    curve:\r?\n).*?(?=    attribute: m_Sprite)'
    if (-not [regex]::IsMatch($yaml, $curvePattern)) { throw "Cannot locate sprite curve: $name" }
    $yaml = [regex]::Replace($yaml, $curvePattern, ('$1' + $curve), 1)
    $mapPattern = '(?s)(    pptrCurveMapping:\r?\n).*?(?=  m_AnimationClipSettings:)'
    if (-not [regex]::IsMatch($yaml, $mapPattern)) { throw "Cannot locate sprite mapping: $name" }
    $yaml = [regex]::Replace($yaml, $mapPattern, ('$1' + $mapping), 1)
    $rate = $fps.ToString('0.######', $culture)
    $stop = ($guids.Count * $step).ToString('0.######', $culture)
    $yaml = [regex]::Replace($yaml, '(?m)^  m_SampleRate: [^\r\n]+', "  m_SampleRate: $rate", 1)
    $yaml = [regex]::Replace($yaml, '(?m)^    m_StopTime: [^\r\n]+', "    m_StopTime: $stop", 1)
    SaveText $path $yaml
}

foreach ($name in @('NewHero_Idle', 'NewHero_Idle_Up', 'NewHero_Idle_Down')) {
    UpdateClip $name $frameGuids['Idle32px'] 12.5 0.08
}
foreach ($name in @('NewHero_Walk_Side', 'NewHero_Walk_Up', 'NewHero_Walk_Down',
                   'NewHero_Walk_UpRight', 'NewHero_Walk_DownRight')) {
    UpdateClip $name $frameGuids['Run32px'] (1000.0 / 70.0) 0.07
}
UpdateClip 'NewHero_Jump' $frameGuids['Jump32px'] 12.5 0.08

$oldPreview = '831d6947721f10d4d888f5616fd96be5'
$newPreview = $frameGuids['Idle32px'][0]
foreach ($relative in @('Assets\Prefabs\Player.prefab', 'Assets\Scenes\Arena.unity')) {
    $path = Join-Path $projectRoot $relative
    $yaml = [System.IO.File]::ReadAllText($path)
    $needle = "m_Sprite: {fileID: 21300000, guid: $oldPreview, type: 3}"
    if (-not $yaml.Contains($needle)) { throw "Expected preview sprite not found: $relative" }
    $yaml = $yaml.Replace($needle, "m_Sprite: {fileID: 21300000, guid: $newPreview, type: 3}")
    SaveText $path $yaml
}

Write-Output 'Updated 9 animation clips, 27 sprite importers, prefab, and Arena preview.'
