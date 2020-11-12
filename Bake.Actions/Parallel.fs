module Bake.Actions.Parallel

open Bake

let parallelRunner : Runner = fun (actionContext: BakeActionContext) (actions: Script seq) ->

    let runAction (tasks, context) action =
        let newTasks, context = Action.runAction context action
        Seq.append tasks newTasks, context

    actions
    |> Seq.fold runAction (Seq.empty, actionContext)

[<BakeAction>]
let Parallel = {
    help = "并行执行子代码块内的Task"

    usage = [
        """Parallel {<子代码块内容>}"""
    ]

    example = [
        """Parallel { Include Sub.bake }    # Sub.bake的Tasks将会并行执行"""
    ]
    
    action = fun ctx script -> 
        script.arguments
        |> Seq.tryExactlyOne
        |> function
        | None -> raise <| Action.ActionUsageError "Parallel must pass one argument."
        | Some block ->
            let orgRunner = ctx.runChildBlock
            let tasks, ctx =
                Parser.parseScripts script.scriptFile (block.Trim() + "\n")
                |> function
                | Error e -> raise e
                | Ok x -> parallelRunner (Sync.setChildBlockRunner ctx parallelRunner) x

            let results =
                tasks
                |> Seq.toArray
                |> Array.Parallel.map (fun x -> Task.run x)
            
            let nextTaskContext = 
                results
                |> Array.fold (fun (result: Result<unit, exn>) x ->
                    result
                    |> Result.bind (fun result -> x))
                    (Ok ())
                |> function
                | Error e -> raise e
                | Ok x -> x

            Seq.empty,
            Sync.setChildBlockRunner ctx orgRunner
}