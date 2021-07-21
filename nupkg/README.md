# Silky打包脚本

## 参数

| 参数名 | 是否必填 | 备注 |
|:----|:-----|:-----|
| -repo | 否 | nuget仓库地址 |
| -push | 否 | 是否将surging组件推送到nuget仓库,缺省值为`false` |
| -apikey | 否 | nuget仓库apikey,如果设置了`-push $true`,必须提供`-apikey`值 |
| -build | 否 | 是否构建surging组件包,缺省值为`true` |

## 推送nuget仓库
```
.\pack.ps1 -push $true -apikey "$apikey"
```