$deleteFiles = @(
    "SE_Counter_Tick_01.wav",
    "SE_Counter_Tick_02.wav",
    "SE_EnemyShot_02_Pik.wav",
    "SE_EnemyShot_03_Ko.wav",
    "SE_EnemyShot_04_DryAir.wav",
    "SE_Enemy_Escape_Insect_02_FastSkitter.wav",
    "SE_Enemy_Escape_Insect_03_ChitinScuttle.wav",
    "SE_Enemy_Escape_Insect_04_DryRustle.wav",
    "SE_GetExp_03_Chirp.wav",
    "SE_Hit_Enemy_02_HeavyThud.wav",
    "SE_Shot_Touhou_02_Su.wav",
    "SE_Shot_Touhou_03_Bas.wav",
    "SE_Shot_Touhou_04_Pashu.wav",
    "SE_Timeline_Skip_Charge_01.wav",
    "SE_Timeline_Skip_Charge_02.wav",
    "SE_Timeline_Skip_Mech_02_PneumaticSeek.wav",
    "SE_Timeline_Skip_Mech_03_RelayCharge.wav",
    "SE_Timeline_Skip_Mech_04_ClockworkFast.wav",
    "SE_WeaponSwitch_3.wav"
)

$seDir = Resolve-Path "../resources/SE"
$deletedCount = 0

foreach ($fileName in $deleteFiles) {
    $filePath = Join-Path $seDir $fileName
    $metaPath = $filePath + ".meta"

    if (Test-Path $filePath) {
        Remove-Item $filePath -Force
        Write-Host "Deleted: $fileName"
        $deletedCount++
    }
    if (Test-Path $metaPath) {
        Remove-Item $metaPath -Force
        Write-Host "Deleted meta: $fileName.meta"
    }
}

Write-Host "Completed. Total deleted: $deletedCount files."
