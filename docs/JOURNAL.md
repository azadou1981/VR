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

**Import validé en mode batch** (`Unity.exe -batchmode -quit`), sortie code 0. Aucune erreur de compilation, l'asmdef est reconnu et ses références résolues, les `.meta` sont générés et committés.

  Limite à connaître : l'assembly `Oasis` n'est pas encore *compilée*, puisqu'elle ne contient aucun script. Unity a validé la syntaxe et la résolution des références, pas leur usage réel. La vraie validation viendra avec le premier `.cs`. Les quatre noms d'assembly ont été relevés directement dans les asmdef des packages, pas devinés.

  À retenir : `Unity.exe -batchmode -quit -projectPath <projet> -logFile <log>` permet de vérifier qu'un projet compile sans ouvrir l'Éditeur. Utile pour ne pas te déranger à chaque changement de script. Ne marche pas si l'Éditeur est déjà ouvert (verrou `Temp/UnityLockfile`).

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

- **`OpenXRPackageSettings.asset` bouge tout seul.** À l'import, Unity a repointé trois entrées Android XR vers des doublons strictement identiques du même fichier (mêmes noms, mêmes états, mêmes versions) : du bruit, aucun effet. Attends-toi à revoir ce diff de temps en temps. Les trois viennent du package `androidxr-openxr`, donc ça disparaîtra avec son retrait.
- `m_SerializationMode: 2` (Force Text) — condition nécessaire pour que la fusion YAML marche. À ne pas changer.

### Remote et flux de travail

Dépôt distant en place : `azadou1981/VR`, branche `main`. Tout le travail de cette session y est poussé.

