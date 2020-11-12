namespace Bake

open System.IO

type Script = {
    command : string
    arguments : string list

    scriptFile : FileInfo
}

module Script = 

    let trimLineComment (line: string) = 
        let x = line + "#"
        x.[..(x.IndexOf '#') - 1]

    let lines (text: string) = 
        text.Split ('\r', '\n')

    let trimLines (lines: string seq) = 
        lines
        |> Seq.map (fun x -> x.Trim())
        |> Seq.filter (not << System.String.IsNullOrWhiteSpace)


