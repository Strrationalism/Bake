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
        if script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Copy must be pass one argument."

        let targetDir = script.arguments.[0].Trim() |> Action.applyContextToArgument ctx
        let targetDir = targetDir.Trim().TrimEnd('\\', '/') + "/"
        let srcDir = script.scriptFile.DirectoryName.TrimEnd('\\', '/') + "/"

        Action.blockArgumentTaskPerLine (fun _ script ->
            Utils.mapPathToOutputPath srcDir
            >> Seq.map (fun (src, fileName) -> copyFileTask (Some fileName) script src <| targetDir + fileName))
            ctx script script.arguments.[1],
        ctx
}
