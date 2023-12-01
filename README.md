# Repro-Nsb-ScriptGeneration
This is a reproduction for the unexpected NServiceBus script generation behaviour.

## Expected Outcome
SQL script generation does not happen when building a project with either direct or transitive reference to a project with NServiceBus references where `SqlPersistenceGenerateScripts` is set to `false`.

## What was Observed
### When building Receiver.csproj
- This is working as expected.
- No SQL scripts are generated in the `bin`.

### When building HasDirectReferenceToReceiver.csproj
- This is unexpected.
- SQL scripts for all dialects are generated in the `bin`.

### When building HasTransitiveReferenceToReceiver.csproj
- This is unexpected.
- SQL scripts for all dialects are generated in the `bin.`

### When setting SqlPersistenceGenerateScripts to `false` on all projects
- This stops SQL scripts from being generated.
- However, this is not an option for the production code.

### When setting SqlPersistenceGenerateScripts to `false` for HasTransitiveReferenceToReceiver, but not for HasDirectReferenceToReceiver
- And building HasTransitiveReferenceToReceiver,
- SQL scripts for all dialects are generated in the `bin`.

## ProjectStructure
### Receiver
The core project containing NServiceBus package references.

#### Note
- `<SqlPersistenceGenerateScripts>false</SqlPersistenceGenerateScripts>`
- `SqlPersistenceSettings.cs` is <b>commented-out<b/>

### HasDirectReferenceToReceiver
The project with a direct reference to `Receiver.csproj`

#### Note
- `<SqlPersistenceGenerateScripts>false</SqlPersistenceGenerateScripts>` is <b>commented-out</b>

### HasTransitiveReferenceToReceiver
The project with a transitive reference to `Receiver.csproj` via `HasDirectReferenceToReceiver.csproj`

#### Note
- `<SqlPersistenceGenerateScripts>false</SqlPersistenceGenerateScripts>` is <b>commented-out</b>

