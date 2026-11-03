<#
.SYNOPSIS
Applies a site's logo, icons, login page background and bundle watermark from a folder of images, in one change.

.DESCRIPTION
Looks in a folder for images named for what they brand, with any of the extensions .png, .jpg, .jpeg, .gif, .svg,
.ico or .webp:

    logo               the site's logo
    icon16, icon32, icon48, icon128
                       the site's icons at those sizes
    login-background   the login page's background image
    bundle-watermark   the watermark of shared bundles

Every image found is sent in one request, with the media type its extension names, so the site changes all at once or
not at all. What has no image in the folder keeps its current branding. Two images for one purpose, such as logo.png and
logo.svg, or one with another extension, stop the script before anything is sent.

Changing site settings asks for confirmation by default: answer the question, or pass -Confirm:$false to apply the
branding unattended. -WhatIf lists the images that would be sent without reading them or contacting Files.com. The
script runs on PowerShell 7.4 or later.

.EXAMPLE
./Set-FilesSiteBranding.ps1 -Path ./branding -WhatIf

.EXAMPLE
./Set-FilesSiteBranding.ps1 -Path ./branding -Confirm:$false
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    # The folder of branding images.
    [Parameter(Mandatory)]
    [string] $Path,

    # The connection to use; defaults to the current default connection.
    [FilesCom.PowerShell.FilesConnection] $Connection
)

# Each image's name, and the Set-FilesSite parameter it is sent as.
$purposes = [ordered]@{
    'logo' = 'LogoFile'
    'icon16' = 'Icon16File'
    'icon32' = 'Icon32File'
    'icon48' = 'Icon48File'
    'icon128' = 'Icon128File'
    'login-background' = 'LoginPageBackgroundImageFile'
    'bundle-watermark' = 'BundleWatermarkAttachmentFile'
}
$mediaTypes = @{ '.png' = 'image/png'; '.jpg' = 'image/jpeg'; '.jpeg' = 'image/jpeg'; '.gif' = 'image/gif'; '.svg' = 'image/svg+xml'; '.ico' = 'image/x-icon'; '.webp' = 'image/webp' }

$folder = Get-Item -LiteralPath $Path -ErrorAction Stop
if (-not $folder.PSIsContainer) {
    throw "$Path is a file; give the folder of branding images."
}
$files = @(Get-ChildItem -LiteralPath $folder.FullName -File)

# Every image is checked before any is sent.
$change = @{}
$images = foreach ($purpose in $purposes.Keys) {
    $found = @($files | Where-Object BaseName -EQ $purpose)
    if ($found.Count -gt 1) {
        throw "$($folder.FullName) has more than one $purpose image ($($found.Name -join ', ')); keep the one to use."
    }
    if ($found.Count -eq 1) {
        $file = $found[0]
        $mediaType = $mediaTypes[$file.Extension.ToLowerInvariant()]
        if (-not $mediaType) {
            throw "$($file.Name) is not an image this script sends; save it as one of $(($mediaTypes.Keys | Sort-Object) -join ', ')."
        }
        $change[$purposes[$purpose]] = New-FilesAttachment -LiteralPath $file.FullName -ContentType $mediaType
        [pscustomobject]@{ Image = $purpose; File = $file.Name; MediaType = $mediaType; Length = $file.Length }
    }
}
if ($change.Count -eq 0) {
    throw "$($folder.FullName) has no branding image: name each one $($purposes.Keys -join ', '), with an image extension."
}
if ($Connection) {
    $change.Connection = $Connection
}

# Set-FilesSite follows this script's -WhatIf and -Confirm: under -WhatIf it reads no image and sends nothing.
$site = Set-FilesSite @change -ErrorAction Stop
$status = if ($WhatIfPreference) { 'Previewed' } elseif ($site) { 'Applied' } else { 'Skipped' }
foreach ($image in $images) {
    $image | Add-Member -NotePropertyName Status -NotePropertyValue $status -PassThru
}
