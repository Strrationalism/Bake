module Bake.Actions.Unzip


open Bake
open System.IO
open System.IO.Compression

let unzipTask echo script targetDir zipFile =
    let zip = ZipFile.OpenRead (zipFile)
    {
        run = fun _ -> 
            if echo then lock stdout (fun () -> printfn "Decompressing %s to %s..." zipFile targetDir)
            zip.ExtractToDirectory (targetDir)
            zip.Dispose ()
        inputFiles = seq { FileInfo zipFile }
        outputFiles = zip.Entries |> Seq.map (fun x -> targetDir + x.FullName)
        dirty = false
        source = script
    }

[<BakeAction>]
let Unzip = {
    help = "解压Zip压缩文件"

    usage = [
        """Unzip <目标文件夹> { 待解压的zip文件列表 }"""
    ]

    example = [
        """Unip $Output { *.zip } """
    ]

    action = fun ctx script -> 
        if script.arguments.Length <> 2 then raise <| Action.ActionUsageError "Unzip必须有两个参数。"

        let srcDir = script.scriptFile.Directory.FullName.Trim().Trim('\\', '/') + "/"
        let targetDir = script.arguments.[0].Trim() |> Action.applyContextToArgument ctx
        let targetDir = targetDir.TrimEnd('\\', '/') + "/"

        Action.blockArgumentTaskPerLine (fun _ script zip ->
            seq { unzipTask true script targetDir <| srcDir + zip }) ctx script script.arguments.[1],
        ctx
}


