module Bake.Actions.Print

open Bake

[<BakeAction>]
let Print = {
    help = "在屏幕上打印内容"

    usage = [
        """Print {<内容>}"""
        """Print "<内容> """
    ]

    example = [
        """Print "Hello, world!" """
        """Print { Hello, world! }"""
    ]
    
    action = fun ctx -> seq { {
        run = fun _ ->
            lock stdout (fun () -> 
                ctx.script.arguments
                |> List.iter (Action.applyContextToArgument ctx >> printfn "%s"))
        source = ctx.script
        inputFiles = Seq.empty
        outputFiles = Seq.empty
        dirty = true
    } }
}
