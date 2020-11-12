namespace Bake

open System.IO


type Task = {
    run : unit -> unit

    inputFiles : FileInfo seq
    outputFiles : string seq

    dirty : bool

    source : Script
}

module Task = 
    let isDirty x = 
        try
            if x.dirty || x.inputFiles |> Seq.isEmpty || x.outputFiles |> Seq.isEmpty then true // 如果input、output为空或者设置了强制ditry
            else if x.outputFiles |> Seq.exists (System.IO.File.Exists >> not) then true        // 如果存在未被生成的文件
            else
                let newestInputFiles = 
                    seq { yield! x.inputFiles; yield x.source.scriptFile } |> Seq.maxBy (fun (x: FileInfo) -> x.LastWriteTimeUtc)
                let oldestOutputFiles =
                    x.outputFiles |> Seq.map FileInfo |> Seq.minBy (fun x -> x.LastWriteTimeUtc)
                if newestInputFiles.LastWriteTimeUtc > oldestOutputFiles.LastWriteTimeUtc then true
                else false
        with _ -> true

    exception TaskException of Task * exn
    let run (task: Task) : Result<unit, exn> = 
        if isDirty task then 
            try
                task.outputFiles
                |> Seq.iter (fun x ->
                    try System.IO.File.Delete x
                    with _ -> ())
                task.run ()
                |> Ok
            with e -> 
                task.outputFiles
                |> Seq.iter (fun x -> 
                    try System.IO.File.Delete x
                    with _ -> ())
                TaskException (task, e) |> Error
            
        else Ok ()



