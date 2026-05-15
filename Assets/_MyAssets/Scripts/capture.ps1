Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap $bounds.width, $bounds.height
$graphics = [System.Drawing.Graphics]::FromImage($bmp)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.size)
$bmp.Save('c:\Users\Esdra\Unity\Lab4_JalAmeBomDia\Assets\_MyAssets\Scripts\screen_capture.png')
$graphics.Dispose()
$bmp.Dispose()
Write-Output 'Screenshot taken successfully.'
