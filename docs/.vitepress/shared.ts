import { defineConfig } from 'vitepress';

export const shared = defineConfig({
  title: 'Ayaka',

  base: '/Ayaka/',

  rewrites: {
    'en/:rest*': ':rest*',
  },

  lastUpdated: true,
  cleanUrls: true,
  metaChunk: true,

  themeConfig: {
    logo: {
      dark: '/logo-white.svg',
      light: '/logo-black.svg',
      width: 24,
      height: 24,
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/Xzelsius/Ayaka' },
    ],

    search: {
      provider: 'local',
    },

    outline: 'deep',
  },
});
