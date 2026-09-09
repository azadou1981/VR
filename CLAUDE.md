# Projet OASIS

Monde VR persistant et multijoueur, inspiré de l'OASIS de Ready Player One.
Principe directeur : **on construit une plateforme, puis des mondes s'y branchent.**

---

## Avec qui tu travailles

Lou. Développeur web : PHP/Symfony, SQL, Git. A écrit des mods Minecraft Fabric en Java.

**Débutant complet** en Unity, en 3D, et en réseau temps réel. C'est la zone où il faut le porter.

- Réponds en français, ton informel
- Ne réexplique pas les bases de la programmation, il les a
- **Fais explicitement le pont Java → C#/Unity** quand c'est pertinent : composition plutôt qu'héritage, propriétés, cycle `Awake`/`Start`/`Update`, absence de `main()`
- Quand tu écris un script, dis toujours sur quel GameObject il doit être attaché

---

## Stack — figée, ne pas dévier sans accord explicite

| Élément | Choix | Note |
|---|---|---|
| Éditeur | Unity **6.3 LTS** (6000.3.x) | ne pas migrer vers 6.4/6.6/6.7 sans accord |
| XR | OpenXR + XR Interaction Toolkit 3.5+ | jamais de version `-pre` |
| Cible | PCVR Windows | Quest via Meta Horizon Link |
| Réseau | Mirror ou FishNet — **non tranché** | décision phase 2 |
| Backend | Symfony + PostgreSQL + JWT | phase 3 |
| Assets | Asset Store / Sketchfab / Quixel | on n'modélise pas à la main |

---

## Où on en est

**Phase 0 — mise en place.** En cours.

Fait : Unity 6000.3.23f1, template VR en place (`SampleScene`, `BasicScene`), dépôt Git sur `azadou1981/VR` avec LFS et fusion YAML Unity, assembly `Oasis` créée, `gh` installé.
Reste : valider la chaîne Quest→PC dans le casque, premier grab en VR. Puis retrait des packages du template, analyse déjà faite dans le journal.

Rappel Git : les scènes et prefabs restent en **texte**, jamais en LFS — c'est ce qui permet à `UnityYAMLMerge` de fusionner au lieu de forcer un choix entre deux versions.

*(Section à tenir à jour à chaque session. Détail dans `docs/JOURNAL.md`.)*

---

## Les phases

0. **Mise en place** — Unity installé, chaîne Quest→PC fonctionnelle, premier grab en VR, Git + LFS
1. **Premier monde solo** — locomotion, confort, interactions, environnement, 90 fps tenus
2. **Multijoueur** — couche réseau, serveur autoritaire, synchro tête + 2 mains, voix spatialisée
3. **Cœur OASIS** — API Symfony, auth, registre des mondes, portails, hub central, inventaire
4. **Avatar et identité** — avatars, IK corps entier, personnalisation persistée
5. **Contenu** — plusieurs mondes, progression, modération

Mondes visés à terme : tranchées 14-18, course automobile à New York, golf, et d'autres.
Les mondes « à parcourir » coûtent de l'art. Les mondes « à mécanique » coûtent un jeu entier chacun. Priorité : plusieurs mondes à parcourir, puis **un seul** monde à mécanique (le golf avant la course).

---

## Répartition du travail

**Toi :**
- Tous les scripts C#
- Le backend Symfony dans son intégralité
- Config Git, `.gitignore`, `.gitattributes`, `Packages/manifest.json`
- Lecture et diagnostic des logs Unity
- Scripts d'éditeur quand ils peuvent supprimer du travail manuel à Lou

**Lou :**
- Tout ce qui passe par l'interface Unity : glisser des composants, positionner dans la Scene view, éclairage, matériaux, cases à cocher dans les Project Settings
- Tout ce qui se passe dans le casque
- Le jugement final : est-ce que ça *sent* juste

### Règle importante

Tu ne peux ni cliquer dans l'Éditeur, ni mettre le casque. Lou veut faire le minimum — donc **ne lui délègue jamais une décision, seulement des gestes**.

Quand une action passe par l'interface, donne la suite exacte de clics, dans l'ordre, avec les noms de menus exacts.

Mauvais : « configure ton XR Origin »
Bon : « Hierarchy → clic droit → XR → XR Origin (VR), puis supprime la Main Camera qui reste »

Si une chose peut se faire par un script d'éditeur plutôt qu'à la main, écris le script.

---

## Règles du projet

1. **Ne modifie jamais un `.unity` ou un `.prefab` à la main.** Ce sont des YAML fragiles ; une scène corrompue ne se répare pas. Si tu penses vraiment devoir le faire, demande d'abord.
2. **Le projet doit rester jouable en permanence.** Pas de commit qui casse la scène principale.
3. **Aucune dépendance payante** sans demander.
4. **Cible 90 fps.** Chaque système ajouté annonce son coût en perf.
5. **Le réseau vient tôt, pas à la fin.** Si Lou propose de repousser le multijoueur pour finir un monde d'abord, rappelle-lui pourquoi c'est un piège : un monde bâti en solo se réécrit entièrement pour passer en multi.
6. **Aucun monde ne se code en dehors des systèmes de la plateforme.**
7. Si tu n'es pas sûr d'un choix d'architecture qui engage la suite, pose la question au lieu de trancher seul.

---

## Conventions de code

- PascalCase pour les membres publics, `_camelCase` pour les champs privés
- `[SerializeField] private` plutôt que `public` pour l'exposition dans l'Inspector
- Un fichier, une classe
- Namespaces `Oasis.<Domaine>`
- Jamais de `GameObject.Find` ni de `GetComponent` dans `Update`
- `FindObjectsByType<T>(FindObjectsSortMode.None)` (les anciennes API sont supprimées en 6.3)
- `Rigidbody.linearVelocity`, plus `velocity`

## Arborescence

```
Assets/
  Scripts/<Domaine>/
  Scenes/
  Prefabs/
  Materials/
  Settings/
backend/          → Symfony, à partir de la phase 3
docs/
  JOURNAL.md
```

---

## Journal de bord

Tiens `docs/JOURNAL.md` à jour **à chaque session, sans qu'on te le demande** :

```
## 2026-09-XX
Fait :
Bloqué sur :
Prochaine étape :
Décisions prises :
```

C'est ce qui permet à Lou de reprendre après deux semaines d'interruption sans avoir à tout relire. Mets aussi à jour la section « Où on en est » de ce fichier quand une phase avance.
