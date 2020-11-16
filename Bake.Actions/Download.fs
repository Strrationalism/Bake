module Bake.Actions.Download

open Bake
open Utils
open System.Net

let downloadFile (url: string) targetFile =
    use webClient = new WebClient ()
    webClient.DownloadFile (url, targetFile)

let downloadTask echo (targetDir: string) script (url: string) = 
    let fileName = url.[1 + url.LastIndexOf '/'..]
    let targetDir = targetDir.Trim().Trim('\\', '/') + "/"
    let targetFile = targetDir + fileName
    {
        run = fun () ->
            if echo then lock stdout (fun () -> printfn "Downloading %s..." fileName)
            downloadFile url targetFile
        inputFiles = Seq.empty
        outputFiles = seq { targetFile }
        dirty = true
        source = script
    }

[<BakeAction>]
let Download = {
    help = "下载文件到目标目录"

    usage = [
        """Download <目标目录> { 文件列表 }"""
    ]

    example = [
        """Run { ls }"""
    ]
    
    action = fun ctx script ->
        verifyArgumentCount script 2
        let targetDir = script.arguments.Head |> applyContextToArgument ctx
        blockArgumentTaskPerLine (fun _ script url ->
            seq { downloadTask true targetDir script url }) ctx script script.arguments.[1],
        ctx
}
