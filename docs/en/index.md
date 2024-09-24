---
layout: home

hero:
  name: "Ayaka"
  text: "A lightweight development kit"
  tagline: Built to help developing applications using .NET
  actions:
    - theme: brand
      text: Getting started
      link: /guide/getting-started
    - theme: alt
      text: GitHub
      link: https://github.com/Xzelsius/Ayaka
  image:
    light: /logo-black.svg
    dark: /logo-white.svg
    alt: Ayaka

features:
  - title: Nuke Build Components
    details: Various opinionated build components for simpler build automation using NUKE.
  - title: Soon&trade;
    details: Keep an eye out for upcoming features and enhancements.
---

<style>
:root {
  --vp-home-hero-name-color: transparent;
  --vp-home-hero-name-background: -webkit-linear-gradient(120deg, #298459 30%, #E3CF46);

  --vp-home-hero-image-background-image: linear-gradient(-45deg, #298459 50%, #E3CF46 50%);
  --vp-home-hero-image-filter: blur(44px);
}

.VPHomeHero .image-src {
  max-width: 150px;
  max-height: 150px;
}

@media (min-width: 640px) {
  :root {
    --vp-home-hero-image-filter: blur(56px);
  }

  .VPHomeHero .imgage-src {
    max-width: 200px;
    min-height: 200px;
  }
}

@media (min-width: 960px) {
  :root {
    --vp-home-hero-image-filter: blur(68px);
  }

  .VPHomeHero .image-src {
    max-width: 250px;
    max-height: 250px;
  }
}
</style>
