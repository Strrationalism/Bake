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
    let isDirty x = true  // Task -> bool

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
                Error e
            
        else Ok ()



