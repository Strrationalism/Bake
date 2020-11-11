module Bake.Actions.Set

open Bake

[<BakeAction>]
let Set = {
    help = "设置环境变量"

    usage = [
        """Set <$变量> <值>"""
    ]

    example = [
        """Set $Var hello"""
        """Set $Var "Hello, world!" """
    ]
    
    action = fun ctx ->
        if ctx.script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Set必须有两个参数。"
        else 
            let variableName = ctx.script.arguments.[0].Trim().TrimStart('$')
            let value = ctx.script.arguments.[1].Trim() |> Action.applyContextToArgument ctx
            ctx.variables := !ctx.variables |> Map.add variableName value
            Seq.empty
}
