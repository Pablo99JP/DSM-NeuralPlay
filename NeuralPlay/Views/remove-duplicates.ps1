$files = @(
    "Equipo\Edit.cshtml",
    "MiembroEquipo\Edit.cshtml"
)

foreach ($file in $files) {
    $path = "c:\Users\pablo\Desktop\universidad\DSM\DSM-NeuralPlay\NeuralPlay\Views\$file"
    $content = Get-Content $path -Raw
    $content = $content -replace '    \.form-group \{ margin-bottom: \d+px; \}[\r\n]+', ''
    $content | Set-Content $path -NoNewline
}
Write-Host "Archivos procesados" -ForegroundColor Green
