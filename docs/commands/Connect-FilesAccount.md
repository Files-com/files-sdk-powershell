---
document type: cmdlet
external help file: FilesCom.PowerShell.dll-Help.xml
HelpUri: ''
Locale: en-US
Module Name: FilesCom.PowerShell
PlatyPS schema version: 2024-05-01
title: Connect-FilesAccount
---

# Connect-FilesAccount

## SYNOPSIS

Connects to a Files.com site and makes the connection the default of the current runspace.

## SYNTAX

### ApiKey (Default)

```
Connect-FilesAccount -ApiKey <securestring> [-BaseUrl <string>] [-WorkspaceId <long>]
 [-Language <string>] [-MaxNetworkRetries <int>] [-RetryDelaySeconds <double>]
 [-MaxRetryDelaySeconds <double>] [-ConnectTimeoutSeconds <int>] [-ReadTimeoutSeconds <int>]
 [-WriteTimeoutSeconds <int>] [-Proxy <uri>] [-ProxyCredential <pscredential>]
 [-HttpMessageHandler <HttpMessageHandler>] [-DiagnosticDebug] [-LogLevel <FilesLogLevel>]
 [-RegionRelocation <FilesRegionRelocationPolicy>] [-OperationTimeout <timespan>] [-NoDefault]
 [-WhatIf] [-Confirm]
```

### Session

```
Connect-FilesAccount -SessionId <securestring> [-BaseUrl <string>] [-WorkspaceId <long>]
 [-Language <string>] [-MaxNetworkRetries <int>] [-RetryDelaySeconds <double>]
 [-MaxRetryDelaySeconds <double>] [-ConnectTimeoutSeconds <int>] [-ReadTimeoutSeconds <int>]
 [-WriteTimeoutSeconds <int>] [-Proxy <uri>] [-ProxyCredential <pscredential>]
 [-HttpMessageHandler <HttpMessageHandler>] [-DiagnosticDebug] [-LogLevel <FilesLogLevel>]
 [-RegionRelocation <FilesRegionRelocationPolicy>] [-OperationTimeout <timespan>] [-NoDefault]
 [-WhatIf] [-Confirm]
```

### Credential

```
Connect-FilesAccount -Credential <pscredential> [-OneTimePassword <string>] [-BaseUrl <string>]
 [-WorkspaceId <long>] [-Language <string>] [-MaxNetworkRetries <int>] [-RetryDelaySeconds <double>]
 [-MaxRetryDelaySeconds <double>] [-ConnectTimeoutSeconds <int>] [-ReadTimeoutSeconds <int>]
 [-WriteTimeoutSeconds <int>] [-Proxy <uri>] [-ProxyCredential <pscredential>]
 [-HttpMessageHandler <HttpMessageHandler>] [-DiagnosticDebug] [-LogLevel <FilesLogLevel>]
 [-RegionRelocation <FilesRegionRelocationPolicy>] [-OperationTimeout <timespan>] [-NoDefault]
 [-WhatIf] [-Confirm]
```

### Challenge

```
Connect-FilesAccount -Challenge <FilesSignInChallenge> -OneTimePassword <string> [-NoDefault]
 [-WhatIf] [-Confirm]
```

## ALIASES

This cmdlet has no aliases.

## DESCRIPTION

Connect-FilesAccount makes a connection to a Files.com site with an API key, an existing session ID, or a username and password. It writes the connection, a FilesConnection object that shows the site, how it authenticates and whether it is open, but never the credentials.

The new connection becomes the default of the current runspace, which FilesCom commands use when they are not given -Connection. Use -NoDefault to keep the current default, for example in scripts that work with several sites and pass -Connection to every command.

An API key or session ID connection sends nothing to Files.com until a command runs. Signing in with -Credential creates a session on Files.com that the connection owns; Disconnect-FilesAccount revokes it. API keys and session IDs you supply are never revoked by the module.

When the user has two-factor authentication and no -OneTimePassword was given, or Files.com needs a second factor another way, such as a code it sends by text message, signing in stops with the error Files.TwoFactorRequired. Its TargetObject is a FilesSignInChallenge that names the site, the user and the kinds of second factor Files.com accepts. Give it to -Challenge with the code from the user's device in -OneTimePassword to finish signing in; the finished connection owns its session like any -Credential connection. The challenge keeps the identifier of the partly completed login to itself: formatting, ConvertTo-Json and Export-Clixml never show it, and a challenge finishes one sign-in. Only one command at a time can use a challenge: another that tries while it is being used fails with Files.InvalidArgument before sending anything. When Files.com refuses the code, the same challenge can be given again with another code.

