
open Bake

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
            variables = 
                Map.empty
                |> Map.add "StartDir" buildScriptFile.Directory.FullName
                |> Map.add "CurrentDir" System.Environment.CurrentDirectory
            actions = defaultActions

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
//       * If
//           * Equals $a $b
//           * Exists File $file
//           * Exists Directory $dir
//           * Not $Expression
//           * $SrcFiles Newer $DstFiles

