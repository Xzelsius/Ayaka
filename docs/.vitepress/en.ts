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
      text: 'Multi-tenancy <span class="VPBadge warning">preview</span>',
      collapsed: false,
      items: [
        { text: 'Overview', link: 'packages/multi-tenancy' },
        { text: 'Quickstart', link: 'packages/multi-tenancy/quickstart' },
        { text: 'Tenant context', link: 'packages/multi-tenancy/tenant-context' },
        {
          text: 'Tenant management',
          items: [
            { text: 'Managing tenants', link: 'packages/multi-tenancy/management/tenants' },
            { text: 'Caching', link: 'packages/multi-tenancy/management/caching' },
          ],
        },
        {
          text: 'ASP.NET Core',
          items: [
            { text: 'Request pipeline', link: 'packages/multi-tenancy/aspnetcore/pipeline' },
            { text: 'Tenant detection', link: 'packages/multi-tenancy/aspnetcore/detection' },
            { text: 'Tenant resolution', link: 'packages/multi-tenancy/aspnetcore/resolution' },
          ],
        },
        {
          text: 'Guides',
          items: [
            { text: 'Database-backed tenant store', link: 'packages/multi-tenancy/guides/database-store' },
            { text: 'Background work', link: 'packages/multi-tenancy/guides/background-work' },
            { text: 'Keep cached tenants in sync', link: 'packages/multi-tenancy/guides/cache-sync' },
          ],
        },
        { text: 'Troubleshooting', link: 'packages/multi-tenancy/troubleshooting' },
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
        { text: 'Documentation Guidelines', link: 'documentation' },
        { text: 'Bug Reports', link: 'bug-reports' },
        { text: 'Feature Requests', link: 'feature-requests' },
        { text: 'Pull Requests', link: 'pull-requests' },
      ],
    },
  ];
}
