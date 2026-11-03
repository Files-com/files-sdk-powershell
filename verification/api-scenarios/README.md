# API scenarios: the PowerShell consumer

This folder runs the generator's shared Files.com API scenarios (`verification/scenarios/api.json` in the generator)
through the installed FilesCom.PowerShell module's public commands, against the Files.com API simulator generated from
the same schema capture. It is separate from the module's ordinary Pester suites, from the generated contract suite and
from the catalogue adapter, and its result says nothing about them.

`build/Invoke-ApiScenarios.ps1` runs it; the ordinary CI job (`build/Invoke-OrdinaryCi.ps1`) runs that on each pinned
PowerShell after its tests, as the same unprivileged user, with the module installed from the package the job built.

## Files

| File | What it is |
| --- | --- |
| `cases.json` | Generated from `__cases.json`: the definition as the generator binds it to the schema (`lib/api_scenarios.rb`), with each operation step bound to the command and parameters that operation has in this module's command catalogue. A step the catalogue cannot bind fails generation. It records the definition's SHA-256, the schema capture's and each scenario's simulator and site-only positions. |
| `run.ps1` | The consumer of one scenario, started by `with-scenario.rb` for a fresh simulator. |
| `controls.ps1` | The consumer's focused controls, started the same way. |
| `Transport.cs` | Test support, compiled before any scenario starts: the handler given to `Connect-FilesAccount -HttpMessageHandler`, a forward-only stream and the recipes' bytes. |

## What a run checks

Before any request, `run.ps1` binds the run to:

- the definition's SHA-256, which `cases.json`, the canonical preset and the simulator's reset all name;
- the raw schema capture's SHA-256 (the simulator's `generation.json` `swagger.sha256`, and `cases.json`'s), and,
  separately, the simulator's normalized schema (`schema_sha256`), which its readiness reports;
- the `api-scenarios-canonical` preset and its profile, this server's instance and the reset's epoch;
- the installed module (its version, assembly and build record), the package it came from, and the PowerShell running it;

and checks that the installed module has every command and parameter each step needs, and that every intent's details,
parameters and expectations are ones the adapter binds. Anything unbound fails the run before it sends anything.

Each step then runs as the module's public command, in a pipeline of its own in a second runspace, on a connection made
with `Connect-FilesAccount -HttpMessageHandler`. The handler forwards every request, to the API and to storage, to a
`SocketsHttpHandler` that follows no redirect, keeps no cookie, uses no proxy and never retries, and notes each request's
method, path, status, outcome, credential (only whether it was exactly the run's API key), User-Agent and timing. A
command shows objects, not HTTP status, so every status a step expects is checked where it happened: one request to the
step's route and path, answered with that status, or for an absence, at most the definition's attempts, its interval
apart, the last answered with the expected error. Uploads, downloads, copies, moves and directory transfers use the
module's own commands (`Send-FilesFile`, `Receive-FilesFile`, `Copy-FilesFile` and `Move-FilesFile` with
`Wait-FilesFileAction`, `Send-FilesDirectory`, `Receive-FilesDirectory`), never the SDK directly, and their results and
the local bytes are checked against the recipes. Listings load one page at a time with `-Page` and `-Continuation`,
within the definition's page limit.

Steps marked for a Files.com site are omitted, each with the definition's own reason; every other step must pass.

## Ownership and cleanup

The run owns a folder only from a 201 answer to its own creation, witnessed at the transport, that returned exactly the
requested path as a folder: first its run folder, then the scenario's folder, as the definition's first steps create it.
An earlier 404, a lookup, another success, a refusal, another path or type, or a failed creation owns nothing. The
scenario's own last steps delete everything it made without recursion and confirm it absent. When a step fails, nothing
after it runs; the first failure is kept, and only then the run deletes the scenario's folder recursively if it owned it,
and its run folder without recursion if it owned it, confirming each absent. What it could not remove is recorded as
unresolved, apart from the failure.

## Cancellation after the first acknowledged part

The definition cancels one upload after its first acknowledged part. The command's progress is shown at most every 250 ms,
so progress cannot time that. Instead the transport holds the first request that starts after a storage part has been
answered with success, before that request is sent. The simulator's canonical profile admits upload parts one at a
time, in order, so the next request starts only once the first part's acknowledgement reached the upload: the hold
shows that, and that nothing after it was sent. The run then stops the command's pipeline, which calls its
StopProcessing, and waits for the command to settle. The cancellation counts only when the held request ended unsent
because that stop cancelled it, the first acknowledged part was part 1, the pipeline stopped and settled, the command
wrote nothing, and the journal shows that part alone and no completion. A held request that nothing cancels ends unsent
when its hold expires, with an error that is never taken as a cancellation; the controls check that.

## The journal

After the cleanup, the run reads the simulator's journal and joins it to every request the transport saw, one to one:
each entry, in sequence order, to the earliest unjoined request with its method, path and status, which must carry the
same User-Agent (the module's) and the credential its kind allows (the run's API key to the API, none to storage), under
no fault. The journal must be complete and numbered without a gap, every entry must be of the reset's epoch, and every
request must have been answered, except the one a cancellation held unsent, which has no entry. Each passed step's
entries must be its own schema operation, or for an intent its operations and storage transfers; an upload's show one
upload with the parts it needs and one completion.

## Bounds and evidence

`Invoke-ApiScenarios.ps1` compiles `Transport.cs` first, then runs each scenario and the controls with
`with-scenario.rb --scenario api-scenarios-canonical`, one fresh simulator each, with a 30 s startup, a 210 s consumer
and a 5 s settlement timeout, and `FILES_MOCK_MAX_BODY_BYTES` of 16 MiB, so the definition's 6 MiB + 3 byte file goes
as parts of 5 MiB and 1 MiB + 3 bytes. A consumer has 175 s for its work, a further 30 s for its cleanup, and 205 s in
all, receipt included. These are selected bounds, not measured times.

Each run's folder holds what the launcher saves (`server.log`, `ready.json`, `reset.json`, `journal.json`, `run.json`)
and the consumer's receipt (`consumer.json`, or `controls.json`), with the requests the transport saw, a storage address
only by its SHA-256. `summary.json` joins them for the PowerShell that ran them.

The module supports Windows with PowerShell 7.4 or later. These runs are bounded evidence on the platform that runs
them, not a qualification of any other platform.
