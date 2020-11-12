module Bake.Actions.Zip

open Bake
open System.IO
open System.IO.Compression

let zipTask echo script targetZip files = {
    run = fun _ ->
        if echo then lock stdout (fun () -> printfn "Creating %s..." targetZip)

        use zip = ZipFile.Open (targetZip, ZipArchiveMode.Create)
        for (file, name) in files do
            if echo then lock stdout (fun () -> printfn "Compressing %s ..." name)
            zip.CreateEntryFromFile(file, name.Replace('\\','/')) |> ignore

    inputFiles = files |> Seq.map (fst >> FileInfo)
    source = script
    outputFiles = seq { targetZip }
    dirty = false
}

[<BakeAction>]
let Zip = {
    help = "创建Zip压缩文件"

    usage = [
        """Zip <目标zip文件> { 文件列表 }"""
    ]

    example = [
        """Zip a.zip { *.txt } """
    ]

    action = fun ctx script -> 
        if script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Zip必须有两个参数。"

        let targetZip = script.arguments.[0].Trim() |> Action.applyContextToArgument ctx
        let files = 
            script.arguments.[1] |> Action.applyContextToArgument ctx
            |> Script.lines
            |> Seq.map Script.trimLineComment
            |> Script.trimLines
            |> Seq.collect (Utils.mapPathToOutputPath script.scriptFile.DirectoryName)
            |> Seq.distinctBy snd
            |> Seq.toArray

        seq { zipTask true script targetZip files },
        ctx
}