The engine retries a request that failed in a way that may be temporary, such as a connection failure, a network timeout or a server error, up to -MaxNetworkRetries times, waiting longer before each retry; a request that changes something, such as a delete or the one that completes an upload, is sent once per try and not again after a lost or failed answer. -ConnectTimeoutSeconds, -ReadTimeoutSeconds and -WriteTimeoutSeconds bound how long a connection or request may stall; a request that stalls longer is retried like other temporary failures and then fails with Files.NetworkTimeout. -OperationTimeout is separate: it bounds a whole command.

By default the connection's requests go through the system's proxy settings. -Proxy sends them through another proxy, to Files.com and to the storage addresses of uploads and downloads alike, with -ProxyCredential when the proxy asks for one. -HttpMessageHandler sends them through an HttpMessageHandler of your own, such as one with its own certificate checks; the connection borrows it and never disposes it.

When Files.com answers that the site is served from another region's address, a command moves on to that address, as -RegionRelocation allows. Automatic, the default, moves only when the answer came from the connection's own address over HTTPS whose certificate was checked as .NET does by default, and names an HTTPS address on port 443 with a public DNS name; Disabled never moves; a hashtable of origin pairs moves only between the listed origins, and allows what Automatic never does, such as another port or plain HTTP, as a development site may need. A command that moves keeps the new address for its remaining requests, and so do the objects and pages it writes: a later command given one of them with -InputObject or -Continuation goes there too, unless it is given -WorkspaceScope or -NoWorkspaceScope. The connection's own address is unchanged, and every other command starts there. Moving takes one of the command's retries. A command that does not move fails with Files.RegionMismatch; its exception's RequestedOrigin names the site's address and RegionRelocationRefusal says why.

The connection's requests to Files.com name the module and its version, then the engine and its version, in their User-Agent, such as FilesCom-PowerShell/1.0.0 Files.com DOTNET SDK v2.0.0.

With -Verbose, commands that use the connection show the engine's messages about their own requests, such as each request sent and each retry. URLs in them keep their site and path but lose any credentials and query. A connection made with -DiagnosticDebug also shows, with -Debug, the engine's DEBUG messages of its own sign-in and of every command that uses it. DEBUG messages may contain sensitive information: URLs, request and response bodies, and values such as the password and one-time code a sign-in sends, the session ID or partial-login identifier it receives, and passwords, API keys or session IDs other commands send or receive. Neither what they include nor any redaction is guaranteed. Restrict access to them and review them before sharing. Other connections' commands and the Verbose stream never show them.

Connections exist only in the current PowerShell session: nothing is saved, and no profile, environment variable or configuration file is read. Store credentials in a secret vault, such as with Microsoft.PowerShell.SecretManagement, and pass them in.

## EXAMPLES

### Example 1: Connect with an API key from a secret vault

```powershell
$apiKey = Get-Secret -Name FilesComApiKey
Connect-FilesAccount -ApiKey $apiKey -BaseUrl https://mysite.files.com
```

Connects with an API key stored as a SecureString and makes the connection the default.

### Example 2: Sign in with a username and password

```powershell
Connect-FilesAccount -Credential (Get-Credential) -BaseUrl https://mysite.files.com -OneTimePassword 123456
```

Signs in, sending the current two-factor authentication code, and creates a session the connection owns.

### Example 3: Finish a two-factor sign-in

```powershell
try {
    Connect-FilesAccount -Credential (Get-Credential) -BaseUrl https://mysite.files.com -ErrorAction Stop
}
catch {
    if ($_.FullyQualifiedErrorId -notlike 'Files.TwoFactorRequired*') { throw }
    $challenge = $_.TargetObject
    $code = Read-Host "Code from your $($challenge.Methods -join ' or ') device"
    Connect-FilesAccount -Challenge $challenge -OneTimePassword $code
}
```

Signs in, and when Files.com asks for a second factor, finishes the sign-in with the code the user reads from their device.

### Example 4: Work with two sites in one script

```powershell
$production = Connect-FilesAccount -ApiKey $productionKey -BaseUrl https://acme.files.com -NoDefault
$staging = Connect-FilesAccount -ApiKey $stagingKey -BaseUrl https://acme-staging.files.com -NoDefault
Get-FilesUser -Connection $production | Select-Object Username
```

Makes two connections without changing the default, and names the connection each command uses.

### Example 5: Bound how long commands may run

```powershell
Connect-FilesAccount -ApiKey $apiKey -OperationTimeout (New-TimeSpan -Minutes 10) -MaxNetworkRetries 5
```

