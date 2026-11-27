[CmdletBinding()]
param(
    [ValidateSet("cn", "ov")]
    [string]$Region = "ov",

    [switch]$CheckOnly,
    [switch]$SkipLogin,
    [switch]$SkipDoctor,
    [switch]$ForceCliUpdate,
    [switch]$NpmNoProxy
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$StateDirectory = Join-Path $RepoRoot ".tripo-workflow"
$StatusPath = Join-Path $StateDirectory "status.json"
$MinimumNodeMajor = 20
$script:Checks = [ordered]@{}

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Refresh-ProcessPath {
    $machinePath = [Environment]::GetEnvironmentVariable("Path", "Machine")
    $userPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $env:Path = "$machinePath;$userPath"
}

function Get-NativeCommand {
    param(
        [Parameter(Mandatory)]
        [string[]]$Names
    )

    foreach ($name in $Names) {
        $command = Get-Command $name -ErrorAction SilentlyContinue
        if ($null -ne $command) {
            return $command.Source
        }
    }

    return $null
}

function Invoke-Native {
    param(
        [Parameter(Mandatory)]
        [string]$Command,

        [Parameter()]
        [string[]]$Arguments = @(),

        [switch]$AllowFailure,

        [scriptblock]$OutputHandler
    )

    $previousErrorAction = $ErrorActionPreference
    try {
        # Windows PowerShell 5.1 can promote native stderr to NativeCommandError.
        # Native CLI success is determined by the process exit code instead.
        $ErrorActionPreference = "Continue"
        if ($OutputHandler) {
            & $Command @Arguments 2>&1 | ForEach-Object {
                $line = $_.ToString()
                Write-Host $line
                & $OutputHandler $line
            }
        } else {
            & $Command @Arguments
        }
        $exitCode = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previousErrorAction
    }

    $script:LastNativeExitCode = $exitCode
    if (-not $AllowFailure -and $exitCode -ne 0) {
        throw "Command failed with exit code ${exitCode}: $Command $($Arguments -join ' ')"
    }
}

function Get-NodeInfo {
    $nodeCommand = Get-NativeCommand -Names @("node.exe", "node")
    if (-not $nodeCommand) {
        return $null
    }

    $versionText = (& $nodeCommand --version).Trim()
    if ($versionText -notmatch "^v(?<major>\d+)\.") {
        throw "Could not parse Node.js version: $versionText"
    }

    return [pscustomobject]@{
        Command = $nodeCommand
        Version = $versionText
        Major = [int]$Matches.major
    }
}

function Invoke-Npm {
    param(
        [Parameter(Mandatory)]
        [string]$Command,

        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    $savedEnvironment = @{}
    $proxyVariables = @("HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "NO_PROXY")
    try {
        if ($NpmNoProxy) {
            Write-Warn "Using a direct connection for this npm command only."
            foreach ($name in $proxyVariables) {
                $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
                [Environment]::SetEnvironmentVariable($name, $null, "Process")
            }
            [Environment]::SetEnvironmentVariable("NO_PROXY", "*", "Process")
            # CLI options override .npmrc without rewriting user configuration.
            $Arguments += @("--proxy=null", "--https-proxy=null", "--noproxy=*")
        }

        Invoke-Native -Command $Command -Arguments $Arguments -AllowFailure | Out-Host
        if ($script:LastNativeExitCode -ne 0) {
            if (-not $NpmNoProxy) {
                Write-Warn "If npm reports ECONNREFUSED for a proxy, start the configured proxy or rerun setup with -NpmNoProxy if direct access is allowed."
            }
            throw "npm failed with exit code $script:LastNativeExitCode. Review the network or permission error above."
        }
    } finally {
        foreach ($name in $savedEnvironment.Keys) {
            [Environment]::SetEnvironmentVariable($name, $savedEnvironment[$name], "Process")
        }
    }
}

function Ensure-Node {
    Write-Step "Checking Node.js $MinimumNodeMajor+"

    $node = Get-NodeInfo
    if ($null -ne $node -and $node.Major -ge $MinimumNodeMajor) {
        Write-Ok "Node.js $($node.Version) at $($node.Command)"
        $script:Checks.Node = $node.Version
        return
    }

    if ($CheckOnly) {
        $current = if ($null -eq $node) { "not installed" } else { $node.Version }
        throw "Node.js $MinimumNodeMajor+ is required; current state: $current"
    }

    $nvmCommand = Get-NativeCommand -Names @("nvm.exe", "nvm")
    if ($nvmCommand) {
        Write-Warn "Node.js is missing or too old. Installing the latest LTS with nvm-windows."
        Invoke-Native -Command $nvmCommand -Arguments @("install", "lts")
        Invoke-Native -Command $nvmCommand -Arguments @("use", "lts")
    } else {
        $wingetCommand = Get-NativeCommand -Names @("winget.exe", "winget")
        if (-not $wingetCommand) {
            throw "Install Node.js $MinimumNodeMajor+ from https://nodejs.org, then rerun this setup."
        }

        Write-Warn "Node.js is missing or too old. Installing the current LTS with winget."
        Invoke-Native -Command $wingetCommand -Arguments @(
            "install",
            "--id", "OpenJS.NodeJS.LTS",
            "--exact",
            "--accept-package-agreements",
            "--accept-source-agreements"
        )
    }

    Refresh-ProcessPath
    $node = Get-NodeInfo
    if ($null -eq $node -or $node.Major -lt $MinimumNodeMajor) {
        throw "Node.js was installed or switched, but this process still sees an old version. Close this window and run Setup-TripoWorkflow.cmd again."
    }

    Write-Ok "Node.js $($node.Version) is ready."
    $script:Checks.Node = $node.Version
}

function Ensure-TripoCli {
    Write-Step "Checking Tripo CLI"

    $npmCommand = Get-NativeCommand -Names @("npm.cmd", "npm.exe", "npm")
    if (-not $npmCommand) {
        throw "npm is not available after Node.js setup."
    }

    $tripoCommand = Get-NativeCommand -Names @("tripo.cmd", "tripo.exe", "tripo")
    $shouldInstall = $ForceCliUpdate -or -not $tripoCommand

    if ($shouldInstall) {
        if ($CheckOnly) {
            throw "Tripo CLI is not installed."
        }

        Write-Warn "Installing the latest Tripo CLI globally with npm."
        Invoke-Npm -Command $npmCommand -Arguments @("install", "--global", "tripo-cli@latest")
        Refresh-ProcessPath
        $tripoCommand = Get-NativeCommand -Names @("tripo.cmd", "tripo.exe", "tripo")
    }

    if (-not $tripoCommand) {
        throw "Tripo CLI installation completed, but the tripo command is not on PATH. Reopen the terminal and rerun setup."
    }

    Write-Ok "Tripo CLI found at $tripoCommand"
    $script:Checks.TripoCommand = $tripoCommand
    return $tripoCommand
}

function Write-TripoApiKeyHint {
    Write-Host ""
    Write-Host "Already have a Tripo API key? You can use it instead of browser authorization."
    Write-Host "Stop setup with Ctrl+C if it is still running, then run this in PowerShell:"
    Write-Host "  tripo.cmd login --paste"
    Write-Host "Paste your key at the CLI prompt, then rerun setup. Existing credentials will be reused."
    Write-Host "Do not put your key in chat, command arguments, scripts, or Git."
    Write-Host ""
}

function Ensure-TripoLogin {
    param(
        [Parameter(Mandatory)]
        [string]$TripoCommand
    )

    Write-Step "Checking Tripo account"
    Invoke-Native -Command $TripoCommand -Arguments @("whoami", "--json") -AllowFailure
    $whoAmIExit = $script:LastNativeExitCode
    if ($whoAmIExit -eq 0) {
        Write-Ok "Tripo account is already authenticated."
        $script:Checks.TripoAuthenticated = $true
        return
    }

    $script:Checks.TripoAuthenticated = $false
    if ($CheckOnly -or $SkipLogin) {
        Write-Warn "Tripo login is still required."
        Write-TripoApiKeyHint
        return
    }

    Write-Host "A browser authorization page and one-time code will be shown."
    $script:TripoApiKeyHintShown = $false
    try {
        Invoke-Native -Command $TripoCommand -Arguments @("login", "--region", $Region, "--yes") -OutputHandler {
            param([string]$Line)
            # Show the alternative after CLI instructions, before its approval wait ends.
            if (-not $script:TripoApiKeyHintShown -and $Line -match '4\..*Authorize') {
                Write-TripoApiKeyHint
                $script:TripoApiKeyHintShown = $true
            }
        }
    } finally {
        if (-not $script:TripoApiKeyHintShown) {
            Write-TripoApiKeyHint
        }
    }

    Invoke-Native -Command $TripoCommand -Arguments @("whoami", "--json") -AllowFailure
    $whoAmIExit = $script:LastNativeExitCode
    if ($whoAmIExit -ne 0) {
        throw "Tripo login did not complete successfully."
    }

    Write-Ok "Tripo login completed for region '$Region'."
    $script:Checks.TripoAuthenticated = $true
    $script:Checks.TripoRegion = $Region
}

function Test-TripoHealth {
    param(
        [Parameter(Mandatory)]
        [string]$TripoCommand
    )

    if ($SkipDoctor) {
        Write-Warn "Skipped tripo doctor."
        $script:Checks.TripoDoctor = "skipped"
        return
    }

    Write-Step "Running Tripo health check"
    Invoke-Native -Command $TripoCommand -Arguments @("doctor") -AllowFailure
    $doctorExit = $script:LastNativeExitCode
    if ($doctorExit -ne 0) {
        $script:Checks.TripoDoctor = "failed"
        throw "tripo doctor failed with exit code $doctorExit. Review the output above."
    }

    Write-Ok "Tripo network, account, and balance checks passed."
    $script:Checks.TripoDoctor = "passed"
}

function Test-Repository {
    Write-Step "Checking BattleTalent project"

    $projectVersionPath = Join-Path $RepoRoot "ModProj\ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path -LiteralPath $projectVersionPath)) {
        throw "Unity project was not found at $projectVersionPath"
    }

    $projectVersionLine = Get-Content -LiteralPath $projectVersionPath |
        Where-Object { $_ -like "m_EditorVersion:*" } |
        Select-Object -First 1
    $unityVersion = ($projectVersionLine -split ":", 2)[1].Trim()

    $gitCommand = Get-NativeCommand -Names @("git.exe", "git")
    if (-not $gitCommand) {
        Write-Warn "Git was not found. Cloning and syncing the workflow will not work until Git is installed."
        $script:Checks.Git = "missing"
    } else {
        $gitVersion = (& $gitCommand --version).Trim()
        Write-Ok $gitVersion
        $script:Checks.Git = $gitVersion
    }

    Write-Ok "Unity project detected; expected editor version is $unityVersion."
    $script:Checks.UnityVersion = $unityVersion
    $script:Checks.Repository = $RepoRoot
}

function Test-Codex {
    Write-Step "Checking Codex"

    $codexCommand = Get-NativeCommand -Names @("codex.exe", "codex")
    if (-not $codexCommand) {
        Write-Warn "Codex CLI is not on PATH. This does not check whether the desktop app is installed; you can use the app by opening this repository."
        $script:Checks.Codex = "not found on PATH"
        return
    }

    Write-Ok "Codex detected at $codexCommand"
    $script:Checks.Codex = $codexCommand
}

function Save-Status {
    if (-not (Test-Path -LiteralPath $StateDirectory)) {
        New-Item -ItemType Directory -Path $StateDirectory | Out-Null
    }

    $status = [ordered]@{
        setupVersion = 1
        checkedAt = (Get-Date).ToString("o")
        checkOnly = [bool]$CheckOnly
        checks = $script:Checks
    }

    $status | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $StatusPath -Encoding UTF8
    Write-Ok "Local status written to $StatusPath"
}

try {
    Set-Location -LiteralPath $RepoRoot
    Write-Host "Tripo + BattleTalent workflow setup" -ForegroundColor White
    Write-Host "Repository: $RepoRoot"

    Test-Repository
    Test-Codex
    Ensure-Node
    $tripoCommand = Ensure-TripoCli
    Ensure-TripoLogin -TripoCommand $tripoCommand
    Test-TripoHealth -TripoCommand $tripoCommand
    Save-Status

    Write-Host ""
    Write-Host "Workflow is ready." -ForegroundColor Green
    Write-Host "Open this repository in Codex and try:"
    Write-Host '  Help me define the asset purpose and mod category, then show a concept image for approval before using Tripo.'
    exit 0
} catch {
    Write-Host ""
    Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    try {
        Save-Status
    } catch {
        Write-Warn "Could not write local setup status."
    }
    exit 1
}
