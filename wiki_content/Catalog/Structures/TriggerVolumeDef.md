# TriggerVolumeDef
**Size:** `0x88`
**Count:** `0x17`

## Structure
| Offset | DataType | Name |
| :--- | :--- | :--- |
| `0x0C` | `key` | **onEnter** |
| `0x1C` | `key` | **onExit** |
| `0x2C` | `key` | **onStay** |
| `0x30` | `nullable` | **events** |
| `0x34` | `bool` | **useGameObjectDimensions** |
| `0x35` | `bool` | **isKinematic** |
| `0x38` | [`Enum<TriggerShape>`](../Enums/TriggerShape) | **shape** |
| `0x3C` | [`cSPVector3`](../Structures/cSPVector3) | **offset** |
| `0x48` | `float` | **timeToActivate** |
| `0x4C` | `bool` | **persistentTimer** |
| `0x4D` | `bool` | **triggerOnceOnly** |
| `0x4E` | `bool` | **triggerIfNotBeaten** |
| `0x50` | [`Enum<TriggerActivationType>`](../Enums/TriggerActivationType) | **triggerActivationType** |
| `0x54` | `charptr` | **luaCallbackOnEnter** |
| `0x58` | `charptr` | **luaCallbackOnExit** |
| `0x5C` | `charptr` | **luaCallbackOnStay** |
| `0x60` | `float` | **boxWidth** |
| `0x64` | `float` | **boxLength** |
| `0x68` | `float` | **boxHeight** |
| `0x6C` | `float` | **sphereRadius** |
| `0x70` | `float` | **capsuleHeight** |
| `0x74` | `float` | **capsuleRadius** |
| `0x78` | `bool` | **serverOnly** |

