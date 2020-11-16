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
        Utils.verifyArgumentCount script 1
        seq { titleTask script <| Utils.applyContextToArgument ctx script.arguments.Head },
        ctx
}
