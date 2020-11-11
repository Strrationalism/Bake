namespace Bake

open System.IO

type TaskContext = {
    updatedOutputFile : FileInfo seq
    errorMessages : exn list ref
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

    let run (context: TaskContext) (task: Task) : TaskContext * Result<unit, exn> = 
        if isDirty task then 
            try
                task.outputFiles
                |> Seq.iter (fun x ->
                    try System.IO.File.Delete x
                    with _ -> ())
                task.run context
                { context with
                    updatedOutputFile = Seq.append context.updatedOutputFile <| Seq.map FileInfo task.outputFiles
                }, Ok ()
            with e -> 
                task.outputFiles
                |> Seq.iter (fun x -> 
                    try System.IO.File.Delete x
                    with _ -> ())
                lock context.errorMessages (fun () -> context.errorMessages := e::!context.errorMessages)
                context, Error e
            
        else context, Ok ()



