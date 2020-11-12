# "Bake" 构建系统

可扩展的构建系统。

## 基本概念
### Task
一个具体的构建任务，将会根据任务要求以及输入输出文件的修改日期决定该Task是否运行。

### Action
一个动作，将会根据参数创建对应的（一对多的）Task，它被写在脚本里。

### Module
.NET DLL文件，里面包含一些Action，供Bake脚本调用。

### Bake Script
使用Import动作导入Module并调用其Action，用于描述真实的项目构建过程。

## 基本流程

1. 根据项目特点编写Modules（可选），Module包含一些Actions。
2. 为要构建的项目创建Build.bake，bake脚本使用Import导入需要的Modules后调用Action编写此脚本和子脚本。
3. 执行构建。

## 规则
1. 输入文件全部使用相对路径（相对于脚本文件）
2. 输出文件全部使用绝对路径（可使用$StartDir获取启动脚本的路径以设置$Output变量）

## 脚本语言示例

##### Publish.bake
这个脚本用于构建Bake构建系统本身，并且将会在publish文件夹下整理各个平台的二进制文件、NuGet包和源代码。

```
Title "Bake Building System"

Print "=== Bake Building System ==="

Set $Output "publish"
GetDateTime $DateTime

Set $PublishToPlatform "dotnet publish Bake/Bake.fsproj -c Release"
Run {
	$PublishToPlatform -p:PublishTrimmed=true -f net472
	$PublishToPlatform -r osx-x64 -f netcoreapp3.1
	$PublishToPlatform -p:PublishTrimmed=true -r linux-x64 -f netcoreapp3.1

	dotnet pack Bake.Core/Bake.Core.fsproj -o $Output -c Release
	dotnet pack Bake.Actions/Bake.Actions.fsproj -o $Output -c Release
	dotnet pack Bake.Parser/Bake.Parser.fsproj -o $Output -c Release
}

CreateDirectory {
	$Output
	$Output/src	
	$Output/src/Bake
	$Output/src/Bake.Core
	$Output/src/Bake.Actions
	$Output/src/Bake.Parser
}

Action CopyProject projectName {
	Copy "$Output/src/projectName" {
		projectName/*.fs
		projectName/*.fsproj
		projectName/Properties
	}
}

Parallel {
	Zip "$Output/Bake-bin-windows-$DateTime.zip" {
		Bake/bin/Release/net472/publish/*
	}
	
	Zip "$Output/Bake-bin-osx-x64-$DateTime.zip" {
		Bake/bin/Release/netcoreapp3.1/osx-x64/publish/*
	}
	
	Zip "$Output/Bake-bin-linux-x64-$DateTime.zip" {
		Bake/bin/Release/netcoreapp3.1/linux-x64/publish/*
	}
	
	Atomic {
		Parallel {
		
			CopyProject Bake.Core
			CopyProject Bake.Actions
			CopyProject Bake.Parser
			CopyProject Bake
						
			
			Copy "$Output/src" {
				LICENSE
				Bake.sln
				Package.bake
				README.md
			}
		}
		
		Zip "$Output/Bake-src-$DateTime.zip" {
			$Output/src/*
		}
	}
}

```


## 扩展性
* 可使用.NET DLL导入扩展模块

