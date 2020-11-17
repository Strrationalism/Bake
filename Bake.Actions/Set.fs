module Bake.Actions.Set

open Bake
open Utils

let setVariable ctx name value = 
    { ctx with
        variables = (name, value) :: ctx.variables |> List.sortByDescending (fst >> String.length)  }

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
        verifyArgumentCount script 2
        let name = script.arguments.[0].Trim().TrimStart('$')
        let value = script.arguments.[1].Trim() |> applyContextToArgument ctx
        Seq.empty,
        setVariable ctx name value
}
