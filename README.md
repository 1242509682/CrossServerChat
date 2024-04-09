## Cross server chat 跨服聊天插件
```
插件源码来于GK,更新了玩家聊天时不会因为输入指令而被另一个服的玩家所监听，屏蔽了“.”与“/”字符  
加入了一个重载config指令
修复了监听不到玩家离开的消息，配置文件加了个：
  ```(json)
  "监听玩家断连时的离开消息": true,
  ```
```
## 命令
/reload -- 重载配置文件  

## Config 示例
```(json)
{  
  "Rest地址": [  
    "127.0.0.1:7878",//这里写对方的reset，格式："IP:Rest端口"，IP为其他服的IP，Port为其他服的Rest端口，不要把自己IP端口写上去  
    "221.130.33.52:7879"//两个服务器不需要在同一个电脑上也可以实现跨服聊天，在这里只需要输入另一台电脑的IP与Rest端口即可  
  ],  
  "Token令牌": "123456", //自己与其他服的rest端口密钥（密钥需统一），不知道的进config.json文件里最底下查看  
  "聊天格式": "[c/FF5E5E:开荒服] {0}{1}{2}: {3}",  
  "进入格式": "{0} 进入开荒服。",  
  "离开格式": "{0} 离开开荒服。",  
  "发送聊天": true,  
  "发送进入离开": true,
  "监听玩家断连时的离开消息": true,
  "接收消息": true  
}  
```
