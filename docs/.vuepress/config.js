module.exports = {
    title: 'Lms微服务框架在线文档',
    description: 'Lms是一个旨在通过.net平台快速构建微服务开发的框架。具有稳定、安全、高性能、易扩展、使用方便的特点。',
    port: 8081,
    plugins: [['social-share', {
        networks: ['qq', 'weibo', 'douban', 'wechat', 'email', 'twitter', 'facebook', 'reddit', 'telegram'],
        email: '1029765111@qq.com',
        fallbackImage: 'https://gitee.com/liuhll2/lms/raw/main/docs/.vuepress/public/assets/logo/logo.word.svg',
        autoQuote: true,
        isPlain: true,
    }]],
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
        ],
        ["meta", { name: "baidu-site-verification", content: "code-q0r0KSL5ZB" }],
        [
            "script",
            {
                src: "/assets/js/autopush-baidu.js"
            }
        ],
        [
            "meta",
            {
                name: "360-site-verification",
                content: "865fffbead89371a9a7cd196f721e64c"
            }
        ],
        [
            "script",
            {
                src: "/assets/js/autopush-360.js"
            }
        ],
        [
            "script",
            {},
            `
(function(){
var src = "https://s.ssl.qhres2.com/ssl/ab77b6ea7f3fbf79.js";
document.write('<script src="' + src + '" id="sozz"><\/script>');
})();
            `
        ],

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
                        'dev-docs/object-mapping',
                        'dev-docs/dependency-injection',
                        'dev-docs/ws',
                        'dev-docs/gateway',
                        'dev-docs/microservice-architecture',
                        'dev-docs/samples',
                        'dev-docs/noun-explanation',

                    ]
                },
            ],
            '/blog/': [
                {
                    title: '博文',
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
                    collapsable: false,
                    children: [
                        ''
                    ]
                },

            ]
        }
    }
}