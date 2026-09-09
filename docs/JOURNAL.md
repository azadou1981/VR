# Journal de bord — OASIS

## 2026-09-09

### Fait

**Contexte Claude réparé.** `CLAUDE.md` et `.claude/settings.local.json` étaient dans `Assets/` au lieu de la racine. Claude Code ne lit `CLAUDE.md` que depuis le dossier de travail : le contexte du projet n'était chargé à aucune session. Les chemins de `settings.local.json` (`Edit(Assets/Scripts/**)`, `Edit(.gitignore)`, …) étaient déjà écrits relativement à la racine et ne résolvaient rien depuis `Assets/`. Déplacés tous les deux. Effet de bord réglé au passage : tout ce qui traîne dans `Assets/` finit importé par Unity avec un `.meta`.

**Dépôt Git initialisé** — branche `main`, 1409 fichiers, premier commit `264a2a0`.

- `.gitignore` : `Library/` (2,4 Go), `Temp/`, `Logs/`, `UserSettings/`, et les `.csproj`/`.slnx` qu'Unity régénère à chaque ouverture.
- `.gitattributes` : 169 binaires en LFS (png, fbx, exr, wav, tif, webm, ttf, mp3), vérifié fichier par fichier avant le commit — zéro binaire stocké en clair.
- Fins de ligne forcées en LF via `* text=auto eol=lf` dans `.gitattributes`. Git for Windows poussait au CRLF de deux façons : `core.autocrlf=true` dans le gitconfig système, et `core.eol=native`. Les deux sont neutralisés localement, mais le réglage est posé dans `.gitattributes` parce que `.git/config` ne se clone pas — sinon le problème reviendrait à chaque nouvelle machine.
- Driver de fusion `UnityYAMLMerge` branché, avec `--fallback-none`.
- `Assets/Scripts/` créé, avec `Oasis.asmdef` (assembly `Oasis`, namespace racine `Oasis`, références XRI + XR Core Utils + XR Hands + Input System).

**Pourquoi une asmdef.** Sans elle, tout notre code atterrit dans `Assembly-CSharp` avec les scripts du template : chaque modification d'un seul fichier recompile l'ensemble, et l'attente grossit à chaque script ajouté. Avec elle, Unity ne recompile que `Oasis`. C'est aussi ce qui rend la frontière « plateforme / mondes » réelle plutôt que déclarative : un monde qui voudra contourner les systèmes de la plateforme devra le faire explicitement, en ajoutant une référence d'assembly. L'équivalent Fabric le plus proche est un sous-module Gradle, ou une frontière de module Java.

  Attention : le `.meta` de l'asmdef n'existe pas encore. Unity le génère à la prochaine ouverture de l'Éditeur — il faudra le committer (c'est lui qui porte le GUID de l'assembly).

### Décisions prises

**Les scènes et prefabs restent en texte, PAS en LFS.** (Je m'étais trompé dans la note précédente de ce journal, qui disait l'inverse.) Un `.unity` en LFS devient un blob opaque : le moindre conflit se résout en « tu gardes ta version ou la sienne », donc quelqu'un perd son travail. En texte, `UnityYAMLMerge` fusionne réellement les deux. C'est le seul cas où on accepte de gros fichiers texte dans le dépôt.

`--fallback-none` sur le driver de fusion : si UnityYAMLMerge n'y arrive pas, la fusion **échoue** au lieu de retomber sur une fusion texte ligne à ligne, qui produirait un YAML syntaxiquement cassé. Un merge en échec se rejoue ; une scène corrompue, non (règle 1 du projet).

Réseau (Mirror vs FishNet) : toujours non tranché, comme prévu en phase 2.

### Constaté, non traité

- **Aucun remote.** Le dépôt est local. Il faut créer le dépôt distant et pousser — voir « Prochaine étape ». Attention : GitHub ne donne qu'**1 Go de stockage LFS gratuit**, et un projet VR avec des assets achetés le dépasse vite.
- **Packages inutiles pour du PCVR**, hérités du template : `com.unity.xr.arfoundation`, `com.unity.xr.androidxr-openxr`, `com.unity.xr.meta-openxr`, `com.unity.learn.iet-framework`.

  Analyse faite par recoupement des GUID (tous les GUID exportés par ces packages, croisés avec tous les GUID référencés dans `Assets/`). **Aucune scène et aucun prefab ne les référence.** Les seules références sont :

  | Package | Référencé uniquement par |
  |---|---|
  | `learn.iet-framework` | 4 assets dans `VRTemplateAssets/Tutorial/` (le tutoriel d'accueil Unity) |
  | `androidxr-openxr` | `XR/Settings/OpenXRPackageSettings.asset`, entrées **Android** |
  | `meta-openxr` | `XR/Settings/OpenXRPackageSettings.asset` |
  | `arfoundation` | les 5 assets XR Simulation dans `XR/UserSimulationSettings/` et `XR/Loaders/` |

  Donc le retrait est sûr pour la jouabilité. Il laisserait en revanche des assets de réglages orphelins à supprimer, et `OpenXRPackageSettings.asset` serait réécrit par Unity. **Reporté jusqu'au test casque** : sans baseline connue-bonne, un problème dans le casque deviendrait indémêlable entre « les packages » et « autre chose ». À reprendre juste après.

  Note : XR Simulation (AR Foundation) simule des environnements AR, pas de la VR. L'outil sans casque qui nous concerne est le **XR Device Simulator**, livré avec XRI, qu'on garde.
- Config XR déjà correcte pour PCVR : `OculusTouchControllerProfile` activé sur Standalone, loader OpenXR en place.
- `m_SerializationMode: 2` (Force Text) — condition nécessaire pour que la fusion YAML marche. À ne pas changer.

### Prochaine étape

Fin de la phase 0, ce qui reste passe par Lou :

1. Créer le dépôt distant, puis `git remote add origin …` et `git push -u origin main`.
2. Brancher le Quest en Horizon Link et valider le rendu dans le casque.
3. Premier grab en VR dans `SampleScene`.
