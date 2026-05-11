# =============================================
# 迁移 Migrations 文件到 CpmServer.Core
# =============================================

$baseDir = "C:\Users\adm\Qsync\AI\CPM_SrcCode"
$sourceDir = Join-Path $baseDir "CpmServer\Migrations"
$targetDir = Join-Path $baseDir "CpmServer.Core\Migrations"

# 1. 删除旧的重复迁移（同名 InitialBaseline）
$duplicates = @(
    "20260430061021_InitialBaseline.cs",
    "20260430061021_InitialBaseline.Designer.cs"
)

foreach ($file in $duplicates) {
    $path = Join-Path $sourceDir $file
    if (Test-Path $path) {
        Remove-Item $path -Force
        Write-Host "Deleted duplicate: $file" -ForegroundColor Yellow
    }
}

# 2. 确保目标目录存在
if (!(Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    Write-Host "Created: $targetDir" -ForegroundColor Green
}

# 3. 移动剩余迁移文件
$filesToMove = @(
    "20260501000000_AddMfgSiteAndFields.cs",
    "20260501000000_AddMfgSiteAndFields.Designer.cs",
    "20260501000001_RemoveMfgCategory.cs",
    "20260508054721_InitialBaseline.cs",
    "20260508054721_InitialBaseline.Designer.cs",
    "20260508054804_AddApprovalCondition.cs",
    "20260508054804_AddApprovalCondition.Designer.cs",
    "20260508084026_AddAvatarUrlToUser.cs",
    "20260508084026_AddAvatarUrlToUser.Designer.cs",
    "CpmDbContextModelSnapshot.cs"
)

foreach ($file in $filesToMove) {
    $sourcePath = Join-Path $sourceDir $file
    $targetPath = Join-Path $targetDir $file
    if (Test-Path $sourcePath) {
        Move-Item $sourcePath $targetPath -Force
        Write-Host "Moved: $file" -ForegroundColor Green
    } else {
        Write-Host "Not found (may already be moved): $file" -ForegroundColor Gray
    }
}

# 4. 删除空源目录
if (Test-Path $sourceDir) {
    $remaining = Get-ChildItem $sourceDir -ErrorAction SilentlyContinue
    if (!$remaining) {
        Remove-Item $sourceDir -Force
        Write-Host "Removed empty: $sourceDir" -ForegroundColor Cyan
    } else {
        Write-Host "WARNING: Source dir still has files:" -ForegroundColor Red
        $remaining | ForEach-Object { Write-Host "  $($_.Name)" -ForegroundColor Red }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor White
Write-Host "Migration files moved successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor White
Write-Host ""
Write-Host "Next step: Run the following command to verify build:" -ForegroundColor Cyan
Write-Host "  dotnet build CpmServer/CpmServer.slnx" -ForegroundColor Yellow
