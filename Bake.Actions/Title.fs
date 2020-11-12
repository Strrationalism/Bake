module Bake.Actions.Title

open Bake

let titleTask script title = {
    run = fun _ ->
        System.Console.Title <- title

    inputFiles = Seq.empty
    source = script
    outputFiles = Seq.empty
    dirty = true
}

[<BakeAction>]
let Title = {
    help = "设置控制台窗口标题"

    usage = [
        """Title <标题>"""
    ]

    example = [
        """Title "Hello, world!" """
    ]

    action = fun ctx script -> 
        if script.arguments.Length <> 1 then raise <| Action.ActionUsageError "Title必须有一个参数。"
        seq { titleTask script <| Action.applyContextToArgument ctx script.arguments.Head },
        ctx
}
