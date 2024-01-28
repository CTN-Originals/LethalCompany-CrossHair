# v1.1.0 (Latest)
### CrossHair Fading
- Added CrossHair fading. Suggested by [AlbinoGeek](https://github.com/AlbinoGeek).
Fading fields:
  - `isWalking`
  - `isSprinting`
  - `isJumping`
  - `isFallingFromJump`
  - `isFallingNoJump`
  - `isCrouching`
  - `isClimbingLadder`
  - `twoHanded`
  - `performingEmote`
  - `isUnderwater`
  - `inTerminalMenu`
  - `isPlayerDead`
  - `hasBegunSpectating`
  - `isHoldingInteract`
### Config
- Added
  - `CrossHairColor`: The color of the CrossHair in hex format (Default: `ffffff`)
  - `CrossHairOpacity`: The opacity of the CrossHair (Default: `80`)
  - `CrossHairFading`: Enables/Disables CrossHair fading (Default: `true`)
- Removed
  - (`CrossHairColor_RED` & `CrossHairColor_GREEN` & `CrossHairColor_BLUE` & `CrossHairColor_ALPHA`): Replaced by `CrossHairColor`
---
### v1.0.5
- The CrossHair GameOject is now instansiated from the quota monitor text, mostly to change the font. Suggested by [AlbinoGeek](https://github.com/AlbinoGeek)
---
### v1.0.4
- Added [CHANGELOG.md](https://github.com/CTN-Originals/LethalCompany-CrossHair/blob/main/CHANGELOG.md) as a seperate file
- Added the [LICENSE](https://github.com/CTN-Originals/LethalCompany-CrossHair/blob/main/LICENSE) file to the distributable folder
---
### v1.0.3
- Added CrossHairShadow<bool> config option, which enables/disables the CrossHair shadow. Suggested by [HazardousMonkey](https://github.com/HazardousMonkey) ([#1](https://github.com/CTN-Originals/LethalCompany-CrossHair/issues/1))
- Reduced the distance of the shadow from the CrossHair
---
### v1.0.2
- Fixed CrossHair overflow wrapping once the size is set to > 60
---
### v1.0.1
- Added configuation options
- - CrossHairText: The text to display as the CrossHair
- - CrossHairSize: The size of the CrossHair
- - CrossHairColor_RED: The red value of the CrossHair color
- - CrossHairColor_GREEN: The green value of the CrossHair color
- - CrossHairColor_BLUE: The blue value of the CrossHair color
- - CrossHairColor_ALPHA: The alpha value of the CrossHair color
## v1.0.0 (Released 10-12-2023)
- Initial release