# FAKE dotnet local tool bug

Companion repo to https://github.com/fsprojects/FAKE/issues/2735.

I'm seeing different behaviour when running a dotnet local tool ([grate](https://erikbra.github.io/grate/)) via FAKE than when running it directly. In particular, it fails when run via FAKE, but works without error when run directly from the command line.

## Prerequisites

- .NET 6 SDK (for running the build project, which delegates to FAKE)
- Docker (for starting a container running a PostgreSQL database cluster that grate can work against)

## Repro steps

Run the following commands from the repo root:

``` sh
dotnet tool restore
dotnet run
```

You should see the following command printed in the FAKE logs:

``` sh
/home/matt/dev/fake-dotnet-local-tool-bug/src/db> "/usr/bin/dotnet" grate --noninteractive --databasetype postgresql --connectionstring 'Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!' --adminconnectionstring 'Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!' (In: false, Out: false, Err: false)
```

And the following error reported (with a stacktrace relating to grate, see below):

``` sh
Unhandled exception: System.ArgumentException: Format of the initialization string does not conform to specification starting at index 55.
```

However, when running the command printed above from the same place that FAKE reports executing it from, everything works fine:

``` sh
matt@matt-framework:~/dev/fake-dotnet-local-tool-bug/src/db$ "/usr/bin/dotnet" grate --noninteractive --databasetype postgresql --connectionstring 'Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!' --adminconnectionstring 'Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!'
Initializing connections.
Running grate v1.4.0.0 against  - my_database.
Looking in . for scripts to run.
================================================================================
Setup, Backup, Create/Restore/Drop
================================================================================
================================================================================
Grate Structure
================================================================================
================================================================================
Versioning
================================================================================
 Migrating my_database from version 0.0.0.0 to 0.0.0.1.
 Versioning my_database database with version 0.0.0.1.
================================================================================
Migration Scripts
================================================================================
Skipping 'BeforeMigration', beforeMigration does not exist.
Skipping 'AlterDatabase', alterDatabase does not exist.
Skipping 'Run After Create Database', runAfterCreateDatabase does not exist.
Skipping 'Run Before Update', runBeforeUp does not exist.

Looking for Update scripts in "./up". These should be one time only scripts.
--------------------------------------------------------------------------------
  Running '0001_create_client_table.sql'.
--------------------------------------------------------------------------------

Skipping 'Run First After Update', runFirstAfterUp does not exist.
Skipping 'Functions', functions does not exist.
Skipping 'Views', views does not exist.
Skipping 'Stored Procedures', sprocs does not exist.
Skipping 'Triggers', triggers does not exist.
Skipping 'Indexes', indexes does not exist.
Skipping 'Run after Other Anytime Scripts', runAfterOtherAnyTimeScripts does not exist.
Skipping 'Permissions', permissions does not exist.
Skipping 'AfterMigration', afterMigration does not exist.


grate v1.4.0.0 has grated your database (my_database)! You are now at version 0.0.0.1. All changes and backups can be found at "/home/matt/.local/share/grate/migrations/my_database/2023-02-26T18_30_08.2106773_00_00".
```

This is confusing. I would expect the commands that are being run in each case to do the same thing, and therefore to behave in the same way.

## Full logs containing error

``` sh
matt@matt-framework:~/dev/fake-dotnet-local-tool-bug$ dotnet run
run ApplyLocalDatabaseMigrations
Building project with version: LocalBuild
Shortened DependencyGraph for Target ApplyLocalDatabaseMigrations:
<== ApplyLocalDatabaseMigrations
   <== StartDockerServices

The running order is:
Group - 1
  - StartDockerServices
Group - 2
  - ApplyLocalDatabaseMigrations
Starting target 'StartDockerServices'
Docker Services: .> /usr/local/bin/docker compose up
Finished (Success) 'StartDockerServices' in 00:00:00.0061097
Starting target 'ApplyLocalDatabaseMigrations'
/home/matt/dev/fake-dotnet-local-tool-bug/src/db> "/usr/bin/dotnet" grate --noninteractive --databasetype postgresql --connectionstring 'Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!' --adminconnectionstring 'Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!' (In: false, Out: false, Err: false)
.> "/usr/local/bin/docker" compose up (In: false, Out: true, Err: true)
Docker Services: Container fake-dotnet-local-tool-bug-db-1  Created
Docker Services: Attaching to fake-dotnet-local-tool-bug-db-1
Initializing connections.
Unhandled exception: System.ArgumentException: Format of the initialization string does not conform to specification starting at index 55.
   at System.Data.Common.DbConnectionOptions.GetKeyValuePair(String connectionString, Int32 currentPosition, StringBuilder buffer, Boolean useOdbcRules, String& keyname, String& keyvalue)
   at System.Data.Common.DbConnectionOptions.ParseInternal(Dictionary`2 parsetable, String connectionString, Boolean buildChain, Dictionary`2 synonyms, Boolean firstKey)
   at System.Data.Common.DbConnectionOptions..ctor(String connectionString, Dictionary`2 synonyms, Boolean useOdbcRules)
   at System.Data.Common.DbConnectionStringBuilder.set_ConnectionString(String value)
   at Npgsql.NpgsqlConnectionStringBuilder..ctor(String connectionString)
   at Npgsql.NpgsqlConnection.GetPoolAndSettings()
   at Npgsql.NpgsqlConnection.set_ConnectionString(String value)
   at Npgsql.NpgsqlConnection..ctor(String connectionString)
   at grate.Migration.PostgreSqlDatabase.GetSqlConnection(String connectionString) in /home/runner/work/grate/grate/grate/Migration/PostgreSqlDatabase.cs:line 17
   at grate.Migration.AnsiSqlDatabase.get_Connection() in /home/runner/work/grate/grate/grate/Migration/AnsiSqlDatabase.cs:line 73
   at grate.Migration.AnsiSqlDatabase.get_ServerName() in /home/runner/work/grate/grate/grate/Migration/AnsiSqlDatabase.cs:line 38
   at grate.Migration.GrateMigrator.Migrate() in /home/runner/work/grate/grate/grate/Migration/GrateMigrator.cs:line 39
   at grate.Commands.MigrateCommand.<>c__DisplayClass0_0.<<-ctor>b__0>d.MoveNext() in /home/runner/work/grate/grate/grate/Commands/MigrateCommand.cs:line 48
