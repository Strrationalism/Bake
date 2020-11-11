module Bake.Actions.Unzip


open Bake
open System.IO
open System.IO.Compression

[<BakeAction>]
let Unzip = {
    help = "解压Zip压缩文件"

    usage = [
        """Zip <目标文件> { 待解压的zip文件列表 }"""
    ]

    example = [
        """Zip $Output { *.zip } """
    ]

    action = fun ctx -> 
        if ctx.script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Unzip必须有两个参数。"

        let targetDir = ctx.script.arguments.[0].Trim() |> Action.applyContextToArgument ctx
        let targetDir = targetDir.TrimEnd('\\', '/') + "/"
        let zipFiles = 
            ctx.script.arguments.[1] |> Action.applyContextToArgument ctx
            |> Script.lines
            |> Seq.map Script.trimLineComment
            |> Script.trimLines

        zipFiles
        |> Seq.map (fun zipFile -> 
            {
                run = fun _ -> 
                    lock stdout (fun () -> printfn "Decompressing %s..." zipFile)
                    use zip = ZipFile.OpenRead (zipFile)
                    zip.ExtractToDirectory (targetDir)
                inputFiles = seq { FileInfo zipFile }
                outputFiles = Seq.empty
                dirty = true
                source = ctx.script
            })
}


