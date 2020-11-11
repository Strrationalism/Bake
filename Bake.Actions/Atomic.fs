module Bake.Actions.Sync

open Bake

[<BakeAction>]
let Sync = {
    help = "强制串行执行子代码块内的Tasks，当其中一个任务出错时终止其后的工作"

    usage = [
        """Atomic {<子代码块内容>}"""
    ]

    example = [
        """Atomic { Include Sub.bake }    # Sub.bake的Tasks将会串行执行"""
    ]
    
    action = fun ctx -> 
        ctx.script.arguments
        |> Seq.tryExactlyOne
        |> function
        | None -> raise <| Action.ActionUsageError "Sync must pass one argument."
        | Some block ->
            let tasks =
                Script.parseTasks ctx.script.scriptFile (block.Trim() + "\n")
                |> function
                | Error e -> raise e
                | Ok x -> Action.runActions ctx x
            seq {
                {
                    run = fun ctx ->
                        tasks
                        |> Seq.fold (fun (ctx, errno) t ->
                            match errno with
                            | Error e -> ctx, Error e
                            | Ok () -> Task.run ctx t) (ctx, Ok ())
                        |> snd
                        |> function
                        | Ok () -> ()
                        | Error _ ->
                            tasks
                            |> Seq.collect (fun x -> x.outputFiles)
                            |> Seq.iter (fun x ->
                                try System.IO.File.Delete x
                                with _ -> ())
                
                    inputFiles = tasks |> Seq.collect (fun x -> x.inputFiles)
                    outputFiles = tasks |> Seq.collect (fun x -> x.outputFiles)
                
                    dirty = false
                
                    source = ctx.script
                }
            }
}