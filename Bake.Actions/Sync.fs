module Bake.Actions.Sync

open Bake
open Utils

let syncBlockRunner : Runner = fun (actionContext: BakeActionContext) (actions: Script seq) ->

    let runTasks result task =
        result |> Result.bind (fun () -> Task.run task)

    let finalContext =
        actions
        |> Seq.fold (fun actionCtx script ->
            let tasks, actionCtx = Action.run actionCtx script
            let taskResult = tasks |> Seq.fold runTasks (Ok ())
            match taskResult with
            | Ok () -> actionCtx
            | Error e -> raise e) 
            actionContext

    Seq.empty, finalContext

let setChildBlockRunner actionContext runner = { actionContext with runChildBlock = runner }


[<BakeAction>]
let Sync = {
    help = "同步任务块"

    usage = [
        """Sync {<子代码块内容>}"""
    ]

    example = [
        """Sync { Include Sub.bake }    # Sub.bake的Tasks将会串行执行"""
    ]
    
    action = fun ctx script -> 
        verifyArgumentCount script 1
        let orgRunner = ctx.runChildBlock
        
        Parser.parseScripts script.scriptFile (script.arguments.Head.Trim() + "\n")
        |> function
        | Error e -> raise e
        | Ok x -> 
            let tasks, ctx =
                syncBlockRunner (setChildBlockRunner ctx syncBlockRunner) x
            tasks, setChildBlockRunner ctx orgRunner
}
