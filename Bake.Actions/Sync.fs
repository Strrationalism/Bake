module Bake.Actions.Sync

open Bake

[<BakeAction>]
let Sync = {
    help = "强制串行执行子代码块内的Task"

    usage = [
        """Sync {<子代码块内容>}"""
    ]

    example = [
        """Sync { Include Sub.bake }    # Sub.bake的Tasks将会串行执行"""
    ]
    
    action = fun ctx -> 
        ctx.script.arguments
        |> Seq.tryExactlyOne
        |> function
        | None -> raise <| ActionUsageError "Sync must pass one argument."
        | Some block ->
            let tasks =
                Script.parseTasks ctx.script.scriptFile block
                |> function
                | Error e -> raise e
                | Ok x -> Action.runActions ctx x
            seq {
                {
                    run = fun ctx ->
                        tasks
                        |> Seq.fold Task.run ctx
                        |> ignore
                
                    inputFiles = tasks |> Seq.collect (fun x -> x.inputFiles)
                    outputFiles = tasks |> Seq.collect (fun x -> x.outputFiles)
                
                    dirty = false
                
                    source = ctx.script
                }
            }
}