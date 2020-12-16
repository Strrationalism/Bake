
open System
open Bake

let processError fmt =
    Console.ForegroundColor <- ConsoleColor.Red
    Console.Beep ()
    printfn fmt

[<EntryPoint>]
let main args =
    try
        let buildScriptFile =
            match args with
            | [||] -> 
                [
                    "Bake.bake"
                    "BuildScript.bake"
                    "Build.bake"
                    "Publish.bake"
                ]
                |> List.find System.IO.File.Exists
            | [|a|] -> [ a; a + ".bake" ] |> List.find System.IO.File.Exists
            | _ -> failwithf "Not support"
            |> System.IO.FileInfo

        let buildScript =
            buildScriptFile
            |> Parser.parseFromFile
            |> function
            | Ok x -> x
            | Error e -> raise e
            |> Seq.ofList

        let defaultActions = 
            System.Reflection.Assembly.Load("Bake.Actions")
            |> Action.getActionsFromAssembly
            |> Map.ofSeq

        //defaultActions |> Seq.iter (printfn "%A")
        //buildScript |> Seq.iter (printfn "%A")

        let context = {
            variables = []
            actions = defaultActions
            loadedModules = Set.empty

            runChildBlock = Bake.Actions.Sync.syncBlockRunner
        }
    
    
        let timer = System.Diagnostics.Stopwatch ()
        timer.Start ()

        context.runChildBlock context buildScript
        |> ignore


        timer.Stop ()

        printfn "Build Time: %A" timer.Elapsed

        0
    with 
    | Parser.ParsingError e -> 
        processError "Parsing Error:%s" e
        Console.ResetColor ()
        -1
    | Action.ActionNotFound e -> 
        processError "Action Not Found:%s" e
        Console.ResetColor ()
        -2
    | Action.ActionUsageError e -> 
        processError "Action Usage Error:%s" e
        Console.ResetColor ()
        -3
    | Action.ActionException (e, script, ctx) ->
        processError "Action Error:%s\n\n%A\n\n%A\n\n%A" e.Message e script ctx
        Console.ResetColor ()
        -4
    | Task.TaskException (task, e) ->
        processError "Task Error:%s\n\n%A\n\n%A" e.Message e task
        Console.ResetColor ()
        -5
    | e -> 
        processError "Error:%A" e 
        Console.ResetColor ()
        -6

    


