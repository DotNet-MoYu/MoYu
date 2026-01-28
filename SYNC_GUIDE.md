# MoYu 同步/替换操作指南

本文档说明如何把 Furion 的最新代码同步到 MoYu，并完成 **命名/链接/图标/README** 的统一替换。

## 目标
- 同步 Furion 的 `v5-transition` 分支到 MoYu
- 命名空间/目录/文件/内容中所有 `Furion` 统一替换为 `MoYu`（含大小写）
- 保留你自己的：`README.md`、`README.zh.md`、`icon.png`
- `framework/Directory.Build.props` 与 `tools/MoYu.Tools/Directory.Build.props` 以你自己的模板为准，但 **TargetFrameworks** 和 **Version** 使用 Furion 的
- 不替换 `schemas` 和 `snks`

## 一键流程（推荐）
在 `D:\MoYu\MoYu` 下执行：
```
# 同步 Furion 最新分支 + 替换
powershell -ExecutionPolicy Bypass -File .\sync-upstream.ps1 \
  -Repo D:\MoYu\MoYu \
  -AllowDirty -AllowUnrelated -AutoStash -PreferUpstream -NormalizeLayout
```

这一步会：
- 拉取 Furion 的 `v5-transition`
- 合并到 MoYu
- 执行字符串替换和路径重命名
- 保留你的 README/icon（脚本内会做备份与回填）

## 必要的后置修正（每次同步后都要跑）
以下是“保证结果正确”的必跑步骤：

### 1) 保留 README
```
Copy-Item -Force .\README.md .\README.zh.md
```

### 2) 替换 icon
```
Get-ChildItem -Recurse -Filter icon.png -File | ForEach-Object {
  Copy-Item -Force D:\MoYu\icon.png $_.FullName
}
```

### 3) 用你的模板重写 Directory.Build.props
- 读取 `framework/Directory.Build.props` 中的 **TargetFrameworks** 和 **Version**
- 其它内容使用 `D:\MoYu\Directory.Build.props`（你的模板）
- 写回：
  - `framework/Directory.Build.props`
  - `tools/MoYu.Tools/Directory.Build.props`

（此步骤由脚本自动处理，但如果你手动改过，也可以重复执行脚本或用现成命令重新生成）

### 4) 统一目录结构（防止出现 Furion 与 MoYu 双份目录）
同步后如果看到 `Furion*` 和 `MoYu*` 同时存在，必须保留新同步的 Furion 版本并改名：
- 删除旧的 `MoYu*`
- 把 `Furion*` 改成 `MoYu*`

示例（只做一次即可，之后不会再出现）：
```
# framework
Get-ChildItem .\framework -Directory -Filter 'MoYu*' | Remove-Item -Recurse -Force
Get-ChildItem .\framework -Directory -Filter 'Furion*' | ForEach-Object {
  Rename-Item $_.FullName ($_.Name -replace '^Furion','MoYu')
}
```

### 5) 从 framework 解决方案中移除 tests（避免 TargetFramework 不兼容）
同步后 `framework\MoYu.sln` 里不要包含 tests 项目。

## 打包
```
# 先 build
dotnet build D:\MoYu\MoYu\framework\MoYu.sln -c Release

# 再 pack
dotnet pack D:\MoYu\MoYu\framework\MoYu.sln -c Release -no-build
```

## 常见问题
- **看到 Furion 目录**：说明同步后有旧 MoYu 目录未删除，按“统一目录结构”步骤处理。
- **README 被改**：说明同步前后置步骤没跑，直接按第 1 步修复。
- **icon 不是你的**：按第 2 步替换。
- **props 中文乱码**：用你的 `Directory.Build.props` 模板重新覆盖即可（第 3 步）。

---

如需更新此文档或脚本逻辑，告诉我即可。
