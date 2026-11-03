<#
.SYNOPSIS
Provisions project members from a CSV file: their user, their project folder and their permission on it.

.DESCRIPTION
Reads rows with Username, Email, Folder and Permission columns. For each row it creates the user unless a user with
that username exists, creates the folder with any missing parents unless it exists, and grants the permission unless
the user has it on that folder. Each row is independent: a row that fails is reported and the next row is provisioned.
Run it again after fixing a failed row; what an earlier run provisioned is reused, not created twice.

Creating a user and granting a permission are security changes, which ask for confirmation by default: answer each
question, or pass -Confirm:$false to provision unattended.

-WhatIf previews every row without contacting Files.com at all. A preview therefore cannot know which users, folders
and permissions already exist: it lists every change each row may need, and its Note says so. The script runs on
PowerShell 7.4 or later.

.EXAMPLE
./Add-FilesProjectMember.ps1 -Path ./members.csv -WhatIf

.EXAMPLE
./Add-FilesProjectMember.ps1 -Path ./members.csv -Confirm:$false | Where-Object Status -NE 'Provisioned'
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    # The CSV file of members.
    [Parameter(Mandatory)]
    [string] $Path,

    # The connection to use; defaults to the current default connection.
    [FilesCom.PowerShell.FilesConnection] $Connection
)

$scope = @{}
if ($Connection) {
    $scope.Connection = $Connection
}

foreach ($row in Import-Csv -LiteralPath $Path) {
    if ($WhatIfPreference) {
        # A local plan: nothing is read from or sent to Files.com.
        $PSCmdlet.ShouldProcess("user $($row.Username)", 'Create the user unless it exists') | Out-Null
        $PSCmdlet.ShouldProcess("folder $($row.Folder)", 'Create the folder and any missing parents unless it exists') | Out-Null
        $PSCmdlet.ShouldProcess("$($row.Folder) for $($row.Username)", "Grant $($row.Permission) permission unless the user has it") | Out-Null
        [pscustomobject]@{ Username = $row.Username; Folder = $row.Folder; Status = 'Previewed'; Error = $null; Note = 'Planned without contacting Files.com: whether the user, folder and permission already exist was not checked.' }
        continue
    }
    try {
        $user = Get-FilesUser @scope -Filter @{ username = $row.Username } -ErrorAction Stop | Select-Object -First 1
        if (-not $user) {
            $user = New-FilesUser @scope -Username $row.Username -Email $row.Email -ErrorAction Stop
        }
        try {
            New-FilesFolder @scope -Path $row.Folder -MkdirParents $true -ErrorAction Stop | Out-Null
        }
        catch {
            # A folder that already exists, such as one an earlier run created, is used as it is.
            if ($_.Exception.ErrorType -ne 'processing-failure/destination-exists') {
                throw
            }
        }
        if ($user) {
            $granted = Get-FilesPermission @scope -Path $row.Folder -UserId $user.Id -ErrorAction Stop |
                Where-Object { $_.Path -eq $row.Folder -and $_.PermissionType -eq $row.Permission }
            if (-not $granted) {
                New-FilesPermission @scope -Path $row.Folder -UserId $user.Id -Permission $row.Permission -ErrorAction Stop | Out-Null
            }
            $status = 'Provisioned'
        }
        else {
            # The user was not created because its confirmation was declined.
            $status = 'Skipped'
        }
        [pscustomobject]@{ Username = $row.Username; Folder = $row.Folder; Status = $status; Error = $null; Note = $null }
    }
    catch {
        [pscustomobject]@{ Username = $row.Username; Folder = $row.Folder; Status = 'Failed'; Error = "$($_.FullyQualifiedErrorId): $($_.Exception.Message)"; Note = $null }
    }
}
