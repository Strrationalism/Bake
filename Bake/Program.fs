
open Bake

[<EntryPoint>]
let main args =
    try
        let buildScript =
            match args with
            | [||] -> 
                [
                    "Bake.bake"
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

        //defaultActions |> Seq.iter (printfn "%A")
        //buildScript |> Seq.iter (printfn "%A")

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
        |> Seq.fold (fun (ctx, errno) t -> Task.run ctx t) (defaultTaskContext, Ok ())
        |> ignore

        timer.Stop ()

        printfn "Build Time: %A" timer.Elapsed

        0
    with 
    | Script.ParsingError e ->
        printfn "Parsing Error:%s" e
        -1
    | Action.ActionNotFound e ->
        printfn "Action Not Found:%s" e
        -1
    | Action.ActionUsageError e ->
        printfn "Action Usage Error:%s" e
        -1
    | e ->
        printfn "Error:%A" e 
        -1
    

// Next: 实现以下Action
//       * GZip
//       * UnGZip
//       * Action
//       * Download
//       * Http Post
//       * Compile C#
//       * Compile F#
//       * Compile VB
//       * PowerShell
//       * Run
//       * Start
//       * SendEMail
//       * AESKeygen
//       * AESEncrypt
