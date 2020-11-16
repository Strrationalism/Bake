module Bake.Actions.CreateDirectory

open Bake

let createDirectoryTask (ctx: BakeActionContext) script (dir: string) =
    Seq.singleton {
        inputFiles = []
        outputFiles = []
        dirty = true
        source = script

        run = fun ctx ->
            if not <| System.IO.Directory.Exists dir then
                System.IO.Directory.CreateDirectory dir
                |> ignore
    }

[<BakeAction>]
let ``CreateDirectory`` = {
    help = "创建文件夹"

    usage = [
        """CreateDirectory <文件夹路径>"""
        """CreateDirectory {\n\t<文件夹路径>\n\t<文件夹路径>\n\t...\n}"""
    ]

    example = [
        """CreateDirectory $Output\NewDirectory"""
        """CreateDirectory {\n\t$Output\$Output\New1\n\t$Output\New2\n}"""
    ]
    
    action = Utils.singleBlockArgumentAction createDirectoryTask
}