Première PR passée par une branche (`feat/asmdef-oasis`, PR #1, mergée puis supprimée). À noter pour la suite : **ne pas committer directement sur `main` si on veut une PR**, sinon il n'y a rien à comparer — une PR `main → main` n'existe pas. Committer sur une branche dès le départ.

GitHub CLI installé (`gh` 2.100.0, portée utilisateur, compte `azadou1981`, portées `repo` + `workflow`). Les prochaines PR se font en ligne de commande, sans passer par le navigateur.

Rappel : `git push` est dans la liste « demander d'abord » de `.claude/settings.local.json`. C'est volontaire, mais ça veut dire que Lou pousse à la main. À changer si ça devient pénible.

### Travailler sans le casque : le simulateur

XRI livre un **XR Interaction Simulator** qui pilote le rig VR à la souris et au clavier : tête, deux manettes, grab, téléportation. Ça permet de valider toute la couche interaction sans casque, et donc de continuer quand le Quest charge ou n'est pas dispo.

**Activé et vérifié en Play** : `m_AutomaticallyInstantiateSimulatorPrefab: 1`, prefab assigné, échantillon importé dans `Assets/Samples/XR Interaction Toolkit/3.5.1/XR Interaction Simulator/`.

Activation, sans toucher à la scène : **Edit → Project Settings → XR Plug-in Management → XR Interaction Toolkit**, cocher **« Use XR Interaction Simulator in scenes »**. Unity propose alors d'importer l'échantillon, répondre **Ok** — il assigne le prefab tout seul. Le simulateur s'instancie ensuite à chaque Play, et uniquement dans l'Éditeur (`m_AutomaticallyInstantiateInEditorOnly: 1`), donc jamais dans un build.

**Piège rencontré.** Cocher la case ne suffit pas : Unity garde le réglage en mémoire et ne l'écrit pas sur le disque. Sans **File → Save Project**, la case se retrouve décochée au redémarrage de l'Éditeur et le réglage n'est jamais committé. `Ctrl+S` n'y change rien, il n'enregistre que la scène.

**Migrations de format déclenchées par cette session.** L'ouverture du projet a réécrit `SampleScene.unity` : TextMeshPro ajoute `m_characterHorizontalScale` et `m_ActiveFontFeatures`, les Rigidbody passent de `m_Drag`/`m_AngularDrag` à `m_LinearDamping`/`m_AngularDamping`, les lumières changent de masques de calques, et XRI ajoute `m_UnparentTransformOnGrab`. Vérifié avant commit : **zéro GameObject ajouté ou supprimé**, aucun changement fonctionnel. Committé exprès, sinon la migration se rejoue à chaque session.

Le simulateur n'est pas inscrit dans la scène : il s'instancie au lancement, uniquement dans l'Éditeur. La scène reste donc propre.

À noter : ça valide les *interactions*, pas la chaîne Quest→PC. Le test casque reste nécessaire pour clore la phase 0.

### Premier code de la plateforme

`Assets/Scripts/Core/` — assembly `Oasis` :

- `SpawnPoint` : marque un endroit d'apparition, sa rotation donne la direction du regard.
- `PlayerSpawner` : y place le XR Origin au `Start`. Vise la hauteur du casque et pas le sol, sinon la caméra se retrouve enterrée — même calcul que la téléportation XRI.

`Assets/Scripts/Editor/` — assembly `Oasis.Editor`, éditeur uniquement :

- `SceneSetup` : prépare le point d'apparition dans `SampleScene`.
- `HubSceneBuilder` : génère `Assets/Scenes/Hub.unity` de zéro — sol téléportable de 50 m, lumière, rig VR, vignettage, point d'apparition. Idempotent.
- `ComfortSetup` : ajoute le vignettage anti-nausée et le branche aux systèmes de locomotion continus.

**Ce que le rig XRI fournit déjà**, vérifié en résolvant les GUID du prefab : snap turn, continuous turn, téléportation, saut, gravité, escalade, grab-move, interacteurs poke/ray/gaze. Ne pas réécrire tout ça. **Le seul manque était le vignettage**, ajouté.

**Décision : le code plateforme ne dépend jamais d'un échantillon Unity.** `DynamicMoveProvider` vient du sample Starter Assets, pas du package. On cible sa classe de base `ContinuousMoveProvider`, qui est dans le package : le polymorphisme retrouve l'instance, et réimporter ou supprimer l'échantillon ne casse rien.

Vignettage branché sur les déplacements continus seulement. La téléportation est instantanée et le snap turn est discret : les vignetter dégraderait la lisibilité sans réduire la nausée.

**Méthode de travail retenue.** Tout ce qui est répétitif passe par un script d'éditeur, lancé soit par le menu **Oasis** dans l'Éditeur, soit par moi en mode batch quand Unity est fermé. Lou ne clique plus dans la Hierarchy.

### Direction artistique du hub

Référence donnée par Lou : le hub de Ready Player One. Blanc, moderne, futuriste, très grand, avec des portails bleus vers les activités.

Traduction en géométrie, générée par `HubEnvironmentBuilder` : une rotonde à 12 pans, 20 m de rayon, murs de 10 m, **ouverte sur le ciel** — pas de plafond, c'est plus beau et ça ne coûte rien. Six portails bleus répartis un panneau sur deux.

Deux choix qui font la différence sur le rendu, et qui ne sont pas évidents :

Le blanc n'est pas un blanc pur mais `0.92` — à 1.0 les surfaces se clippent et tout le relief disparaît. L'ambiante est remontée à `1.25`, sinon les faces qui ne prennent pas le soleil virent au gris sale et le décor a l'air terne.

L'émissif des portails est en HDR au-delà de 1 (`0.2, 1.4, 3.2`). C'est ce qui déclenche le bloom. Et chaque portail porte **sa propre lumière ponctuelle** : sans elle le bleu resterait un rectangle plat, avec elle il déborde sur le sol blanc. C'est ça qui donne l'effet.

**Coût perf** (règle 4) : 26 MeshRenderer, 7 lumières dont 6 ponctuelles sans ombres, 2 matériaux. Négligeable, on est très loin du budget 90 fps. Les ombres sont désactivées sur les lumières de portail exprès : six sources d'ombres dynamiques coûteraient cher pour un gain visuel nul.

### Prochaine étape

Fin de la phase 0, ce qui reste passe par Lou et par le casque :

1. Brancher le Quest en Horizon Link et valider le rendu dans le casque.
2. Premier grab en VR dans `SampleScene`.

**Puis, immédiatement après :** retrait des 4 packages du template (analyse déjà faite plus haut, il ne reste qu'à l'appliquer). Le faire sur une branche, avec un import batch de contrôle avant et après.
