<#
.SYNOPSIS
Exports every user of a Files.com site with their groups and folder permissions to CSV.

.DESCRIPTION
Lists all users, group memberships and permissions, one page at a time, and writes one row per user with the groups
they belong to and the permissions they hold directly or through a group. Out-FilesLocalFile writes the CSV in one
complete step: if any page cannot be read, the export stops with an error, the file at -Path stays as it was, and no
partial file is left behind. A file already at -Path is replaced only when the export completes. The script runs on
PowerShell 7.4 or later.

.EXAMPLE
Connect-FilesAccount -ApiKey (Get-Secret -Name FilesComApiKey) -BaseUrl https://mysite.files.com
./Export-UserAccessInventory.ps1 -Path ./user-access.csv
#>
[CmdletBinding()]
param(
    # The CSV file to write.
    [Parameter(Mandatory)]
    [string] $Path,

    # The connection to use; defaults to the current default connection.
    [FilesCom.PowerShell.FilesConnection] $Connection
)

$ErrorActionPreference = 'Stop'
$scope = @{}
if ($Connection) {
    $scope.Connection = $Connection
}

$groupsByUser = @{}
foreach ($membership in Get-FilesGroupUser @scope) {
    $groupsByUser[$membership.UserId] += @($membership.GroupName)
}
$permissionsByUser = @{}
$permissionsByGroup = @{}
foreach ($permission in Get-FilesPermission @scope) {
    $grant = "$($permission.Path):$($permission.PermissionType)"
    if ($permission.UserId) {
        $permissionsByUser[$permission.UserId] += @($grant)
    }
    elseif ($permission.GroupId) {
        $permissionsByGroup[$permission.GroupName] += @($grant)
    }
}

Get-FilesUser @scope | ForEach-Object {
    $groups = @($groupsByUser[$_.Id] | Where-Object { $_ })
    $inherited = foreach ($group in $groups) { $permissionsByGroup[$group] }
    [pscustomobject]@{
        Id = $_.Id
        Username = $_.Username
        Name = $_.Name
        Email = $_.Email
        SiteAdmin = $_.SiteAdmin
        Disabled = $_.Disabled
        LastLoginAt = $_.LastLoginAt
        Groups = $groups -join '; '
        Permissions = @(@($permissionsByUser[$_.Id]) + @($inherited) | Where-Object { $_ } | Sort-Object -Unique) -join '; '
    }
} | ConvertTo-Csv -NoTypeInformation | Out-FilesLocalFile -LiteralPath $Path -Force
