﻿module Bake.Actions.CreateDirectory

open Bake

let createDirectoryTask (ctx: BakeActionContext) (dir: string) =
    Seq.singleton {
        inputFiles = []
        outputFiles = []
        dirty = true
        source = ctx.script

        run = fun ctx ->
            if not <| System.IO.Directory.Exists dir then
                System.IO.Directory.CreateDirectory dir
                |> ignore
                lock stdout (fun () -> printfn "Create-Directory %s" dir)
    }

[<BakeAction>]
let ``CreateDirectory`` = {
    help = "创建文件夹"

    usage = [
        """Create-Directory <文件夹路径>"""
        """Create-Directory {\n\t<文件夹路径>\n\t<文件夹路径>\n\t...\n}"""
    ]

    example = [
        """Create-Directory $Output\NewDirectory"""
        """Create-Directory {\n\t$Output\$Output\New1\n\t$Output\New2\n}"""
    ]
    
    action = Action.singleBlockArgumentAction createDirectoryTask
}
