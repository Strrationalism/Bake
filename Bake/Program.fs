
open Bake
(*
[<EntryPoint>]
let main args =
    let buildScript =
        match args with
        | [||] -> 
            [
                "BuildScript.bake"
                "Build.bake"
                "Assets/Build.bake"
                "Assets/BuildScript.bake"
            ]
            |> List.find System.IO.File.Exists
        | [|a|] -> a
        | _ -> failwithf "Not support"
        |> System.IO.FileInfo

    let defaultContext = {
        updatedOutputFile = []
    }
    
    let rootTasks =
        Parser.parseFromFile buildScript
    
    let timer = System.Diagnostics.Stopwatch ()
    timer.Start ()

    rootTasks
    |> List.fold Task.run defaultContext
    |> ignore

    timer.Stop ()

    printfn "Build Time: %A" timer.Elapsed

    0
    *)
Script.parseFromFile (System.IO.FileInfo "c:/repos/t.txt")
|> List.iter (printfn "%A")

// Next: 实现ActionAttribute和以下Action
// Next: 完善文档，总结Module、Action和Task的概念（Module包含Actions，Actions创建Tasks，Tasks经过IsDirty过滤后执行）
// Next: 实现以下Action
//       * Set
//       * Import
//       * Parallel
//       * Include
//       * MakeDir
//       * Copy
//       * Zip
