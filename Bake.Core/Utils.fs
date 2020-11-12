module Bake.Utils

open System.IO

let mapPathToOutputPath (srcPath: string) (namePattern: string) =
    let namePattern = namePattern.Replace('\\','/')
    let srcPath = srcPath.TrimEnd('\\', '/') + "/" + namePattern.[..(-1 + namePattern.LastIndexOf '/')]
    let srcPath = srcPath.Trim('\\', '/') + "/"
    let namePattern = namePattern.[1 + namePattern.LastIndexOf '/' ..]
    Directory.EnumerateDirectories (srcPath, namePattern, SearchOption.TopDirectoryOnly)
    |> Seq.collect (fun x -> Directory.EnumerateFiles (x, "", SearchOption.TopDirectoryOnly))
    |> Seq.append (Directory.EnumerateFiles (srcPath, namePattern, SearchOption.TopDirectoryOnly))
    |> Seq.map (fun x -> x, x.[srcPath.Length..])
