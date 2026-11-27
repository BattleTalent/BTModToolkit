Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$setupPath = Join-Path (Split-Path -Parent $PSScriptRoot) "Setup-TripoWorkflow.ps1"
$parseErrors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile(
    $setupPath, [ref]$null, [ref]$parseErrors
)
if ($parseErrors.Count -gt 0) {
    throw "Setup script failed syntax validation."
}

# Load functions without running installation, authorization, or the main block.
foreach ($statement in $ast.EndBlock.Statements) {
    if ($statement -is [System.Management.Automation.Language.FunctionDefinitionAst]) {
        . ([scriptblock]::Create($statement.Extent.Text))
    }
}
$nativeImplementation = (Get-Item Function:Invoke-Native).ScriptBlock

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) {
        throw $Message
    }
}

$variables = @("HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "NO_PROXY")
$originalEnvironment = @{}
foreach ($name in $variables) {
    $originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
}

try {
    foreach ($name in @("HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY")) {
        [Environment]::SetEnvironmentVariable($name, "http://127.0.0.1:7890", "Process")
    }
    [Environment]::SetEnvironmentVariable("NO_PROXY", $null, "Process")

    function Invoke-Native {
        param($Command, $Arguments, [switch]$AllowFailure)
        $script:CapturedArguments = $Arguments
        $script:CapturedProxy = [Environment]::GetEnvironmentVariable("HTTPS_PROXY", "Process")
        $script:CapturedNoProxy = [Environment]::GetEnvironmentVariable("NO_PROXY", "Process")
        $script:LastNativeExitCode = $script:MockExitCode
        Write-Output "Mock npm installation output"
    }

    $NpmNoProxy = $true
    $script:MockExitCode = 0
    $result = @(Invoke-Npm -Command "npm.cmd" -Arguments @("install", "--global", "tripo-cli@latest"))
    Assert-True ($result.Count -eq 0) "npm output leaked into the caller's return value."
    Assert-True ($script:CapturedArguments -contains "--proxy=null") "npm proxy override missing."
    Assert-True ($script:CapturedArguments -contains "--https-proxy=null") "npm HTTPS proxy override missing."
    Assert-True ([string]::IsNullOrEmpty($script:CapturedProxy)) "Proxy environment was not cleared."
    Assert-True ($script:CapturedNoProxy -eq "*") "Direct-access environment was not applied."
    Assert-True ($env:HTTPS_PROXY -eq "http://127.0.0.1:7890") "Proxy was not restored after success."
    Assert-True ([string]::IsNullOrEmpty($env:NO_PROXY)) "Originally absent NO_PROXY was not restored."

    $script:MockExitCode = 1
    $failed = $false
    try {
        Invoke-Npm -Command "npm.cmd" -Arguments @("install")
    } catch {
        $failed = $true
    }
    Assert-True $failed "Failed npm command was not rejected."
    Assert-True ($env:HTTPS_PROXY -eq "http://127.0.0.1:7890") "Proxy was not restored after failure."
    Assert-True ([string]::IsNullOrEmpty($env:NO_PROXY)) "NO_PROXY was not restored after failure."

    $NpmNoProxy = $false
    $script:MockExitCode = 0
    Invoke-Npm -Command "npm.cmd" -Arguments @("install")
    Assert-True ($script:CapturedProxy -eq "http://127.0.0.1:7890") "Default mode changed the proxy."
    Assert-True ($script:CapturedArguments.Count -eq 1) "Default mode added proxy overrides."

    function Get-NativeCommand {
        param([string[]]$Names)
        if ($Names[0] -eq "npm.cmd") { return "npm.cmd" }
        $script:TripoLookups++
        if ($script:TripoLookups -gt 1) { return "C:\Mock\tripo.cmd" }
        return $null
    }
    function Refresh-ProcessPath {}
    $CheckOnly = $false
    $ForceCliUpdate = $false
    $script:Checks = [ordered]@{}
    $script:TripoLookups = 0
    $paths = @(Ensure-TripoCli)
    Assert-True ($paths.Count -eq 1) "CLI discovery returned extra installation output."
    Assert-True ($paths[0] -eq "C:\Mock\tripo.cmd") "CLI path was not returned correctly."

    $regionParameter = $ast.ParamBlock.Parameters |
        Where-Object { $_.Name.VariablePath.UserPath -eq "Region" }
    $defaultRegion = $regionParameter.DefaultValue.SafeGetValue()
    Assert-True ($defaultRegion -eq "ov") "Default account region is not international."

    function Read-Host {
        throw "Setup must not prompt for an account region."
    }
    function Invoke-Native {
        param($Command, $Arguments, [switch]$AllowFailure, [scriptblock]$OutputHandler)
        $script:LoginCalls.Add($Arguments) | Out-Null
        if ($Arguments[0] -eq "login") {
            $line = '  4. Click "Authorize". The code expires in 15 minutes'
            Write-Host $line
            & $OutputHandler $line
            Assert-True $script:TripoApiKeyHintShown "API key hint was not shown before the approval wait."
            Write-Host "Mock approval wait completed"
        }
        $script:LastNativeExitCode = if ($script:LoginCalls.Count -eq 1) { 1 } else { 0 }
    }
    $SkipLogin = $false
    foreach ($Region in @($defaultRegion, "cn")) {
        $script:LoginCalls = [System.Collections.Generic.List[object]]::new()
        $script:Checks = [ordered]@{}
        $loginOutput = @(Ensure-TripoLogin -TripoCommand "tripo.cmd" 6>&1)
        $loginText = $loginOutput -join "`n"
        Assert-True ($loginText -match "tripo\.cmd login --paste") "API key instructions were not displayed."
        Assert-True ($loginText.IndexOf('4. Click "Authorize"') -lt $loginText.IndexOf("Already have a Tripo API key?")) "API key hint preceded browser instructions."
        Assert-True ($loginText.IndexOf("Already have a Tripo API key?") -lt $loginText.IndexOf("Mock approval wait completed")) "API key hint was delayed until after login."
        Assert-True ($script:LoginCalls.Count -eq 3) "Login did not follow the expected command sequence."
        Assert-True (($script:LoginCalls[1] -join " ") -eq "login --region $Region --yes") "Unexpected login region."
        Assert-True ($script:Checks.TripoRegion -eq $Region) "Login status recorded the wrong region."
    }

    $CheckOnly = $true
    $script:LoginCalls = [System.Collections.Generic.List[object]]::new()
    $script:Checks = [ordered]@{}
    $loginOutput = @(Ensure-TripoLogin -TripoCommand "tripo.cmd" 6>&1)
    Assert-True (($loginOutput -join "`n") -match "tripo\.cmd login --paste") "Check-only mode omitted API key instructions."
    Assert-True ($script:LoginCalls.Count -eq 1) "Check-only mode started browser authorization."

    $node = (Get-Command node.exe).Source
    $script:StreamLines = [System.Collections.Generic.List[string]]::new()
    & $nativeImplementation -Command $node -Arguments @(
        "-e", "console.log('stdout test'); console.error('stderr test');"
    ) -OutputHandler {
        param([string]$Line)
        $script:StreamLines.Add($Line)
    }
    Assert-True ($script:LastNativeExitCode -eq 0) "Output handling lost the native exit code."
    Assert-True ($script:StreamLines.Contains("stdout test")) "Output handler missed stdout."
    Assert-True ($script:StreamLines.Contains("stderr test")) "Output handler missed stderr."

    Write-Output "PASS: npm proxy handling, CLI output isolation, login region, streamed output, and API key hint ordering."
} finally {
    foreach ($name in $originalEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $originalEnvironment[$name], "Process")
    }
}
