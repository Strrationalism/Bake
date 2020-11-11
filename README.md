# "Bake" 构建系统

可扩展的构建系统。

## 基本概念
### Task
一个具体的构建任务，将会根据任务要求以及输入输出文件的修改日期决定该Task是否运行。

### Action
一个动作，将会根据参数创建对应的（一对多的）Task，它被写在脚本里。

### Module
.NET DLL文件，里面包含一些Action，供Bake脚本调用。

### Bake脚本
使用Import动作导入Module并调用其Action，用于描述真实的项目构建过程。

## 基本流程

1. 根据项目特点编写Modules（可选），Module包含一些Actions。
2. 为要构建的项目创建Build.bake，bake脚本使用Import导入需要的Modules后调用Action编写此脚本和子脚本。
3. 执行构建。

## 基本可用的Actions
* GetDateTime - 将日期和时间设置到变量中
* Delete - 删除文件和目录
* Atomic - 同步执行，如果块中失败，则停止所有后续操作
* Import - 导入Modules
* Action - 创建一个动作
* Include - 包含子Bake脚本文件
* Set - 用于设置一个环境变量
* Parallel - 此Action内的子脚本将会并行执行
* CreateDirectory - 创建文件夹
* Copy - 复制
* Zip - 打包

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
