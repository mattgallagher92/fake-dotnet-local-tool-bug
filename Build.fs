module Build

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators

open BuildHelpers
open BuildTools

initializeContext()

let srcPath = Path.getFullName "src"
let dbSrcPath = srcPath </> "db"
let localDbConnectionString = "Host=localhost;Database=my_database;Username=postgres;Password=aVeryStrongPassword!"
let localDbAdminConnectionString = "Host=localhost;Database=postgres;Username=postgres;Password=aVeryStrongPassword!"

Target.create "StartDockerServices" (fun _ ->
    async { runParallel [ "Docker Services", Tools.docker "compose up" "." ] } |> Async.Start
)

Target.create "ApplyLocalDatabaseMigrations" (fun _ ->
    let grateCommand =
        "grate"
        + " --noninteractive"
        + " --databasetype postgresql"
        + $" --connectionstring '%s{localDbConnectionString}'"
        + $" --adminconnectionstring '%s{localDbAdminConnectionString}'"
    // This works locally but not via FAKE
    run Tools.dotnet grateCommand dbSrcPath
)

let dependencies = [
    "StartDockerServices"
        ==> "ApplyLocalDatabaseMigrations"
]

[<EntryPoint>]
let main args = runOrDefault "ApplyLocalDatabaseMigrations" args
