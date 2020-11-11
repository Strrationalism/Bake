module Bake.Actions.Delete

open Bake
open System.IO

let deleteFile src fileName = {
    run = fun _ ->
        File.Delete fileName

    inputFiles = Seq.empty
    outputFiles = Seq.empty

    dirty = true

    source = src
}

let deleteDirectory src dirName = {
    run = fun _ ->
        Directory.Delete (dirName, true)

    inputFiles = Seq.empty
    outputFiles = Seq.empty

    dirty = true

    source = src
}
    

[<BakeAction>]
let Delete = {
    help = "删除文件"

    usage = [
        """Delete {<目录名>}"""
        """Delete {<文件名>}"""
    ]

    example = [
        """Delete { *.tmp }    # 删除文件"""
        """Delete { Some }     # 删除Some目录"""
    ]
    
    action = Action.singleBlockArgumentAction (fun ctx path ->
        seq {
            if File.Exists path then
                deleteFile ctx.script path
            if Directory.Exists path then
                deleteDirectory ctx.script path
        })
}