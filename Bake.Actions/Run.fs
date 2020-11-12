module Bake.Actions.Run

open Bake

exception ExitCodeIsNotZero of int

let runTask echo waitForExit script (command: string seq) = {
    inputFiles = Seq.empty
    outputFiles = Seq.empty
    dirty = true
    source = script
    run = fun () ->
        command
        |> Seq.iter (fun cmd ->
            if echo then lock stdout (fun () -> printfn "%s" cmd)
            let cmd, args = let cells = cmd.Split ' ' in cells.[0], cells.[1..]
            let args = Array.reduce (fun a b -> a + " " + b) args

            let startInfo = new System.Diagnostics.ProcessStartInfo ()
            startInfo.FileName <- cmd
            startInfo.Arguments <- args
            startInfo.WorkingDirectory <- script.scriptFile.Directory.FullName

            let prc = System.Diagnostics.Process.Start startInfo
            if waitForExit then prc.WaitForExit ()
            if prc.ExitCode <> 0 then raise <| ExitCodeIsNotZero prc.ExitCode)
}

let runAction echo waitForExit = fun ctx script -> 
    if script.arguments.Length <> 1 then raise <| Action.ActionUsageError "Run must be pass 1 argument."

    let command =
        script.arguments.Head
        |> Script.lines
        |> Seq.map (Script.trimLineComment >> Action.applyContextToArgument ctx)
        |> Script.trimLines
    
    seq {
        runTask echo waitForExit script command
    }, ctx

[<BakeAction>]
let Run = {
    help = "启动命令并等待其完成"

    usage = [
        """Run <命令>"""
    ]

    example = [
        """Run { ls }"""
    ]
    
    action = runAction true true
}
