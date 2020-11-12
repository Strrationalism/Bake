module Bake.Actions.Set

open Bake

let setVariable ctx name value = 
    { ctx with
        variables = ctx.variables |> Map.add name value }

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
    
    action = fun ctx script ->
        if script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Set must be pass one argument."
        else 
            let name = script.arguments.[0].Trim().TrimStart('$')
            let value = script.arguments.[1].Trim() |> Action.applyContextToArgument ctx
            Seq.empty,
            setVariable ctx name value
}
