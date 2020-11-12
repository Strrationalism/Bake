module Bake.Actions.Atomic

open Bake
open Sync

[<BakeAction>]
let Atomic = {
    help = "原子任务块"

    usage = [
        """Atomic {<子代码块内容>}"""
    ]

    example = [
        """Atomic { Include Sub.bake }    # Sub.bake的Tasks将会串行执行，可作为一个单位放入Parallel中"""
    ]
    
    action = fun ctx script -> 
        seq{ {
            inputFiles = Seq.empty
            outputFiles = Seq.empty
            dirty = true
            source = script
            run = fun _ ->
                script.arguments
                |> Seq.tryExactlyOne
                |> function
                | None -> raise <| Action.ActionUsageError "Atomic must pass one argument."
                | Some block ->
                    Parser.parseScripts script.scriptFile (block.Trim() + "\n")
                    |> function
                    | Error e -> raise e
                    | Ok x -> syncBlockRunner (setChildBlockRunner ctx syncBlockRunner) x |> ignore
        } },
        ctx
}

