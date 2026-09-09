# Journal de bord — OASIS

## 2026-09-09

### Fait

**Contexte Claude réparé.** `CLAUDE.md` et `.claude/settings.local.json` étaient dans `Assets/` au lieu de la racine. Claude Code ne lit `CLAUDE.md` que depuis le dossier de travail : le contexte du projet n'était chargé à aucune session. Les chemins de `settings.local.json` (`Edit(Assets/Scripts/**)`, `Edit(.gitignore)`, …) étaient déjà écrits relativement à la racine et ne résolvaient rien depuis `Assets/`. Déplacés tous les deux. Effet de bord réglé au passage : tout ce qui traîne dans `Assets/` finit importé par Unity avec un `.meta`.

**Dépôt Git initialisé** — branche `main`, 1409 fichiers, premier commit `264a2a0`.

- `.gitignore` : `Library/` (2,4 Go), `Temp/`, `Logs/`, `UserSettings/`, et les `.csproj`/`.slnx` qu'Unity régénère à chaque ouverture.
- `.gitattributes` : 169 binaires en LFS (png, fbx, exr, wav, tif, webm, ttf, mp3), vérifié fichier par fichier avant le commit — zéro binaire stocké en clair.
- `core.autocrlf` forcé à `false` **localement**. Le gitconfig système de Git for Windows le mettait à `true`, ce qui réécrivait en CRLF des YAML qu'Unity relit en LF.
- Driver de fusion `UnityYAMLMerge` branché, avec `--fallback-none`.
- `Assets/Scripts/` créé.

### Décisions prises

**Les scènes et prefabs restent en texte, PAS en LFS.** (Je m'étais trompé dans la note précédente de ce journal, qui disait l'inverse.) Un `.unity` en LFS devient un blob opaque : le moindre conflit se résout en « tu gardes ta version ou la sienne », donc quelqu'un perd son travail. En texte, `UnityYAMLMerge` fusionne réellement les deux. C'est le seul cas où on accepte de gros fichiers texte dans le dépôt.

`--fallback-none` sur le driver de fusion : si UnityYAMLMerge n'y arrive pas, la fusion **échoue** au lieu de retomber sur une fusion texte ligne à ligne, qui produirait un YAML syntaxiquement cassé. Un merge en échec se rejoue ; une scène corrompue, non (règle 1 du projet).

Réseau (Mirror vs FishNet) : toujours non tranché, comme prévu en phase 2.

### Constaté, non traité

- **Aucun remote.** Le dépôt est local. Il faut créer le dépôt distant et pousser — voir « Prochaine étape ». Attention : GitHub ne donne qu'**1 Go de stockage LFS gratuit**, et un projet VR avec des assets achetés le dépasse vite.
- **Packages inutiles pour du PCVR** dans `Packages/manifest.json` : `com.unity.xr.arfoundation`, `com.unity.xr.androidxr-openxr`, `com.unity.xr.meta-openxr`, `com.unity.learn.iet-framework`. Ils viennent du template. Les retirer allégerait les builds, mais le template les référence peut-être dans ses scènes : à faire proprement, pas à l'arrache, sinon on casse la règle 2 (le projet reste jouable en permanence).
- Config XR déjà correcte pour PCVR : `OculusTouchControllerProfile` activé sur Standalone, loader OpenXR en place.
- `m_SerializationMode: 2` (Force Text) — condition nécessaire pour que la fusion YAML marche. À ne pas changer.

### Prochaine étape

Fin de la phase 0, ce qui reste passe par Lou :

1. Créer le dépôt distant, puis `git remote add origin …` et `git push -u origin main`.
2. Brancher le Quest en Horizon Link et valider le rendu dans le casque.
3. Premier grab en VR dans `SampleScene`.
