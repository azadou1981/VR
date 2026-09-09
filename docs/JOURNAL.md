# Journal de bord — OASIS

## 2026-09-09

Fait :
- Déplacé `CLAUDE.md` et `.claude/settings.local.json` de `Assets/` vers la racine du projet. Ils étaient au mauvais endroit : Claude Code ne lit `CLAUDE.md` que depuis le dossier de travail, donc le contexte du projet n'était chargé à aucune session. Les chemins dans `settings.local.json` (`Edit(Assets/Scripts/**)`, `Edit(.gitignore)`, ...) étaient déjà écrits relativement à la racine, ils ne résolvaient rien depuis `Assets/`.
- Créé ce journal.
- État du projet vérifié sur disque (voir « Prochaine étape »).

Bloqué sur :
- Rien.

Prochaine étape — fin de la phase 0 :
- `git init`, `.gitignore` Unity, `.gitattributes` avec Git LFS (les `.unity`, `.fbx`, textures et audio doivent passer en LFS **avant** le premier commit, sinon l'historique est à refaire).
- Créer l'arborescence `Assets/Scripts/<Domaine>/`.
- Valider la chaîne Quest → PC (Meta Horizon Link) et un premier grab en VR.

Décisions prises :
- Aucune décision d'architecture. Réseau (Mirror vs FishNet) toujours non tranché, comme prévu en phase 2.

Constaté :
- Unity `6000.3.23f1` — conforme à la stack figée (6.3 LTS).
- Template VR en place : `Assets/Scenes/SampleScene.unity`, `BasicScene.unity`, `VRTemplateAssets/` (14 scripts d'exemple Unity, pas du code à nous).
- Pas encore de dépôt Git, pas de `Assets/Scripts/`, pas de `backend/`.
