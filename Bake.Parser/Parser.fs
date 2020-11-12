module Bake.Parser

open Bake
open Bake.Script
open FParsec
open System.IO

exception ParsingError of string

let parseScripts fileInfo script = 
    let spaces = anyOf [' ';'\t'] |> many |>> ignore    // 空格

    let command =   // 命令
        let isFirst = isLetter
        let is c = isLetter c || isDigit c || c = '-'
        spaces >>. (many1Satisfy2L isFirst is "command" .>> spaces) <?> "command"

    let simpleArgument =    // 简单参数
        let is c = isDigit c || isLetter c || isAnyOf ['$'; '\\'; '/'; '.'; '-'] c
        (many1SatisfyL is "simpleArgument" .>> spaces) <?> "simpleArgument"

    let warppedArgument =   // 被双引号包裹起来的参数
        let normalChar = satisfy (fun c -> c <> '\\' && c <> '"')
        let unescape c = match c with
                         | 'n' -> '\n'
                         | 'r' -> '\r'
                         | 't' -> '\t'
                         | c   -> c
        let escapedChar = pstring "\\" >>. (anyOf "\\nrt\"" |>> unescape)
        between (pstring "\"") (pstring "\"")
                (manyChars (normalChar <|> escapedChar))
        .>> spaces
        <?> "warppedArgument"

    // 被花括号包裹起来的参数
    let betweenBigBracketsWithBrackets, betweenBigBracketsWithBracketsRef = createParserForwardedToRef ()
    let betweenBigBrackets =
        between (pstring "{") (pstring "}") 
                (many (many1Satisfy (isNoneOf ['{';'}']) <|> betweenBigBracketsWithBrackets))
        |>> List.reduce (+)
        .>> spaces
        <?> "betweenBigBrackets"
    
    // 用来处理花括号中的花括号
    betweenBigBracketsWithBracketsRef :=
        spaces >>. betweenBigBrackets .>> spaces
        |>> fun x -> "{" + x + "}"
        <?> "betweenBigBracketsWithBrackets"
    
    let lineEnd = 
        let lineEnd = spaces .>> newline
        lineEnd
        |>> ignore
        <?> "lineEnd"

    // 单个任务的解析器
    let singleAction fileInfo = 
        (command .>>. many (simpleArgument <|> warppedArgument <|> betweenBigBrackets) .>> lineEnd
        |>> fun (cmd, args) ->
            {
                command = cmd.Trim()
                arguments = args
                scriptFile = fileInfo
            })
        <?> "singleAction"
    let actions fileInfo = many (many lineEnd >>. spaces >>. singleAction fileInfo .>> many lineEnd) .>> eof <?> "actions"

    let script = 
        script |> lines |> Seq.map (fun x -> trimLineComment(x).Trim()) 
        |> fun x -> Seq.append x [""]
        |> Seq.reduce (fun a b -> a + System.Environment.NewLine + b)

    match run (actions fileInfo) script with
    | Success(ls, _, _) -> Result.Ok ls
    | Failure(msg, a, _) -> Result.Error <| ParsingError msg


let parseFromFile (fileInfo: FileInfo) = 
    parseScripts fileInfo <| File.ReadAllText fileInfo.FullName