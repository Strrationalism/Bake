namespace Bake

open System.IO

type TaskContext = {
    updatedOutputFile : FileInfo seq
}

and Task = {
    run : TaskContext -> unit

    inputFiles : FileInfo seq
    outputFiles : string seq

    dirty : bool

    source : Script
}

module Task = 
    let isDirty x = true  // Task -> bool

    let mergeTaskContext a b = {
        updatedOutputFile = Seq.append a.updatedOutputFile b.updatedOutputFile
    }

    let run (context: TaskContext) (task: Task) : Result<TaskContext, exn> = 
        if isDirty task then 
            try
                task.outputFiles
                |> Seq.iter (fun x ->
                    try System.IO.File.Delete x
                    with _ -> ())
                task.run context
                Ok 
                    { context with
                        updatedOutputFile = Seq.append context.updatedOutputFile <| Seq.map FileInfo task.outputFiles }
            with e -> 
                task.outputFiles
                |> Seq.iter (fun x -> 
                    try System.IO.File.Delete x
                    with _ -> ())
                Error e
            
        else Ok context



