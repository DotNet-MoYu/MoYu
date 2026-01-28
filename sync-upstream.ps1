param(
    [string]$Repo = (Get-Location).Path,
    [string]$UpstreamRemote = 'upstream',
    [string]$UpstreamBranch = 'v5-transition',
    [switch]$Rebase,
    [switch]$AllowDirty,
    [switch]$Commit,
    [switch]$SkipReplace,
    [switch]$SkipSync,
    [switch]$ReplaceOnly,
    [switch]$AllowUnrelated,
    [switch]$AutoStash,
    [switch]$PreferUpstream,
    [switch]$NormalizeLayout,
    [switch]$ForceRenamePaths,
    [bool]$RemoveTestsFromSln = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-FileEncodingInfo {
    param([byte[]]$Bytes)
    if ($Bytes.Length -ge 3 -and $Bytes[0] -eq 0xEF -and $Bytes[1] -eq 0xBB -and $Bytes[2] -eq 0xBF) { return @{ Encoding = [System.Text.UTF8Encoding]::new($true); Bom = $true; BomLength = 3 } }
    if ($Bytes.Length -ge 2 -and $Bytes[0] -eq 0xFF -and $Bytes[1] -eq 0xFE) { return @{ Encoding = [System.Text.UnicodeEncoding]::new($false, $true); Bom = $true; BomLength = 2 } }
    if ($Bytes.Length -ge 2 -and $Bytes[0] -eq 0xFE -and $Bytes[1] -eq 0xFF) { return @{ Encoding = [System.Text.UnicodeEncoding]::new($true, $true); Bom = $true; BomLength = 2 } }
    if ($Bytes.Length -ge 4 -and $Bytes[0] -eq 0xFF -and $Bytes[1] -eq 0xFE -and $Bytes[2] -eq 0x00 -and $Bytes[3] -eq 0x00) { return @{ Encoding = [System.Text.UTF32Encoding]::new($false, $true); Bom = $true; BomLength = 4 } }
    if ($Bytes.Length -ge 4 -and $Bytes[0] -eq 0x00 -and $Bytes[1] -eq 0x00 -and $Bytes[2] -eq 0xFE -and $Bytes[3] -eq 0xFF) { return @{ Encoding = [System.Text.UTF32Encoding]::new($true, $true); Bom = $true; BomLength = 4 } }
    return @{ Encoding = [System.Text.UTF8Encoding]::new($false); Bom = $false; BomLength = 0 }
}

function Read-TextWithEncoding {
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $info = Get-FileEncodingInfo -Bytes $bytes
    $text = $info.Encoding.GetString($bytes, $info.BomLength, $bytes.Length - $info.BomLength)
    return @{ Text = $text; Encoding = $info.Encoding; Bom = $info.Bom }
}

function Write-TextWithEncoding {
    param([string]$Path,[string]$Text,[System.Text.Encoding]$Encoding,[bool]$Bom)
    $payload = $Encoding.GetBytes($Text)
    if ($Bom) {
        $preamble = $Encoding.GetPreamble()
        $bytes = New-Object byte[] ($preamble.Length + $payload.Length)
        [System.Array]::Copy($preamble, 0, $bytes, 0, $preamble.Length)
        [System.Array]::Copy($payload, 0, $bytes, $preamble.Length, $payload.Length)
        [System.IO.File]::WriteAllBytes($Path, $bytes)
    } else {
        [System.IO.File]::WriteAllBytes($Path, $payload)
    }
}

function Replace-InFile {
    param([string]$Path,[string[]]$Find,[string[]]$Replace)
    $info = Read-TextWithEncoding -Path $Path
    $text = $info.Text
    $orig = $text
    for ($i = 0; $i -lt $Find.Length; $i++) { $text = $text -replace [regex]::Escape($Find[$i]), $Replace[$i] }
    if ($text -ne $orig) {
        Write-TextWithEncoding -Path $Path -Text $text -Encoding $info.Encoding -Bom $info.Bom
        return $true
    }
    return $false
}

function Invoke-Git {
    param([string[]]$GitArgs)
    $null = & git @GitArgs
    if ($LASTEXITCODE -ne 0) { throw ("git " + ($GitArgs -join ' ') + " (exit $LASTEXITCODE)") }
}

function Get-ReplaceFiles {
    param([string]$RepoPath)
    $rg = Get-Command rg -ErrorAction SilentlyContinue
    if ($rg) {
        $include = @(
            '*.cs','*.csx','*.csproj','*.sln','*.slnx','*.props','*.targets','*.nuspec',
            '*.config','*.json','*.jsonc','*.xml','*.yml','*.yaml','*.md','*.mdx','*.txt',
            '*.ps1','*.psm1','*.psd1','*.sh','*.bat','*.cmd','Dockerfile',
            '*.js','*.jsx','*.ts','*.tsx','*.css','*.scss','*.less','*.html','*.cshtml','*.razor',
            '*.editorconfig','*.gitignore','*.gitattributes'
        )
        $args = @(
            '-l',
            '-e','MoYu','-e','MoYu','-e','MoYu','-e','MoYu','-e','MoYu','-e','MoYu',
            '--no-messages','--text'
        )
        foreach ($g in $include) { $args += @('-g', $g) }
        $args += @('-g','!schemas/**','-g','!snks/**','-g','!**/bin/**','-g','!**/obj/**','-g','!**/.git/**','-g','!icon*.png')
        $files = & $rg @args $RepoPath
        $rootReadme = Join-Path $RepoPath 'README.md'
        $rootReadmeZh = Join-Path $RepoPath 'README.zh.md'
        return $files | Where-Object { $_ -ne $rootReadme -and $_ -ne $rootReadmeZh }
    }

    return Get-ChildItem -Path $RepoPath -Recurse -File |
        Where-Object {
            $_.FullName -notmatch '\\schemas\\' -and $_.FullName -notmatch '\\snks\\' -and $_.FullName -notmatch '\\bin\\' -and
            $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\.git\\' -and $_.Name -notlike 'icon*.png'
        } |
        ForEach-Object { $_.FullName } |
        Where-Object {
            try { (Get-Content -Raw -Path $_ -ErrorAction Stop) -match 'MoYu|MoYu|MoYu|MoYu|MoYu|MoYu' } catch { $false }
        } |
        Where-Object {
            $_ -ne (Join-Path $RepoPath 'README.md') -and $_ -ne (Join-Path $RepoPath 'README.zh.md')
        }
}

function Get-RenameTargets {
    param([string]$RepoPath)
    Get-ChildItem -Path $RepoPath -Recurse -Force |
        Where-Object {
            $_.FullName -notmatch '\\schemas\\' -and $_.FullName -notmatch '\\snks\\' -and $_.FullName -notmatch '\\bin\\' -and
            $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\.git\\' -and $_.Name -notlike 'icon*.png'
        } |
        Where-Object { $_.Name -match 'MoYu|MoYu|MoYu|MoYu|MoYu|MoYu' }
}

function Rename-PathTokens {
    param([string]$RepoPath,[switch]$Force)

    $dirs = Get-RenameTargets -RepoPath $RepoPath | Where-Object { $_.PSIsContainer } | Sort-Object { $_.FullName.Length } -Descending
    foreach ($t in $dirs) {
        $newName = $t.Name -replace 'MoYu','MoYu' -replace 'MoYu','moyu' -replace 'MoYu','MOYU' -replace 'MoYu','MoYu' -replace 'MoYu','moyu' -replace 'MoYu','MOYU'
        if ($newName -ne $t.Name) {
            $dest = Join-Path $t.Parent.FullName $newName
            if ((Test-Path $dest) -and -not $Force) { continue }
            if ((Test-Path $dest) -and $Force) { Remove-Item -Path $dest -Recurse -Force }
            Rename-Item -Path $t.FullName -NewName $newName
        }
    }

    $files = Get-RenameTargets -RepoPath $RepoPath | Where-Object { -not $_.PSIsContainer } | Sort-Object { $_.FullName.Length } -Descending
    foreach ($t in $files) {
        if (-not (Test-Path $t.FullName)) { continue }
        $newName = $t.Name -replace 'MoYu','MoYu' -replace 'MoYu','moyu' -replace 'MoYu','MOYU' -replace 'MoYu','MoYu' -replace 'MoYu','moyu' -replace 'MoYu','MOYU'
        if ($newName -ne $t.Name) {
            $dest = Join-Path $t.DirectoryName $newName
            if ((Test-Path $dest) -and -not $Force) { continue }
            if ((Test-Path $dest) -and $Force) { Remove-Item -Path $dest -Force }
            Rename-Item -Path $t.FullName -NewName $newName
        }
    }
}

function Backup-BrandingFiles {
    param([string]$RepoPath,[string]$TempRoot)
    $items = Get-ChildItem -Path $RepoPath -Recurse -File | Where-Object { $_.Name -like 'README*' -or $_.Name -like 'icon*.png' }
    foreach ($item in $items) {
        $rel = $item.FullName.Substring($RepoPath.Length).TrimStart('\\')
        $dest = Join-Path $TempRoot $rel
        $destDir = Split-Path $dest -Parent
        if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
        Copy-Item -Path $item.FullName -Destination $dest -Force
    }
}

function Restore-BrandingFiles {
    param([string]$RepoPath,[string]$TempRoot)
    if (-not (Test-Path $TempRoot)) { return }
    $items = Get-ChildItem -Path $TempRoot -Recurse -File
    foreach ($item in $items) {
        $rel = $item.FullName.Substring($TempRoot.Length).TrimStart('\\')
        $dest = Join-Path $RepoPath $rel
        $destDir = Split-Path $dest -Parent
        if (-not (Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
        Copy-Item -Path $item.FullName -Destination $dest -Force
    }
}

function Get-UpstreamValue {
    param([string]$FilePathInRepo,[string]$Pattern)
    $spec = "${UpstreamRemote}/${UpstreamBranch}:$FilePathInRepo"
    $content = & git -C $Repo show $spec
    if ($LASTEXITCODE -ne 0) { throw "git show $spec" }
    $m = [regex]::Match($content, $Pattern)
    if (-not $m.Success) { throw "Pattern not found in $FilePathInRepo" }
    return $m.Groups[1].Value
}

function Remove-TestsFromFrameworkSln {
    param([string]$SlnPath)
    if (-not (Test-Path $SlnPath)) { return }

    $content = Get-Content $SlnPath
    $patterns = @(
        '"..\\tests\\MoYu.UnitTests\\MoYu.UnitTests.csproj"',
        '"..\\tests\\MoYu.IntegrationTests\\MoYu.IntegrationTests.csproj"',
        '"..\\tests\\MoYu.TestProject\\MoYu.TestProject.csproj"'
    )

    $lines = @()
    $skip = $false
    foreach ($line in $content) {
        if ($line -match '^Project\(' -and ($patterns | Where-Object { $line -like "*$_*" })) {
            $skip = $true
            continue
        }
        if ($skip -and $line -match '^EndProject') {
            $skip = $false
            continue
        }
        if (-not $skip) { $lines += $line }
    }

    $guidsToRemove = @(
        '{BE9F6374-929F-4541-811A-08D9B971CAC7}',
        '{F6D6D551-E436-480B-A334-7FD6DADD01D1}',
        '{323F958E-8900-489D-BF47-2540A1DF10BA}'
    )

    $final = @()
    foreach ($line in $lines) {
        if ($guidsToRemove | Where-Object { $line -like "*$_*" }) { continue }
        $final += $line
    }

    Set-Content -Path $SlnPath -Value $final -Encoding UTF8
}

if (-not (Test-Path (Join-Path $Repo '.git'))) { throw "Not a git repository: $Repo" }

if (-not $AllowDirty) {
    $status = git -C $Repo status --porcelain
    if ($status) { throw 'Working tree is dirty. Commit or stash changes, or pass -AllowDirty.' }
}

if ($ReplaceOnly -and ($SkipSync -or $SkipReplace)) { throw 'ReplaceOnly cannot be combined with SkipSync or SkipReplace.' }

if (-not $ReplaceOnly) {
    $tempRoot = Join-Path $env:TEMP ("moyu-branding-" + [guid]::NewGuid().ToString('N'))
    Backup-BrandingFiles -RepoPath $Repo -TempRoot $tempRoot
}

if (-not $ReplaceOnly -and -not $SkipSync) {
    Write-Host "Fetching $UpstreamRemote/$UpstreamBranch..."
    Invoke-Git -GitArgs @('-C', $Repo, 'fetch', $UpstreamRemote, $UpstreamBranch)
    Invoke-Git -GitArgs @('-C', $Repo, 'checkout', 'master')

    if ($Rebase) {
        if ($AllowUnrelated) { throw 'AllowUnrelated is only supported with merge (no rebase).' }
        if ($AutoStash) { throw 'AutoStash is only supported with merge (no rebase).' }
        Write-Host "Rebasing onto $UpstreamRemote/$UpstreamBranch..."
        Invoke-Git -GitArgs @('-C', $Repo, 'rebase', "$UpstreamRemote/$UpstreamBranch")
    } else {
        Write-Host "Merging $UpstreamRemote/$UpstreamBranch..."
        if ($AutoStash) { Invoke-Git -GitArgs @('-C', $Repo, 'stash', 'push', '-u', '-m', 'moyu sync auto-stash') }
        $mergeArgs = @('-C', $Repo, 'merge', "$UpstreamRemote/$UpstreamBranch")
        if ($AllowUnrelated) { $mergeArgs += '--allow-unrelated-histories' }
        if ($PreferUpstream) { $mergeArgs += @('-X', 'theirs') }
        Invoke-Git -GitArgs $mergeArgs
        if ($AutoStash) { Invoke-Git -GitArgs @('-C', $Repo, 'stash', 'pop') }
    }
}

if ($NormalizeLayout) {
    Write-Host "Normalizing paths (MoYu -> MoYu)..."
    Rename-PathTokens -RepoPath $Repo -Force:$ForceRenamePaths
}

if (-not $ReplaceOnly) {
    Restore-BrandingFiles -RepoPath $Repo -TempRoot $tempRoot
}

if ($RemoveTestsFromSln) {
    Remove-TestsFromFrameworkSln -SlnPath (Join-Path $Repo 'framework\MoYu.sln')
}

if (-not $ReplaceOnly) {
    # Update version + frameworks from upstream
    $tfm = Get-UpstreamValue -FilePathInRepo 'framework/Directory.Build.props' -Pattern '<TargetFrameworks>([^<]+)</TargetFrameworks>'
    $ver = Get-UpstreamValue -FilePathInRepo 'framework/Directory.Build.props' -Pattern '<Version>([^<]+)</Version>'

    $props = @('framework/Directory.Build.props','tools/MoYu.Tools/Directory.Build.props')
    foreach ($p in $props) {
        $full = Join-Path $Repo $p
        $info = Read-TextWithEncoding -Path $full
        $text = $info.Text
        $text = $text -replace '<TargetFrameworks>.*?</TargetFrameworks>', "<TargetFrameworks>$tfm</TargetFrameworks>"
        $text = $text -replace '<Version>.*?</Version>', "<Version>$ver</Version>"
        Write-TextWithEncoding -Path $full -Text $text -Encoding $info.Encoding -Bom $info.Bom
    }
}

if (-not $SkipReplace) {
    Write-Host "Replacing branding strings..."
    $files = Get-ReplaceFiles -RepoPath $Repo
    $find = @(
        'MoYu','MoYu','MoYu','MoYu','MoYu','MoYu',
        'https://gitee.com/dotnetchina/MoYu','https://gitee.com/dotnetchina/MoYu',
        'https://gitee.com/dotnetchina/MoYu','https://gitee.com/dotnetchina/MoYu'
    )
    $repl = @(
        'MoYu','moyu','MOYU','MoYu','moyu','MOYU',
        'https://gitee.com/dotnetmoyu/MoYu','https://gitee.com/dotnetmoyu/MoYu',
        'https://gitee.com/dotnetmoyu/MoYu','https://gitee.com/dotnetmoyu/MoYu'
    )

    $changed = 0
    foreach ($f in $files) { if (Replace-InFile -Path $f -Find $find -Replace $repl) { $changed++ } }
    Write-Host "Replaced in $changed files."
} else {
    Write-Host "Skip replace (-SkipReplace)."
}

if ($Commit) {
    Invoke-Git -GitArgs @('-C', $Repo, 'add', '-A')
    $msg = "sync upstream $UpstreamBranch + branding"
    Invoke-Git -GitArgs @('-C', $Repo, 'commit', '-m', $msg)
}

Write-Host 'Done.'
