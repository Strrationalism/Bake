module Bake.Actions.Copy

open Bake
open System.IO

let copyFileTask (echo: string option) script srcFile dstFile = {
    run = fun _ ->
        match echo with
        | Some fileName -> lock stdout (fun () -> printfn "Copy %s ..." fileName)
        | None -> ()
        let dstDir = FileInfo(dstFile).Directory
        if not dstDir.Exists then dstDir.Create ()
        File.Copy (srcFile, dstFile)
            
    inputFiles = seq { FileInfo (srcFile) }
    source = script
    outputFiles = seq { dstFile }
    dirty = false
}

[<BakeAction>]
let Copy = {
    help = "复制文件"

    usage = [
        """Copy <目标文件夹> { 文件列表 }"""
    ]

    example = [
        """Copy $Output { *.txt } """
    ]

    action = fun ctx script -> 
        if script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Copy必须有两个参数。"

        let targetDir = script.arguments.[0].Trim() |> Action.applyContextToArgument ctx
        let targetDir = targetDir.Trim().TrimEnd('\\', '/') + "/"
        let srcDir = script.scriptFile.DirectoryName.TrimEnd('\\', '/') + "/"
        let files = 
            script.arguments.[1] |> Action.applyContextToArgument ctx
            |> Script.lines
            |> Seq.map Script.trimLineComment
            |> Script.trimLines
            |> Seq.collect (Utils.mapPathToOutputPath srcDir)

        files
        |> Seq.map (fun (src, fileName) -> 
            copyFileTask (Some fileName) script src <| targetDir + fileName),
        ctx
}