Every command using the connection stops with Files.OperationTimeout once it has run for 10 minutes, however many pages, files or requests it has handled by then, and failed requests are retried up to 5 times.

### Example 6: Send requests through a proxy, and show retries

```powershell
$proxyCredential = Get-Credential -Message 'Proxy account'
$connection = Connect-FilesAccount -ApiKey $apiKey -Proxy http://proxy.example.com:8080 -ProxyCredential $proxyCredential -MaxNetworkRetries 5 -ReadTimeoutSeconds 120
Get-FilesUser -Connection $connection -Verbose
```

Sends every request through the proxy, waits up to two minutes for a stalled response, and shows each request and retry on the Verbose stream.

### Example 7: Keep every command at one address, or allow a development move

```powershell
$strict = Connect-FilesAccount -ApiKey $apiKey -BaseUrl https://mysite.files.com -RegionRelocation Disabled -NoDefault
$dev = Connect-FilesAccount -ApiKey $devKey -BaseUrl http://localhost:8080 -RegionRelocation @{ 'http://localhost:8080' = 'http://localhost:8081' } -NoDefault
```

The first connection's commands never move, and fail with Files.RegionMismatch naming the site's address; the second's move from the local development address to the other one, which Automatic would refuse.

## PARAMETERS

### -ApiKey

The API key to authenticate with, as a SecureString. Disconnecting never revokes it.

```yaml
Type: System.Security.SecureString
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: ApiKey
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -BaseUrl

The address of the site, such as https://mysite.files.com. Defaults to https://app.files.com, which works for API keys of any site. The address must not contain credentials, a query or a fragment.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Challenge

A sign-in that needs a second factor: the TargetObject of the Files.TwoFactorRequired error of an earlier Connect-FilesAccount -Credential. The connection is made with that sign-in's site and settings, so -BaseUrl, -WorkspaceId and the other connection settings are not given again.

```yaml
Type: FilesCom.PowerShell.FilesSignInChallenge
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Challenge
  Position: Named
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Confirm

Prompts you for confirmation before running the cmdlet.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases:
- cf
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -ConnectTimeoutSeconds

How long, in seconds, to wait to connect to a server, from 1 to 2147483. Defaults to 30. It applies on PowerShell 7 to the connections the engine makes itself, including through -Proxy. Windows PowerShell has no separate connect timeout: there a connection that cannot be made ends when -ReadTimeoutSeconds, or -WriteTimeoutSeconds for an upload part, runs out. A -HttpMessageHandler connects as it is configured.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Credential

The username and password to sign in with. Signing in creates a session that the connection owns and Disconnect-FilesAccount revokes.

```yaml
Type: System.Management.Automation.PSCredential
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Credential
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -DiagnosticDebug

Lets the engine's DEBUG messages reach the Debug stream, shown with -Debug, of this command's own sign-in and of every command that uses the connection. DEBUG messages may contain sensitive information: URLs, request and response bodies, and values such as the password and one-time code a sign-in sends, the session ID or partial-login identifier it receives, and passwords, API keys or session IDs other commands send or receive. Neither what they include nor any redaction is guaranteed. Use it only to diagnose a problem. Restrict access to these messages, and to any transcript or log that keeps them, and review them before sharing. Other connections' commands and the Verbose stream never show these messages. Without it, only the engine's ordinary messages, such as requests and retries, reach the Verbose stream, without credentials or queries.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -HttpMessageHandler

An HttpMessageHandler of your own, such as one with its own TLS or certificate settings, to send every request of the connection through, to Files.com and to the storage addresses of uploads and downloads. The connection borrows it: closing the connection never disposes it, so dispose it yourself once every connection using it is closed. It must not follow redirects itself: set AllowAutoRedirect to $false on an HttpClientHandler or SocketsHttpHandler, since the engine follows redirects without sending credentials to other sites, and a handler that follows them is refused with Files.InvalidArgument. It must not retry requests itself. It cannot be combined with -Proxy.

```yaml
Type: System.Net.Http.HttpMessageHandler
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Language

The language Files.com writes its messages in, such as de or es.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -LogLevel

The least severe engine diagnostic this connection's commands produce: Debug, Information, Warning, Error or None. By default the engine produces all of them, and a command shows its ordinary diagnostics on the Verbose stream, with -Verbose, and its DEBUG diagnostics only with -DiagnosticDebug. Warning, for example, keeps this connection's commands from writing requests and retries to the Verbose stream, whatever other connections do. -DiagnosticDebug can be used only with Debug or no -LogLevel.

