$ErrorActionPreference = "Stop"
Write-Host "================ [CadIME Modern 统一沙箱编译引擎 2025/2026] ==============" -ForegroundColor Cyan

# 1. 锁死编译核心与输出路径
$cscPath = "C:\Program Files\dotnet\sdk\8.0.422\Roslyn\bincore\csc.dll"
$releaseDir = "E:\Soft\CAD\D\CADIme\release"
$hostOut = "$releaseDir\CADIME.2025.dll"

if (!(Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir | Out-Null }

# ==============================================================================
# 【核心修正】：前置物理强行消杀，精准清除原有的旧 DLL 成果物，消灭坏缓存
# ==============================================================================
if (Test-Path $hostOut) {
    Write-Host ">>> 检测到旧版成果物存在，正在强行将其彻底铲除..." -ForegroundColor DarkYellow
    Remove-Item -Path $hostOut -Force -ErrorAction SilentlyContinue
}

# 搜寻本地 .NET 8 核心运行时作为引用底座
$net8App = "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.*"
$net8AppPath = (Get-ChildItem $net8App | Select-Object -Last 1).FullName
$net8Refs = Get-ChildItem "$net8AppPath\*.dll" | Where-Object { $_.Name -notmatch "Native|clr|jit|gc|msquic|mscor|host" } | ForEach-Object { "/reference:`"$($_.FullName)`"" }

# 精准绑定您指定的全局外部依赖沙箱路径
$privateLibDir = "E:\Soft\CAD\D\CADIme\lib\lib2026"
if (!(Test-Path $privateLibDir)) {
    Write-Host "[错误] 未检测到指定的库文件夹: $privateLibDir" -ForegroundColor Red
    exit
}
Write-Host "已成功挂载统一沙箱 SDK 依赖路径: $privateLibDir" -ForegroundColor Green
$cadRefs = @("/reference:`"$privateLibDir\accoremgd.dll`"", "/reference:`"$privateLibDir\acdbmgd.dll`"", "/reference:`"$privateLibDir\acmgd.dll`"")

# 2. 全量收集代码文件（完美抓取底层及共用业务文件）
Write-Host ">>> 正在进行跨层级代码文件雷达检索..." -ForegroundColor Yellow
$allSources = @()
$allSources += Get-ChildItem ".\src\Abstractions\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Models\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Common\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Engine\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Windows\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Hosts\Host.Net8\*.cs" | ForEach-Object { $_.FullName }

# 终极复用：将 Host.Net10 中除入口外的 4 个共用业务支撑组件强行并流复用
$allSources += Get-ChildItem ".\src\Hosts\Host.Net10\AppMain.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Hosts\Host.Net10\SimpleLogger.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Hosts\Host.Net10\JsonConfigurationProvider.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Hosts\Host.Net10\ActionExecutor.cs" | ForEach-Object { $_.FullName }

# 3. 执行最终合体大编译
Write-Host ">>> 正在执行终极单体合体打包编译..." -ForegroundColor Yellow
dotnet exec $cscPath /target:library /nullable:enable /define:NETCOREAPP /out:"$hostOut" $net8Refs $cadRefs $allSources

if (Test-Path $hostOut) {
    Write-Host "[构建成功] 全逻辑独立单体版 CADIME.25.0.dll 已经成功诞生！" -ForegroundColor Green
    Write-Host "新版成果物已被精准集结至: $hostOut" -ForegroundColor Green
}
Write-Host "=======================================================" -ForegroundColor Cyan
