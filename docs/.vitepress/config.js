module.exports = {
    title: 'Alba',
    description: 'Supercharged integration testing for ASP.Net Core web services',
    base: '/alba/',
    head: [],
    themeConfig: {
        logo: null,
        repo: 'JasperFx/alba',
        docsDir: 'docs',
        docsBranch: 'master',
        editLinks: true,
        editLinkText: 'Suggest changes to this page',

        nav: [
            { text: 'Guide', link: '/guide/' },
            { text: 'Gitter | Join Chat', link: 'https://gitter.im/JasperFx/alba?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge' }
        ],

        algolia: {
            apiKey: 'your_api_key',
            indexName: 'index_name'
        },

        sidebar: [
            {
                text: 'Getting Started',
                link: '/guide/'
            }
        ]
    }
}