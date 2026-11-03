# FilesCom examples

Each script is a complete task, written with only the FilesCom.PowerShell module's own commands and standard PowerShell,
and runs wherever the module does. The module installs them in the `examples` folder beside itself:

```powershell
$examples = Join-Path (Get-Module -ListAvailable FilesCom.PowerShell | Select-Object -First 1).ModuleBase 'examples'
Get-Help (Join-Path $examples 'Export-UserAccessInventory.ps1') -Full
```

The module's tests run every script, from the installed package, against a stand-in Files.com site.

| Script | Task |
| --- | --- |
| `Export-UserAccessInventory.ps1` | Export every user with their groups and folder permissions to CSV, across all pages, writing the file only when the export completes. |
| `Add-FilesProjectMember.ps1` | Provision users, folders and permissions from a CSV, idempotently, with a `-WhatIf` preview that contacts no one and per-row failure reporting. |
| `Send-FilesNightlyExport.ps1` | Upload a folder unattended within one shared budget, confirm an upload whose completion is uncertain only by checksum, and report the rest as uncertain without uploading again. |
| `Send-FilesLiveExport.ps1` | Stream a program's output, such as a database export, to Files.com within a declared size, with parts kept in temporary files, and publish it under its final name only when the program succeeds. |
| `Export-FilesUploadDiagnostics.ps1` | Upload a file with its diagnostics captured and export what it decided and measured, such as throttling pauses, retried parts and part sizes, to a JSON report that holds only named fields and can be shared with support. |
| `Set-FilesSiteBranding.ps1` | Apply a site's logo, icons, login page background and bundle watermark from a folder of images in one change, each sent with its media type, with a `-WhatIf` preview that reads no image. |

The scripts are scripts: where the execution policy allows none, as under Windows' default `Restricted`, read them as
examples or run them once a policy such as `RemoteSigned` allows local scripts. Importing the module itself runs no
script.

Run `Get-Help <path to script> -Full` for each script's parameters and examples.
