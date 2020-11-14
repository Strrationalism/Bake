module Bake.Actions.Import

open Bake
open System.IO

let importAction ctx actionName action =
    { ctx with
        actions = Map.add actionName action ctx.actions }

let importActions ctx (actions: (string * BakeAction) seq) =
    actions
    |> Seq.fold (fun ctx (name, action) -> importAction ctx name action) ctx

let importDll ctx (dll: string) =
    let moduleName = dll.Trim().ToLower()
    if ctx.loadedModules |> Seq.contains moduleName then ctx
    else
        let actions = Action.getActionsFromDLL dll
        { importActions ctx actions with
            loadedModules = Set.add moduleName ctx.loadedModules }
    

let importDlls ctx dlls = Seq.fold importDll ctx dlls

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
    
    action = fun ctx script ->
        Seq.empty,
        
        script.arguments
        |> Seq.collect (
            Action.applyContextToArgument ctx 
            >> Script.lines
            >> Seq.map Script.trimLineComment)
        |> Script.trimLines
        |> Seq.fold (fun ctx moduleName ->
            let moduleName = moduleName.Replace('\\', '/').Trim()
            let dir = moduleName.[..moduleName.LastIndexOf '/'].Trim('/')
            let moduleName = moduleName.[1 + moduleName.LastIndexOf '/' ..]
            let files =
                seq {
                    script.scriptFile.DirectoryName + "/" + dir
                    System.Environment.CurrentDirectory + "/" + dir
                    System.Reflection.Assembly.GetExecutingAssembly().Location + "/" + dir
                }
                |> Seq.filter System.IO.Directory.Exists
                |> Seq.collect (fun x -> Directory.EnumerateFiles(x, moduleName, SearchOption.AllDirectories))

            if (Seq.length files) = 0 then raise <| FileNotFoundException (moduleName + "is not found.")

            importDlls ctx files)
            ctx

}
