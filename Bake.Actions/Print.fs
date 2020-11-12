module Bake.Actions.Print

open Bake

let printTask script text = {
    run = fun _ ->
        lock stdout (fun () -> 
            text
            |> printfn "%s")
    source = script
    inputFiles = Seq.empty
    outputFiles = Seq.empty
    dirty = true
}

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
    
    action = fun ctx script -> 
        seq { 
            Seq.reduce (fun a b -> a + System.Environment.NewLine + b) script.arguments
            |> Action.applyContextToArgument ctx
            |> printTask script },
        ctx
}
