module Bake.Actions.Include

open Bake
open System.IO

[<BakeAction>]
let Include = {
    help = "包含子脚本文件"

    usage = [
        """Include <子脚本文件>"""
    ]

    example = [
        """Include Sub.bake"""
        """Include $Assets/Sub.bake"""
        """Include {\n\tFirst.bake\n\tSecond.bake\n\tThird.bake\n\t...\n}"""
    ]
    
    action =
        Action.singleBlockArgumentAction (fun ctx script ->
            let files =
                Directory.EnumerateFiles (ctx.script.scriptFile.DirectoryName, script, SearchOption.AllDirectories)
            if (files |> Seq.length) = 0 then 
                raise <| System.IO.FileNotFoundException (script + " not found.")
            else
                files
                |> Seq.collect (fun x ->
                    Script.parseFromFile (System.IO.FileInfo x)
                    |> function
                    | Error e -> raise e
                    | Ok x -> Action.runActions ctx x))
}
