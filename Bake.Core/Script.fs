namespace Bake

open System.IO
open FParsec

type Script = {
    command : string
    arguments : string list

    scriptFile : FileInfo
}

module Script = 
    let parseTasks fileInfo script = 
        let spaces = anyOf [' ';'\t'] |> many |>> ignore    // 空格

        let command =   // 命令
            let isFirst = isLetter
            let is c = isLetter c || isDigit c || c = '-'
            many1Satisfy2L isFirst is "command" .>> spaces

        let simpleArgument =    // 简单参数
            let is c = isDigit c || isLetter c || isAnyOf ['$'; '\\'; '/'; '.'] c
            many1SatisfyL is "simpleArgument" .>> spaces

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

        // 被花括号包裹起来的参数
        let betweenBigBracketsWithBrackets, betweenBigBracketsWithBracketsRef = createParserForwardedToRef ()
        let betweenBigBrackets =
            between (pstring "{") (pstring "}") 
                    (many (betweenBigBracketsWithBrackets <|> many1Satisfy (isNoneOf ['{';'}'])))
            |>> List.reduce (+)
            .>> spaces
        
        // 用来处理花括号中的花括号
        betweenBigBracketsWithBracketsRef :=
            spaces >>. betweenBigBrackets .>> spaces
            |>> fun x -> "{" + x + "}"
        
        let lineEnd = 
            let lineEnd = spaces .>> newline
            lineEnd <|> (spaces >>. pstring "#" >>. many (noneOf ['\n';'\r']) >>. lineEnd)
            |>> ignore

        // 单个任务的解析器
        let singleTask fileInfo = 
            command .>>. many (simpleArgument <|> warppedArgument <|> betweenBigBrackets) .>> lineEnd
            |>> fun (cmd, args) ->
                {
                    command = cmd
                    arguments = args
                    scriptFile = fileInfo
                }
        let tasks fileInfo = many (many lineEnd >>. singleTask fileInfo .>> many lineEnd) .>> eof

        match run (tasks fileInfo) script with
        | Success(ls, _, _) -> ls
        | Failure(x, _, _) -> failwithf "%s" x

    let parseFromFile (fileInfo: FileInfo) = 
        parseTasks fileInfo <| File.ReadAllText fileInfo.FullName