--- End of stack trace from previous location ---
   at System.CommandLine.NamingConventionBinder.CommandHandler.GetExitCodeAsync(Object returnValue, InvocationContext context)
   at System.CommandLine.NamingConventionBinder.ModelBindingCommandHandler.InvokeAsync(InvocationContext context)
   at System.CommandLine.Invocation.InvocationPipeline.<>c__DisplayClass4_0.<<BuildInvocationChain>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at System.CommandLine.Builder.CommandLineBuilderExtensions.<>c__DisplayClass17_0.<<UseParseErrorReporting>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at System.CommandLine.Builder.CommandLineBuilderExtensions.<>c__DisplayClass12_0.<<UseHelp>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at System.CommandLine.Builder.CommandLineBuilderExtensions.<>c__DisplayClass19_0.<<UseTypoCorrections>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at System.CommandLine.Builder.CommandLineBuilderExtensions.<>c.<<UseSuggestDirective>b__18_0>d.MoveNext()
--- End of stack trace from previous location ---
   at System.CommandLine.Builder.CommandLineBuilderExtensions.<>c__DisplayClass16_0.<<UseParseDirective>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at System.CommandLine.Builder.CommandLineBuilderExtensions.<>c.<<RegisterWithDotnetSuggest>b__5_0>d.MoveNext()
--- End of stack trace from previous location ---
   at System.CommandLine.Builder.CommandLineBuilderExtensions.<>c__DisplayClass8_0.<<UseExceptionHandler>b__0>d.MoveNext()
Finished (Failed) 'ApplyLocalDatabaseMigrations' in 00:00:01.1089003

---------------------------------------------------------------------
Build Time Report
---------------------------------------------------------------------
Target                         Duration
------                         --------
StartDockerServices            00:00:00.0010908
ApplyLocalDatabaseMigrations   00:00:01.1086972   (Process exit code '1' <> 0. Command Line: /usr/bin/dotnet grate --noninteractive --databasetype postgresql --connectionstring 'Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!' --adminconnectionstring 'Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!')
Total:                         00:00:01.2985069
Status:                        Failure
---------------------------------------------------------------------
Fake.Core.BuildFailedException: Target 'ApplyLocalDatabaseMigrations' failed.
 ---> System.AggregateException: One or more errors occurred. (Process exit code '1' <> 0. Command Line: /usr/bin/dotnet grate --noninteractive --databasetype postgresql --connectionstring 'Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!' --adminconnectionstring 'Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!')
 ---> System.Exception: Process exit code '1' <> 0. Command Line: /usr/bin/dotnet grate --noninteractive --databasetype postgresql --connectionstring 'Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!' --adminconnectionstring 'Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!'
   at Fake.Core.CreateProcess.ensureExitCode@608.Invoke(a data, Int32 exitCode)
   at Fake.Core.CreateProcess.addOnExited@584-5.Invoke(RawProcessResult e)
   at Microsoft.FSharp.Control.AsyncPrimitives.CallThenInvokeNoHijackCheck[a,b](AsyncActivation`1 ctxt, b result1, FSharpFunc`2 userCode) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 525
   at Microsoft.FSharp.Control.Trampoline.Execute(FSharpFunc`2 firstAction) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 112
--- End of stack trace from previous location ---
   at Microsoft.FSharp.Control.AsyncResult`1.Commit() in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 453
   at Microsoft.FSharp.Control.AsyncPrimitives.QueueAsyncAndWaitForResultSynchronously[a](CancellationToken token, FSharpAsync`1 computation, FSharpOption`1 timeout) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1133
   at Microsoft.FSharp.Control.AsyncPrimitives.RunSynchronously[T](CancellationToken cancellationToken, FSharpAsync`1 computation, FSharpOption`1 timeout) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1160
   at Microsoft.FSharp.Control.FSharpAsync.RunSynchronously[T](FSharpAsync`1 computation, FSharpOption`1 timeout, FSharpOption`1 cancellationToken) in D:\a\_work\1\s\src\FSharp.Core\async.fs:line 1504
   at Fake.Core.Proc.run[a](CreateProcess`1 c)
   at BuildHelpers.run[a,b,c](FSharpFunc`2 proc, a arg, b dir) in /home/matt/dev/fake-weirdness/BuildHelpers.fs:line 76
   at Build.clo@22-1.Invoke(TargetParameter _arg2) in /home/matt/dev/fake-weirdness/Build.fs:line 30
   at Fake.Core.TargetModule.runSimpleInternal(TargetContext context, Target target)
   --- End of inner exception stack trace ---
   --- End of inner exception stack trace ---
   at Fake.Core.TargetModule.raiseIfError(OptionalTargetContext context)
   at Fake.Core.TargetModule.runOrDefault(String defaultTarget)
   at BuildHelpers.runOrDefault(String defTarget, String[] args) in /home/matt/dev/fake-weirdness/BuildHelpers.fs:line 88
```
