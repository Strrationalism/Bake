namespace Bake

open System.IO

type Context = {
    updatedOutputFile : FileInfo seq
    mutable variables : Map<string, string>
}

and Task = {
    run : Context -> unit

    inputFiles : FileInfo seq
    outputFiles : FileInfo seq

    forceUpdate : bool

    script : Script
}

module Task = 
    let isDirty x = true  // Task -> bool

    let run (context: Context) (task: Task) : Context = 
        if isDirty task || task.forceUpdate then 
            task.run context
            { context with
                updatedOutputFile = Seq.append context.updatedOutputFile task.outputFiles
            }
        else context
