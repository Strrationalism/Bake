module Bake.Actions.Sync

open Bake

let syncBlockRunner : Runner = fun (actionContext: BakeActionContext) (actions: Script seq) ->

    let runTasks result task =
        result |> Result.bind (fun () -> Task.run task)

    let finalContext =
        actions
        |> Seq.fold (fun actionCtx script ->
            let tasks, actionCtx = Action.runAction actionCtx script
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
        let orgRunner = ctx.runChildBlock
        script.arguments
        |> Seq.tryExactlyOne
        |> function
        | None -> raise <| Action.ActionUsageError "Atomic must pass one argument."
        | Some block ->
            Script.parseScripts script.scriptFile (block.Trim() + "\n")
            |> function
            | Error e -> raise e
            | Ok x -> 
                let tasks, ctx =
                    syncBlockRunner (setChildBlockRunner ctx syncBlockRunner) x
                tasks, setChildBlockRunner ctx orgRunner
}
