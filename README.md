# "Bake" 构建系统

可扩展的构建系统。

## 脚本语言示例

##### Build.bake

```
Set $Output "../build"
Set $Temp "../build-temp"

Print-Split-Line "Compile Modules"

Parallel {
    Compile-Module "$Temp/modules" {
        First.fsx
        Second.fsx
        Third.fsx
        *-Module.fsx
    }
}


Import-Module {
    $Temp/modules/*.dll
}

Print-Split-Line "Build"

Parallel {
    Call {
        Bake.bake
        CompileScript.bake
    }
}


Encrypt

PowerShell {
    rem Some Power Shell Here
}

Cmd {
    rem Some CMD Here
}
```

##### Clean.bake
```
Delete "../build"
```

##### Rebuid.bake
```
Call {
    Clean
    Build
}
```

## 扩展性
* 可使用.NET DLL导入扩展模块
* 可现场下载并编译模块
