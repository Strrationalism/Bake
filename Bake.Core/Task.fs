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

    let run (context: TaskContext) (task: Task) : TaskContext = 
        if isDirty task then 
            try
                task.run context
                { context with
                    updatedOutputFile = Seq.append context.updatedOutputFile <| Seq.map FileInfo task.outputFiles
                }
            with e -> 
                task.outputFiles
                |> Seq.iter (fun x -> 
                    try System.IO.File.Delete x
                    with _ -> ())
                lock context.errorMessages (fun () -> context.errorMessages := e::!context.errorMessages)
                context
            
        else context


