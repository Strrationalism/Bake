module Bake.Actions.Print

open Bake

let printTask ctx script text = {
    run = fun _ ->
        lock stdout (fun () -> 
            text
            |> Action.applyContextToArgument ctx 
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
        seq { printTask ctx script <| Seq.reduce (fun a b -> a + System.Environment.NewLine + b) script.arguments },
        ctx
}
