# FAKE dotnet local tool bug

Companion repo to https://github.com/fsprojects/FAKE/issues/2735.

I'm seeing different behaviour when running a dotnet local tool ([grate](https://erikbra.github.io/grate/)) via FAKE than when running it directly. In particular, it fails when run via FAKE, but works without error when run directly from the command line.

## Prerequisites

- .NET 6 SDK (for running FAKE)
- Docker (for starting a container running a PostgreSQL database cluster that grate can work against)

## Repro steps

Run the following commands from the repo root:

``` sh
dotnet tool restore
dotnet run
```

You should see the following command printed in the FAKE lo gs:

``` sh
/home/matt/dev/fake-dotnet-local-tool-bug/src/db> "/usr/bin/dotnet" grate --noninteractive --databasetype postgresql --connectionstring 'Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!' --adminconnectionstring 'Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!' (In: false, Out: false, Err: false)
```

And the following error reported (with a stacktrace relating to grate):

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
