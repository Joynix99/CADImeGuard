$ErrorActionPreference = "Stop"
Write-Host "================ [CadIME Modern 强攻全编译引擎 2027] ================" -ForegroundColor Cyan

# 1. 锁死编译核心与输出路径
$cscPath = "C:\Program Files\dotnet\sdk\8.0.422\Roslyn\bincore\csc.dll"
$releaseDir = "E:\Soft\CAD\D\CADIme\release"
$hostOut = "$releaseDir\CADIME.26.0.dll"

if (!(Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir | Out-Null }

# ==============================================================================
# 前置物理强行消杀，精准清除原有的旧 DLL 成果物，消灭坏缓存
# ==============================================================================
if (Test-Path $hostOut) {
    Write-Host ">>> 检测到旧版成果物存在，正在强行将其彻底铲除..." -ForegroundColor DarkYellow
    Remove-Item -Path $hostOut -Force -ErrorAction SilentlyContinue
}

# 搜寻本地 100% 契合 Win10 内核的 .NET 8 核心运行时作为引用底座
$net8App = "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.*"
$net8AppPath = (Get-ChildItem $net8App | Select-Object -Last 1).FullName
$net8Refs = Get-ChildItem "$net8AppPath\*.dll" | Where-Object { $_.Name -notmatch "Native|clr|jit|gc|msquic|mscor|host" } | ForEach-Object { "/reference:`"$($_.FullName)`"" }

# AutoCAD 2027 物理 SDK 引用绑定
$cad2027Dir = "C:\Program Files\Autodesk\AutoCAD 2027"
if (!(Test-Path $cad2027Dir)) {
    Write-Host "[错误] 未检测到 AutoCAD 2027 安装目录，请检查路径！" -ForegroundColor Red
    exit
}
$cadRefs = @("/reference:`"$cad2027Dir\accoremgd.dll`"", "/reference:`"$cad2027Dir\acdbmgd.dll`"", "/reference:`"$cad2027Dir\acmgd.dll`"")

# 2. 全量收集 7 子项目源码文件进行大融合
Write-Host ">>> 正在进行跨层级代码文件雷达检索..." -ForegroundColor Yellow
$allSources = @()
$allSources += Get-ChildItem ".\src\Abstractions\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Models\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Common\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Engine\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Windows\*.cs" | ForEach-Object { $_.FullName }
$allSources += Get-ChildItem ".\src\Hosts\Host.Net10\*.cs" | ForEach-Object { $_.FullName }

# 3. 执行最终编译
Write-Host ">>> 正在执行终极单体合体打包编译..." -ForegroundColor Yellow
dotnet exec $cscPath /target:library /nullable:enable /define:NETCOREAPP /out:"$hostOut" $net8Refs $cadRefs $allSources

if (Test-Path $hostOut) {
    Write-Host "[构建成功] 全逻辑通用单体版 CADIME.26.0.dll 已经诞生！" -ForegroundColor Green
    Write-Host "新版成果物已被精准集结至: $hostOut" -ForegroundColor Green
}
Write-Host "=======================================================" -ForegroundColor Cyan
