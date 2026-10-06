# Stylized maritime revision — research and implementation

## Primary references checked 2026-10-06

- Rare, Krzysztof Narkowicz / Andy Dennison / et al., [The Technical Art of Sea of Thieves, SIGGRAPH 2018](https://history.siggraph.org/wp-content/uploads/2022/09/2018-Talks-Ang_The-Technical-Art-of-Sea-of-Thieves.pdf). The published summary describes deep/subsurface color blending driven by view direction, sun direction and wave peaks. Our shader follows that visual principle, **not** their proprietary assets or FFT ocean implementation. This project's bounded multi-wave surface is a simpler approximation, not equivalent tech or quality.
- [Poralu Marine gangways](https://www.poralu.com/en/products/gangways/): articulated access, deck continuity, handrails and transitions. Used as real-world construction reference, not as a licensed model source. Existing wooden gangway now has authored kit railings and ends in an opening cut into the supplied boat model. It is not a claim of engineering or safety certification.
- User screenshots of Sea of Thieves and RV There Yet are art direction only. No screenshots were repackaged as game textures; the sky bitmap was generated separately.

## Deliberate changes

- Remove the two large submerged rock slabs that made flat green shapes in the sea.
- Keep the actual local island shoreline depth tint, but separate it from open-ocean wave-crest turquoise light.
- Harbor sheltered-wave exposure and stronger open-ocean exposure use the same CPU/shader formula and network clock.
- Sparse crest lace replaces repeated white dots; no foam appears everywhere at the same strength.
- Extend sun shadows from 50 to 180 metres so nearby island props can cast shadows; keep Unity/URP versions unchanged.
- Author model imports remain visual children. The user-supplied boat is edited in Blender only for its boarding opening, with original source retained.

## Honest limitations

The generated sky is a panorama, not volumetric cloud simulation. Water is not an FFT fluid simulator, has no breaking-wave spray or planar reflection camera. The result needs screenshot and play review, not just parameter inspection. Computer Use native access failed with `Sky Computer Use native pipe startup failed`; Unity's actual Game view captures are the available visual inspection path. No claim of native mouse/keyboard verification follows from API tests.
