module Bake.Actions.Run

open Bake
open Utils

exception ExitCodeIsNotZero of int

type Options = {
    waitForExit : bool
}

let run options workingDir (cmd: string) =
    let cmd, args = let cells = cmd.Split ' ' in cells.[0], cells.[1..]
    let args = Array.reduce (fun a b -> a + " " + b) args

    let startInfo = new System.Diagnostics.ProcessStartInfo ()
    startInfo.FileName <- cmd
    startInfo.Arguments <- args
    startInfo.WorkingDirectory <- workingDir

    let prc = System.Diagnostics.Process.Start startInfo
    if options.waitForExit then prc.WaitForExit ()
    if prc.ExitCode <> 0 then raise <| ExitCodeIsNotZero prc.ExitCode

let runTask options script (command: string seq) = {
    inputFiles = Seq.empty
    outputFiles = Seq.empty
    dirty = true
    source = script
    run = fun () ->
        command
        |> Seq.iter (fun cmd ->
            run options script.scriptFile.Directory.FullName cmd)
}

let runAction options = fun ctx script -> 
    verifyArgumentCount script 1

    let command =
        script.arguments.Head
        |> Script.lines
        |> Seq.map (Script.trimLineComment >> applyContextToArgument ctx)
        |> Script.trimLines
    
    seq {
        runTask options script command
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
    
    action = runAction { waitForExit = true }
}

