module Bake.Actions.Delete

open Bake
open System.IO

let deleteFileTask src fileName = {
    run = fun _ ->
        File.Delete fileName
        lock stdout (fun () -> printfn "Delete %s" fileName)

    inputFiles = Seq.empty
    outputFiles = Seq.empty

    dirty = true

    source = src
}

let deleteDirectoryTask src dirName = {
    run = fun _ ->
        Directory.Delete (dirName, true)
        lock stdout (fun () -> printfn "Delete %s" dirName)

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
    
    action = Action.singleBlockArgumentAction (fun ctx script path ->
        seq {
            if File.Exists path then
                deleteFileTask script path
            if Directory.Exists path then
                deleteDirectoryTask script path
        })
}