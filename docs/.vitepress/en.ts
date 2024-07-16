import { type DefaultTheme, defineConfig } from "vitepress";

export const en = defineConfig({
    lang: 'en-US',
    description: "A lightweight development kit built to help developing applications using .NET",

    themeConfig: {
        nav: nav(),

        sidebar: {
            '/guide/': {
                base: '/guide/',
                items: guideSidebar(),
            },
            '/contributing/': {
                base: '/contributing/',
                items: contributingSidebar(),
            }
        },

        editLink: {
            pattern: 'https://github.com/Xzelsius/Ayaka/edit/main/docs/:path',
            text: 'Edit this page on GitHub'
        },

        footer: {
            message: "Released under the MIT License.",
            copyright: "Copyright © Raphael Strotz. All rights reserved."
        }
    }
});

function nav(): DefaultTheme.NavItem[] {
    return [
        {
            text: 'Guide',
            link: '/guide/getting-started',
            activeMatch: '/guide/'
        },
        {
            text: 'Contributing',
            link: '/contributing',
            activeMatch: '/contributing/'
        },
    ];
}

function guideSidebar(): DefaultTheme.SidebarItem[] {
    return [
        {
            text: 'Guide',
            collapsed: false,
            items: [
                { text: 'Compatibility', link: 'compatibility' }
            ]
        }
    ];
}

function contributingSidebar(): DefaultTheme.SidebarItem[] {
    return [];
}
