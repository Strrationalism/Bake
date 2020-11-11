module Bake.Actions.Parallel

open Bake

[<BakeAction>]
let Parallel = {
    help = "并行执行子代码块内的Task"

    usage = [
        """Parallel {<子代码块内容>}"""
    ]

    example = [
        """Parallel { Include Sub.bake }    # Sub.bake的Tasks将会并行执行"""
    ]
    
    action = fun ctx -> 
        ctx.script.arguments
        |> Seq.tryExactlyOne
        |> function
        | None -> raise <| Action.ActionUsageError "Parallel must pass one argument."
        | Some block ->
            let tasks =
                Script.parseTasks ctx.script.scriptFile (block.Trim() + "\n")
                |> function
                | Error e -> raise e
                | Ok x -> Action.runActions ctx x
                |> Seq.toArray
            seq {
                {
                    run = fun ctx ->
                        tasks 
                        |> Array.Parallel.iter (Task.run ctx >> ignore)
                
                    inputFiles = tasks |> Seq.collect (fun x -> x.inputFiles)
                    outputFiles = tasks |> Seq.collect (fun x -> x.outputFiles)
                
                    dirty = false
                
                    source = ctx.script
                }
            }
}