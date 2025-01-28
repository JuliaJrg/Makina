# Makina by Bad Gateway 

![Makina Logo](images/makina_logo.png)

## **Technologie et outils :**
- Unity
- C#
- Gitlab
- CI/CD
- Figma
- Notion

## **But du jeu :**
Zero-player game a pour but de suivre l’évolution d’une population sur un terrain donné, suivant plusieurs règles et états de jeu. Le joueur pourra donc analyser les résultats, re-définir les règles et les conditions de la simulation afin de voir l’évolution et conclure sur les résultats.

![NPC](images/npc.png)

## **Fin du jeu :**
En fonction des règles et des conditions établies avant le lancement de la simulation, le jeu s’arrête quand il n’y a plus d’NPC sur la map. A ce moment là, le joueur pourra essayer de comprendre ce qu’il s’est passé, définir des nouvelles règles et conditions pour la nouvelle simulation, et voir jusqu’où il pourra faire survivre sa population.

## **Règles par défaut :**
- Une journée in-game = 30sec IRL
- Survie d’NPC = doit manger 3 plantes/jour
- Reproduction d’NPC = doit manger 5 plantes/jour
- Génération ou Régénération de plantes = 1ère génération aléatoire. Générée ou régénérée à chaque jour. Si mangée, la plante est régénérée au même endroit. Si non mangée, nouvelle plante générée à côté.
- Reset de la faim à chaque journée

## **Attribut des NPCs principales :**
- Gestion de la faim
- Age

## **Attribut des NPCs secondaires :**
- Egoïsme : Selon son niveau d’Egoïsme → s’il a assez manger pour sa survie, il pourra donner à manger à un autre NPC
- Altruisme : Selon son niveau d’altruisme, un individu va plus ou moins aller au contact des autres individus
- Pacifisme : Selon son niveau de pacifisme, un individu sera plus ou moins hostile à l’égard des autres individus

## **Gestion de projet :**
- Serveur Gitlab à distance
- VPN WireGuard
- CI/CD (code_quality > unit_tests > build > deploy)

## **Teams :**

- [@romdmr](https://github.com/romdmr)
- [@QUsOK](https://github.com/QUsOK)
- [@LilCisaille](https://github.com/LilCisaille)
- [@JuliaJrg](https://github.com/JuliaJrg)
- [@Gungnir54](https://github.com/Gungnir54)
- [@nathangassmann](https://github.com/nathangassmann)
- [@MelvynDenisEpitech](https://github.com/MelvynDenisEpitech)
- [@iamhmh](https://www.github.com/iamhmh)


<img src="https://github.com/user-attachments/assets/99741351-a293-4409-8a82-080577691b8e" />
