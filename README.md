# 基于windows的定时任务shell

#### 问题
1. 项目中的定时任务被需要
2. 除开业务逻辑部分，需要新建windows服务，并在此工程中完成日志记录，异常处理等等，此为重复劳动。

#### 解决问题
1. 基于windows的定时任务shell
2. 基于配置完成定时任务的调度

#### 使用到的第三方组件
1. AppSettings
2. Common.Logging.3.3.1
3. Quartz.2.5.0


#### 如何使用
1. 编译Common.WinServices，并发布
2. 将业务组件拷贝至Common.WinServices发布目录下

![image](https://github.com/GavinYao2018/first/blob/master/pic/winserviceshell/publish.png)
<br>
3. 修改ServiceNameSetting.xml的ServiceName为项目名称

![image](https://github.com/GavinYao2018/first/blob/master/pic/winserviceshell/service_name.png)
4. 在节点QuartzJob中配置调度策略
5. 若服务启动或停止需要执行其他任务，请配置节点ServiceStart，ServiceStop

![image](https://github.com/GavinYao2018/first/blob/master/pic/winserviceshell/config.png)
6. 将业务组件需要使用到的其他配置添加到Common.WinServices.exe.config

![image](https://github.com/GavinYao2018/first/blob/master/pic/winserviceshell/add_settings.png)
<br>
7. 安装服务，以管理员身份运行Install.cmd
<br>
8. 卸载服务，以管理员身份运行UnInstall.cmd

#### 配置说明
ServiceNameSetting.xml

节点 | 说明
---|---
ServiceName | 服务名称，不能与windows存在的服务名相同
DisplayName | 服务显示名称，最好不与windows存在的服务名相同
Description | 服务描述，最好是对业务的说明，可空


WinServicesConfig.xml

CommonWinService，属性ServiceName必须与ServiceNameSetting.xml的ServiceName一致； 


|节点 | 子节点 | 属性 | 说明|
|---|---|---|---|
| ServiceStart | | | 服务启动时执行|
| | MethodItem | | 执行任务项|
| | | Assembly | 程序集名称|
| | | MethodName | 要执行方法名，FullName |
| | | Parameters | 传递给调度任务方法的参数，使用英文逗号隔开<br>都为string类型，参数个数必须与方法参数一致 |
| ServiceStop | | | 服务停止时执行 |
| | MethodItem | | 同ServiceStart的MethodItem|
| QuartzJob | | | 调度任务 |
|  | JobItem |  | 调度任务项 |
| | | JobKey | 调度任务的key，唯一|
| | | Assembly | 程序集名称|
| | | ClassName | 要执行方法的类名，FullName<br>类必须继承Quartz.IJob，与MethodName二选其一 |
| | | MethodName | 要执行方法名，FullName<br>与ClassName二选其一 |
| | | QuartzCron | 调度策略，详情请百度：quartz cron |
| | | Parameters | 传递给调度任务方法的参数，使用英文逗号隔开<br>都为string类型，参数个数必须与方法参数一致 |
