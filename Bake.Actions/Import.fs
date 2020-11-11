module Bake.Actions.Import

open Bake
open System.IO

[<BakeAction>]
let Import = {
    help = "导入模块"

    usage = [
        """Import <模块>"""
    ]

    example = [
        """Import PowerShell"""
        """Import $MyModules/MyModule"""
    ]
    
    action =
        Action.singleBlockArgumentAction (fun ctx moduleName ->
            let files =
                seq {
                    ctx.script.scriptFile.DirectoryName + "/"
                    System.Environment.CurrentDirectory + "/"
                    System.Reflection.Assembly.GetExecutingAssembly().Location + "/"
                }
                |> Seq.collect (fun x -> Directory.EnumerateFiles(x, moduleName, SearchOption.AllDirectories))

            if (Seq.length files) = 0 then raise <| FileNotFoundException (moduleName + "is not found.")

            files
            |> Seq.iter (fun x ->
                for actionName, action in Action.getActionsFromDLL x do
                    ctx.actions := Map.add actionName action !ctx.actions)
            Seq.empty)
}
