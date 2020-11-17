module Bake.Actions.Copy

open Bake
open Utils
open System.IO

let copyFileTask (echo: string option) script srcFile dstFile = {
    run = fun _ ->
        match echo with
        | Some fileName -> lock stdout (fun () -> printfn "Copy %s ..." fileName)
        | None -> ()
        let dstDir = FileInfo(dstFile).Directory
        if not dstDir.Exists then dstDir.Create ()
        let bytes = File.ReadAllBytes srcFile
        File.WriteAllBytes (dstFile, bytes)
            
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
        verifyArgumentCount script 2

        let targetDir = script.arguments.[0].Trim() |> applyContextToArgument ctx
        let targetDir = normalizeDirPath targetDir
        let srcDir = script.scriptFile.DirectoryName |> normalizeDirPath

        blockArgumentTaskPerLine (fun _ script ->
            Utils.matchInputFiles srcDir
            >> Seq.map (fun (src, fileName) -> copyFileTask (Some fileName) script src <| targetDir + fileName))
            ctx script script.arguments.[1],
        ctx
}
