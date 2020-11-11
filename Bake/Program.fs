﻿
open Bake

[<EntryPoint>]
let main args =
    try
        let buildScript =
            match args with
            | [||] -> 
                [
                    "BuildScript.bake"
                    "Build.bake"
                    "Package.bake"
                ]
                |> List.find System.IO.File.Exists
            | [|a|] -> [ a; a + ".bake" ] |> List.find System.IO.File.Exists
            | _ -> failwithf "Not support"
            |> System.IO.FileInfo
            |> Script.parseFromFile
            |> function
            | Ok x -> x
            | Error e -> raise e

        let defaultActions = 
            System.Reflection.Assembly.Load("Bake.Actions")
            |> Action.getActionsFromAssembly
            |> Map.ofSeq

        let defaultActionContext = {
            script = buildScript |> List.head
            variables = ref Map.empty
            actions = ref defaultActions
        }

        let defaultTaskContext = {
            updatedOutputFile = []
            errorMessages = ref []
        }
    
    
        let timer = System.Diagnostics.Stopwatch ()
        timer.Start ()

        buildScript
        |> Action.runActions defaultActionContext
        |> Seq.fold Task.run defaultTaskContext
        |> ignore

        timer.Stop ()

        printfn "Build Time: %A" timer.Elapsed

        0
    with 
    | Script.ParsingError e ->
        printfn "Parsing Error:%s" e
        -1
    | e ->
        printfn "Error:%s" e.Message 
        -1
    

// Next: 实现以下Action
//       * Copy
//       * Zip
//       * Function