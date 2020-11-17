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
            >> Seq.collect (fun (src, fileName) -> 
                if File.Exists src then
                    seq { copyFileTask (Some fileName) script src <| targetDir + fileName }
                else if Directory.Exists src then
                    let src = src |> Utils.normalizeDirPath
                    Directory.EnumerateDirectories (src, "", SearchOption.AllDirectories)
                    |> Seq.map (fun x -> targetDir + x.[src.Length+1..])
                    |> Seq.iter (Directory.CreateDirectory >> ignore)

                    Directory.EnumerateFiles (src, "", SearchOption.AllDirectories)
                    |> Seq.sortBy String.length
                    |> Seq.map (fun path ->
                        let fileName = path.[src.Length+1..]
                        let dst = targetDir + fileName
                        copyFileTask (Some fileName) script path dst)
                else raise <| DirectoryNotFoundException src))
            ctx script script.arguments.[1],
        ctx
}
