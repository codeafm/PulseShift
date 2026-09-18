$ErrorActionPreference = 'Stop'
# Compile the real policy, result gate and interstitial lifecycle against test doubles.
# No Unity process, network requests or paid impressions are used by these tests.
$project = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$sources = @(
    (Join-Path $project 'Assets/Lobby/PulseProfile.cs'),
    (Join-Path $project 'Assets/Lobby/PulseAds.Interstitial.cs'),
    (Join-Path $project 'Assets/Campaign/CampaignAds.cs'),
    (Join-Path $PSScriptRoot 'TestDoubles.cs'),
    (Join-Path $PSScriptRoot 'RegressionChecks.cs')
)
Add-Type -Path $sources
$count = [AdsRegressionChecks]::Run()
Write-Output "PASS: $count ad regression checks. Native Android/iOS SDK tests still required."
