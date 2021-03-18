---
home: true
heroText: Lms微服务
tagline:  Lms框架在线文档
actionText: 快速上手 →
features:
- title: 安全
  details: 1.只有请求来源于网关,才被认为是合法的请求。集群外部无法通过rpc端口与主机直接通信;  2.服务内部通信过程中,同一个集群只有配置的token一致, 通信才会被被认为是合法的。

- title: 稳定
  details: 1. 使用.net平台提供的主机寄宿应用,保证服务能够稳定的运行。

- title: 高性能
  details: 1. 基于高性能的通信框架dotnetty实现的rpc通信框架; 2. rpc通信过程中支持缓存拦截

footer: MIT Licensed | Copyright © 2021-present Liuhll
---