```yaml
Type: FilesCom.PowerShell.FilesLogLevel
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: ApiKey
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Session
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Credential
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxNetworkRetries

How many times the engine retries a request that failed in a way that may be temporary, such as a connection failure, a network timeout or a server error, from 0 to 100, each retry shown with -Verbose. Defaults to 3; 0 turns the engine's retries off. Below them, a request that only reads something, such as a download, can be sent again when its connection closes before any answer. A request that changes something, such as a delete or the one that completes an upload, is sent once per try, and again only where Files.com redirects it; after a lost or failed answer it is not sent again.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -MaxRetryDelaySeconds

The longest wait, in seconds, before a retry of a Files.com API request. Defaults to 2.

```yaml
Type: System.Double
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -NoDefault

Leaves the runspace's default connection as it is, so commands use the new connection only when given it with -Connection.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -OneTimePassword

The current code from the user's two-factor authentication device or text message: sent with -Credential, or finishing a -Challenge, where it is required. It is a single-use code, not a stored secret.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Credential
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -OperationTimeout

The longest any one command may run with this connection, from its start to its end, across every request, page, retry, input object and file it handles; a command that runs longer stops with Files.OperationTimeout and returns once its work has stopped. By default commands have no time limit.

```yaml
Type: System.TimeSpan
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Proxy

The address of a proxy server, such as http://proxy.example.com:8080, for every request of the connection, to Files.com and to the storage addresses of uploads and downloads. It must be an absolute http:// or https:// address without credentials; give them with -ProxyCredential. By default the system's proxy settings apply.

```yaml
Type: System.Uri
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -ProxyCredential

The credential the -Proxy server asks for. It is sent only to the proxy, when the proxy asks for it.

```yaml
Type: System.Management.Automation.PSCredential
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -ReadTimeoutSeconds

How long, in seconds, a request may wait for its response once sent, and then between the parts of the response, from 1 to 2147483. Defaults to 80. It limits a stalled connection, not a long download that keeps receiving data. A request that runs out is retried like other temporary failures, then fails with Files.NetworkTimeout.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -RegionRelocation

Whether commands move on to the address Files.com names when it answers that the site is served from another region: Automatic, the default, moves to a verified HTTPS address on port 443 with a public DNS name; Disabled never moves, and the command fails with Files.RegionMismatch naming the address; a hashtable of origin pairs, such as @{ 'https://files.example.com' = 'https://files.example.eu' }, moves only from each key to its value, or values, and also allows what Automatic never does, such as another port, a name that is not public or plain HTTP. Origins are a scheme, a host and an optional port, with no path. A command that moves keeps the new address for its remaining requests, and so do the objects and pages it writes; the connection's own address is unchanged. Moving takes one of the command's retries (-MaxNetworkRetries). The connection's RegionRelocation property shows the policy.

```yaml
Type: FilesCom.PowerShell.FilesRegionRelocationPolicy
DefaultValue: Automatic
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: ApiKey
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Session
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: Credential
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues:
- Automatic
- Disabled
HelpMessage: ''
```

### -RetryDelaySeconds

The wait, in seconds, before the first retry of a Files.com API request; each later retry waits twice as long, up to -MaxRetryDelaySeconds. Defaults to 0.1. A server that says when to retry is waited for instead, and requests to the storage addresses of uploads and downloads wait as their servers ask.

```yaml
Type: System.Double
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -SessionId

An existing session ID to authenticate with, as a SecureString. Disconnecting never revokes it.

```yaml
Type: System.Security.SecureString
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: Session
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -WhatIf

Runs the command in a mode that only reports what would happen without performing the actions.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases:
- wi
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -WorkspaceId

The workspace commands run in unless they say otherwise with -WorkspaceScope or -NoWorkspaceScope, including workspace 0.

```yaml
Type: System.Int64
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -WriteTimeoutSeconds

How long, in seconds, an upload part may go without sending data, from 1 to 2147483. Defaults to 80. It limits a stalled connection, not a long upload; time spent reading the local file or stream is not counted.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### FilesCom.PowerShell.FilesSignInChallenge

A sign-in to finish with -Challenge.

## OUTPUTS

### FilesCom.PowerShell.FilesConnection

The new connection.

## NOTES

The connection is closed when Disconnect-FilesAccount closes it, when the runspace that made it closes or when the module is removed. Closing it stops the commands using it and closes the network connections the engine made for it; a -HttpMessageHandler is never disposed. Other runspaces, such as ForEach-Object -Parallel script blocks given it with $using:, may use it while it is open. Connections cannot be saved: an object read back with Import-Clixml is data only. See about_FilesCom for connections, errors and previewing changes.

## RELATED LINKS

- [about_FilesCom]()

