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
    
    action = fun ctx script ->
        script.arguments
        |> Seq.collect (Action.applyContextToArgument ctx >> Script.lines)
        |> Seq.map Script.trimLineComment
        |> Script.trimLines
        |> Seq.map (FileInfo >> Script.parseFromFile)
        |> Seq.collect (function
        | Error e -> raise e
        | Ok x -> x)
        |> ctx.runChildBlock ctx
}
