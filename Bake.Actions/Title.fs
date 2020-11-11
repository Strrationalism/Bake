module Bake.Actions.Title

open Bake

[<BakeAction>]
let Title = {
    help = "设置控制台窗口标题"

    usage = [
        """Title <标题>"""
    ]

    example = [
        """Title "Hello, world!" """
    ]

    action = fun ctx -> 
        if ctx.script.arguments.Length <> 1 then raise <| Action.ActionUsageError "Title必须有一个参数。"
        seq { {
            run = fun _ ->
                System.Console.Title <- ctx.script.arguments.Head

            inputFiles = Seq.empty
            source = ctx.script
            outputFiles = Seq.empty
            dirty = true
        }}
}
