# Mall object model candidate r18-1

Input: `4ff3e341c228592a0cd152eb413b751e9d18d575` (r17 material1). This is an isolated, unintegrated representative mall bay. The original checkout and public r15 were not changed.

## What is included

- FragranceCase: bevelled stone top, oak flutes, drawers/handles, aluminium rails, thin transparent glazing, two shelves of shaped fragrance flacons and labels, two shadowless vertex lights, intact-only contact shade.
- FragranceDisplay: framed glass showcase, raised product plinth, large flacon with neck, cap, liquid and label.
- BoutiqueEntrance: ceramic jambs, metal ribs, open glass leaves, continuous dark fascia with brass edging and geometric EAU letters. Centre opening stays clear.
- BoutiqueBay: one asymmetrical store backdrop with neutral walls, oak shelf wall, cool stone floor, overhead coffers and warm shelf/ceiling diffusers. It supplements one bay at x=76; it is not a replacement of the whole mall.
- Separate round and square flacon Prefabs. Six model Prefabs total, 12 materials, three 256-square authored maps, Unity mesh assets and OBJ/MTL exports. Unity materials/shaders are authoritative; OBJ/MTL preserves geometry, UVs, normals, grouping and approximate base tint/transparency.

No package, external model, paid asset or new tool was installed. No crab model, camera, VFX or physics implementation was changed.

## Integration (integrator applies; not applied in this candidate)

1. Copy `Assets/MallModelPackR18` and `Assets/MallModelPackR18.meta`, preserving every `.meta`, into the r17 project. Or cherry-pick the output commit containing only these added assets.
2. In `City2Data.Build`, immediately after `SectionWorldArtR16.ApplyBreakables(built)`, call `MallModelPackR18.ApplyToRoot(built)`, under the same SectionWorldArtR16.Enabled condition. Apply after all existing surface overrides, before the host creates/spawns fragments.
3. In `CrabFlightGame.MakeField`, immediately after `SectionWorldArtR16.BuildBackground(...)`, call `MallModelPackR18.AddBay(transform)`, once, under the same condition.
4. `optional-runtime-hooks.patch` contains these two connections for exact r17. It has NOT been applied to shared Runtime here. If the integrator has edited either line, insert the two calls manually rather than replacing their current file.
5. Keep the rest of the r17 material/physics code and placement JSON. The adapter is deliberately restricted to the three IDs below and does not mirror them into all sections.

`ApplyToRoot` is idempotent per target. `Restore(visual)` restores the original renderers. `AddBay` returns the single background root so the host may destroy it on teardown; do not call AddBay repeatedly during reset. There is no Update/FixedUpdate loop in the adapter.

## Existing target and fragment mapping

| Existing placement | Model | Existing host material/event | Visual parts attached to existing shard indices |
| --- | --- | --- | --- |
| street3-+1-10-market_stall (x72.5) | FragranceCase | glass | 0/1 skids; 2 oak/stone base; 3 drawer block; 4 glass/frame/header and upper samples; 5 shelf and lower samples |
| street3-+1-11-market_crate (x76) | FragranceDisplay | metal | -1 fixed rails; 0 base; 1 upper rim; 2 front pane; 3 plinth/product/back pane; 4/5 side frames |
| street3-+1-12-street_awning (x79.5) | BoutiqueEntrance | metal | -1 jambs/open leaves/rail; 0..5 existing header cells |

Each Part mesh stays within the original corresponding shard AABB. Existing root transforms, collider centres/sizes, mass suggestions, pivots and destruction thresholds remain authoritative. No model adds a Collider or Rigidbody. Fragment clones reuse existing host pivots; intact-only contact shades and local lights do not follow flying fragments. Fixed group -1 follows the host's existing scaffold-collapse behavior.

The host still chooses one physical event material per object. Visual glass in the display and entry does not create a new glass contact rule, and perfume bottles are attached to existing shards rather than independently simulated. Splitting those physical groups/material events would be a separate integration proposal, not part of this candidate. The original box collision approximations also remain; transparent door leaves are visually open at the sides.

The background floor top is y=.046 (existing r17 floor is about .042); no floor collider is added. Existing crab shadow remains in front. Shelf/contact shade is authored vertex shading/alpha, not a realtime cast-shadow claim. The scene keeps r17 global lighting and disabled shadows.

## Reproduction and evidence

Unity 6000.3.23f1, Built-in pipeline. Open `Assets/MallModelPackR18/Showcase.unity` for the model inspection scene. Do not open another Unity Editor on a project already running.

- `BoutiqueModelBuilderR18.BuildAll`: regenerates Prefabs/assets and exports OBJ/MTL to `<project>/ModelExports/MallObjectR18`.
- `BoutiqueModelPreviewR18.BuildAndReview`: regenerates, audits and renders the subject using existing r17 runtime; writes images/report under `<project>/ModelReviews/MallObjectR18Review`.
- This editor-only fixture saves its own `Editor/PreviewSession.unity`, then uses the existing `ReviewRouteSegmentR11` to deliberately position an actual collision near the chosen case. It does not change gameplay physics. It is NOT proof of natural reach, browser control, new physics correctness, or a complete playthrough.
- Before/intact/after-contact landscape images use the actual game camera at 844:390 (also double-resolution exports); the same camera transform/size is frozen for the comparison. Portrait is 390:844. These are direct Camera.Render outputs without HUD/browser chrome.
- The case really breaks via the existing simulation, not by manually setting broken meshes. The collision/body hash audit and break-event receipt are included.

## Delivery judgement and limits

Deliverable as an object-model integration candidate for this one bay. The glass, recognisable merchandise, bevels, drawers, material separation and front/back depth are visible in actual renders. It is not a finished-world or release approval. Neighbouring generic signage and fixtures remain in the normal game frame; the narrow portrait view shows only part of the upcoming case. The old header still uses six physical cells, and case destruction remains coarse at the existing shard grouping.

No WebGL/device FPS, natural reach, portrait gameplay input, sustained fragment budget, full-world regression or Owner acceptance is claimed. The added bay uses two shadowless ForceVertex point lights; reported triangle/submesh counts are asset counts, not measured draw-call or FPS results. A final lighting/art pass and surrounding prop consistency remain integration decisions. No output was published.
