module Bake.Utils

open System.IO

let verifyArgumentCount (script: Script) argc =
    if script.arguments.Length <> argc then 
        raise <| Action.ActionUsageError (sprintf "This action is only recive %d argument(s)." argc)

let applyContextToArgument actionContext (text: string) =
    actionContext.variables
    |> Map.fold (fun (text: string) k v -> text.Replace("$" + k, v)) text

// 根据{}参数中每一行单独创建一个Task
let blockArgumentTaskPerLine createTask ctx (script: Script) =
    Script.lines
    >> Seq.map Script.trimLineComment
    >> Script.trimLines
    >> Seq.map (applyContextToArgument ctx)
    >> Seq.collect (createTask ctx script)

// 一个只具有一个{}参数的指令，它每一行为一个Task，可使用此函数创建Action
let singleBlockArgumentAction createTask ctx script =
    verifyArgumentCount script 1
    blockArgumentTaskPerLine createTask ctx script <| script.arguments.Head,
    ctx

let mapPathToOutputPath (srcPath: string) (namePattern: string) =
    let namePattern = namePattern.Replace('\\','/')
    let srcPath = srcPath.TrimEnd('\\', '/') + "/" + namePattern.[..(-1 + namePattern.LastIndexOf '/')]
    let srcPath = srcPath.Trim('\\', '/') + "/"
    let namePattern = namePattern.[1 + namePattern.LastIndexOf '/' ..]
    Directory.EnumerateDirectories (srcPath, "", SearchOption.AllDirectories)
    |> Seq.collect (fun x -> Directory.EnumerateFiles (x, namePattern, SearchOption.TopDirectoryOnly))
    |> Seq.append (Directory.EnumerateFiles (srcPath, namePattern, SearchOption.TopDirectoryOnly))
    |> Seq.map (fun x -> x, x.[srcPath.Length..])

let stringReducing f (s: string seq) =
    match s with
    | x when Seq.isEmpty x -> ""
    | x -> x |> Seq.reduce f

let modifyExtensionName ext (name: string) =
    name.[..name.LastIndexOf '.'] + ext

let normalizeDirPath (path: string) =
    path.Trim('\\', '/') + "/"

let resolveInputFile (script: Script) fileName =
    normalizeDirPath script.scriptFile.Directory.FullName + fileName
    |> FileInfo
