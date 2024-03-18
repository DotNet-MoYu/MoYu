## 使用指南

打开 `PowerShell` 终端，并进入当前目录，执行 `generate.ps` 脚本，将自动生成所有模板脚手架的 `.nupkg` 包。

```powershell
&"./generate.ps1"
```

然后执行 `nuget-push.ps1 -apikey xxx -source xxx`,默认 source="https://api.nuget.org/v3/index.json"
