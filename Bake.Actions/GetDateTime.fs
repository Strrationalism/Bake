module Bake.Actions.GetDateTime

open Bake

[<BakeAction>]
let GetDateTime = {
    help = "将环境变量设置为时间日期字符串"

    usage = [
        """DateTime <$变量>"""
        """DateTime <$变量> <格式字符串>"""
    ]

    example = [
        """DateTime $DateTime "YYYY-MM-DD-hh-mm-ss" """
    ]
    
    action = fun ctx script ->
        let len = script.arguments.Length
        if len <> 2 && len <> 1 then raise <| Action.ActionUsageError "DateTime must recive 1 or 2 argument(s)."

        let format = 
            script.arguments |> List.tryItem 1 
            |> Option.map (Utils.applyContextToArgument ctx)
            |> Option.defaultValue "YYYY-MM-DD--hh-mm-ss"
        let now = System.DateTime.Now
            
        let variableName = script.arguments.[0].Trim().TrimStart('$')
        let value = 
            format
                .Replace("YYYY", string now.Year)
                .Replace("MM", string now.Month)
                .Replace("DD", string now.Day)
                .Replace("hh", string now.Hour)
                .Replace("mm", string now.Minute)
                .Replace("ss", string now.Second)
                .Replace("ms", string now.Millisecond)
        Seq.empty,
        Set.setVariable ctx variableName value
}


