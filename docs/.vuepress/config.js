module.exports = {
    title: 'Lms微服务框架在线文档',
    description: 'Lms是一个旨在通过.net平台快速构建微服务开发的框架。具有稳定、安全、高性能、易扩展、使用方便的特点。',
    port: 8081,
    head: [
        [
            "script",
            {},
            `
var _hmt = _hmt || [];
(function() {
  var hm = document.createElement("script");
  hm.src = "https://hm.baidu.com/hm.js?935dc174ecf32301b55bc431ff5f5b1a";
  var s = document.getElementsByTagName("script")[0];
  s.parentNode.insertBefore(hm, s);
})();
            `
        ]

    ],
    themeConfig: {
        logo: '/assets/logo/logo.png',
        docsRepo: 'liuhll/lms',
        docsBranch: 'main',
        docsDir: 'docs',
        editLinks: true,
        editLinkText: '编辑当前页',
        edit: {
            docsDir: 'src',
        },
        lastUpdated: '最后更新时间',
        nav: [
            { text: '首页', link: '/' },
            { text: '文档', link: '/lms/' },
            { text: '配置', link: '/config/' },
            { text: '博文', link: '/blog/' },
            {
                text: 'github', link: 'https://github.com/liuhll/lms'
            },
            {
                text: 'gitee', link: 'https://gitee.com/liuhll2/lms'
            },
        ],
        sidebar: {
            '/lms/': [
                {
                    title: '简介',
                    collapsable: false,
                    children: [
                        ''
                    ]

                },
                {
                    title: '开发文档',
                    collapsable: false,
                    children: [
                        'dev-docs/quick-start',
                        'dev-docs/host',
                        'dev-docs/modularity',
                        'dev-docs/appservice-and-serviceentry',
                        'dev-docs/service-registry',
                        'dev-docs/rpc',
                        'dev-docs/service-governance',
                        'dev-docs/caching',
                        'dev-docs/distributed-transactions',
                        'dev-docs/aggregation-and-gateway',
                        'dev-docs/microservice-architecture',
                        'dev-docs/object-mapping',
                        'dev-docs/samples',
                        'dev-docs/noun-explanation',

                    ]
                },
            ],
            '/blog/': [
                {
                    title: 'lms框架博文',
                    collapsable: false,
                    children: [
                        'transaction-design',
                        'lms-sample',
                        'lms-sample-order'
                    ]
                },

            ],
            '/config/': [
                {
                    title: 'lms框架配置',
                    collapsable: false,
                    children: [
                        ''
                    ]
                },

            ]
        }
    }
}