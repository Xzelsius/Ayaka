import type { DefaultTheme } from 'vitepress';
import { defineConfig } from 'vitepress';

export const en = defineConfig({
  lang: 'en-US',
  description: 'A lightweight development kit built to help developing applications using .NET',

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
      },
    },

    editLink: {
      pattern: 'https://github.com/Xzelsius/Ayaka/edit/main/docs/:path',
      text: 'Edit this page on GitHub',
    },

    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © Raphael Strotz. All rights reserved.',
    },
  },
});

function nav(): DefaultTheme.NavItem[] {
  return [
    {
      text: 'Guide',
      link: '/guide/getting-started',
      activeMatch: '/guide/',
    },
    {
      text: 'Contributing',
      link: '/contributing/sources',
      activeMatch: '/contributing/',
    },
  ];
}

function guideSidebar(): DefaultTheme.SidebarItem[] {
  return [
    {
      text: 'Guide',
      collapsed: false,
      items: [
        { text: 'Getting started', link: 'getting-started' },
        { text: 'Packages', link: 'packages' },
        { text: 'Compatibility', link: 'compatibility' },
      ],
    },
    {
      text: 'Ayaka.Nuke',
      collapsed: true,
      items: [
        { text: 'Overview', link: 'packages/nuke' },
        {
          text: `Build Components`,
          items: [
            {
              text: 'General',
              link: 'packages/nuke/build-components/general',
            },
            {
              text: '.NET',
              link: 'packages/nuke/build-components/dotnet',
            },
            {
              text: 'VitePress',
              link: 'packages/nuke/build-components/vitepress',
            },
            {
              text: 'GitHub',
              link: 'packages/nuke/build-components/github',
            },
          ],
        },
        {
          text: 'Build Tasks',
          items: [
            {
              text: '.NET',
              link: 'packages/nuke/build-tasks/dotnet',
            },
            {
              text: 'GitHub',
              link: 'packages/nuke/build-tasks/github',
            },
          ],
        },
        { text: 'Utilities', link: 'packages/nuke/utilities' },
      ],
    },
    {
      text: 'Ayaka.MultiTenancy',
      collapsed: true,
      items: [
        { text: 'Overview', link: 'packages/multi-tenancy' },
        { text: 'Tenant Context', link: 'packages/multi-tenancy/tenant-context' },
      ],
    },
  ];
}

function contributingSidebar(): DefaultTheme.SidebarItem[] {
  return [
    {
      text: 'Overview',
      collapsed: false,
      items: [
        { text: 'Sources', link: 'sources' },
        { text: 'Renovate', link: 'renovate' },
      ],
    },
    {
      text: 'Contributing',
      collapsed: false,
      items: [
        { text: 'Code Guidelines', link: 'code-guidelines' },
        { text: 'Bug Reports', link: 'bug-reports' },
        { text: 'Feature Requests', link: 'feature-requests' },
        { text: 'Pull Requests', link: 'pull-requests' },
      ],
    },
  ];
}
