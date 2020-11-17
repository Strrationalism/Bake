module Bake.Actions.Start

open Bake

[<BakeAction>]
let Start = {
    help = "启动命令"

    usage = [
        """Start <命令>"""
    ]

    example = [
        """Start { ls }"""
    ]
    
    action = Run.runAction { hidden = false; waitForExit = false }
}
