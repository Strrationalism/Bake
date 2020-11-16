module Bake.Actions.Atomic

open Bake
open Utils
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
        verifyArgumentCount script 1
        seq{ {
            inputFiles = Seq.empty
            outputFiles = Seq.empty
            dirty = true
            source = script
            run = fun _ ->
                Parser.parseScripts script.scriptFile (script.arguments.Head.Trim() + "\n")
                |> function
                | Error e -> raise e
                | Ok x -> syncBlockRunner (setChildBlockRunner ctx syncBlockRunner) x |> ignore
        } },
        ctx
}

