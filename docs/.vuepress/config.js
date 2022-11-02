module.exports = {
    title: 'Silky微服务框架在线文档',
    description: 'Silky框架是一个旨在通过.net平台快速构建微服务开发的框架。具有稳定、安全、高性能、易扩展、使用方便的特点。',
    port: 8081,
    plugins: [['social-share', {
        networks: ['qq', 'weibo', 'douban', 'wechat', 'email', 'twitter', 'facebook', 'reddit', 'telegram'],
        email: '1029765111@qq.com',
        fallbackImage: 'https://gitee.com/liuhll2/silky/raw/main/docs/.vuepress/public/assets/logo/logo.svg',
        autoQuote: true,
        isPlain: true,
    }], ['@vuepress/active-header-links', {
        sidebarLinkSelector: '.sidebar-link',
        headerAnchorSelector: '.header-anchor'
    }],
        '@vuepress/back-to-top',
        '@vuepress/last-updated',
        '@vuepress/nprogress',
    [
        '@vuepress/pwa', {
            serviceWorker: true,
            updatePopup: true
        }
    ],
    ['@vuepress/search', {
        searchMaxSuggestions: 10
    }],
    [
        'seo', {
            siteTitle: (_, $site) => $site.title,
            title: $page => $page.title,
            description: $page => $page.frontmatter.description,
            author: (_, $site) => $site.themeConfig.author,
            tags: $page => $page.frontmatter.tags,
            twitterCard: _ => 'summary_large_image',
            type: $page => ['articles', 'posts', 'blog', 'silky'].some(folder => $page.regularPath.startsWith('/' + folder)) ? 'article' : 'website',
            url: (_, $site, path) => ($site.themeConfig.domain || '') + path,
            image: ($page, $site) => $page.frontmatter.image && (($site.themeConfig.domain && !$page.frontmatter.image.startsWith('http') || '') + $page.frontmatter.image),
            publishedAt: $page => $page.frontmatter.date && new Date($page.frontmatter.date),
            modifiedAt: $page => $page.lastUpdated && new Date($page.lastUpdated),
        }
    ],
    ['autometa', {
        site: {
            name: 'silky microservice framework',
        },
        canonical_base: 'http://docs.silky-fk.com',
    }],
    ['sitemap', {
        hostname: 'http://docs.silky-fk.com',
        // 排除无实际内容的页面
        exclude: ["/404.html"]
    }
    ]
    ],
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
        [
            "meta",
            {
                name: "keywords",
                content: 'silky微服务,silky文档,silky微服务框架,silky docs,微服务架构,.net微服务框架,dotnetcore微服务'
            }
        ],
        [
            "meta",
            {
                name: "viewport",
                content: 'width=device-width, initial-scale=1'
            }
        ],
        ["meta", { name: "baidu-site-verification", content: "code-cAZSIwloPN" }],
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
                content: "5da0cdaf9aaf8d8972302c8c7ecabb82"
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
    markdown: {
        lineNumbers: true,
        externalLinks:
            { target: '_blank', rel: 'noopener noreferrer' }
    },
    themeConfig: {
        logo: '/assets/logo/logo.png',
        docsRepo: 'liuhll/silky',
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
            { text: '文档', link: '/silky/' },
            { text: '配置', link: '/config/' },
            { text: '源码解析', link: '/source/' },
            { text: '博文', link: '/blog/' },
            {
                text: 'github', link: 'https://github.com/liuhll/silky'
            },
            {
                text: 'gitee', link: 'https://gitee.com/liuhll2/silky'
            },
        ],
        sidebar: {
            '/silky/': [
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
                        'dev-docs/templete',
                        'dev-docs/host',
                        'dev-docs/modularity',
                        'dev-docs/appservice-and-serviceentry',
                        'dev-docs/service-registry',
                        'dev-docs/rpc',
                        'dev-docs/service-governance',
                        'dev-docs/link-tracking',
                        'dev-docs/caching',
                        'dev-docs/identity',
                        'dev-docs/distributed-transactions',
                        'dev-docs/object-mapping',
                        'dev-docs/dependency-injection',
                        'dev-docs/lock',
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
                        'silky-microservice-profile',
                        'transaction-design',
                        'silky-sample',
                        'silky-sample-order'
                    ]
                },

            ],
            '/source/': [{
                title: '启动时',
                collapsable: false,
                children: [
                    'startup/host',
                    'startup/engine',
                    'startup/modularity',
                    'startup/service-serviceentry',
                    'startup/server'
                ]
            },
            {
                title: '运行时',
                collapsable: false,
                children: [
                    'runtime/routing'
                ]
            }],
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