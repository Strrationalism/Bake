module Bake.Actions.Copy

open Bake
open System.IO
open System.IO.Compression

[<BakeAction>]
let Copy = {
    help = "复制文件"

    usage = [
        """Copy <目标文件夹> { 文件列表 }"""
    ]

    example = [
        """Copy $Output { *.txt } """
    ]

    action = fun ctx -> 
        if ctx.script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Zip必须有两个参数。"

        let targetDir = ctx.script.arguments.[0].Trim() |> Action.applyContextToArgument ctx
        let targetDir = targetDir.Trim().TrimEnd('\\', '/') + "/"
        let srcDir = ctx.script.scriptFile.DirectoryName.TrimEnd('\\', '/') + "/"
        let files = 
            ctx.script.arguments.[1] |> Action.applyContextToArgument ctx
            |> Script.lines
            |> Seq.map Script.trimLineComment
            |> Script.trimLines
            |> Seq.collect (Utils.mapPathToOutputPath srcDir)

        files
        |> Seq.map (fun (src, fileName) -> 
            {
                run = fun _ ->
                    lock stdout (fun () -> printfn "Copy %s..." fileName)
                    let dstDir = FileInfo(targetDir + fileName).Directory
                    if not dstDir.Exists then dstDir.Create ()
                    File.Copy (src, targetDir + fileName)
                    
                inputFiles = seq { FileInfo (fileName) }
                source = ctx.script
                outputFiles = seq { targetDir + fileName }
                dirty = false
            })
}
