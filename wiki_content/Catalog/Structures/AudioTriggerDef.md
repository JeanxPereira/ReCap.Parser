# AudioTriggerDef
**Size:** `0x20`
**Count:** `0x8`

## Structure
| Offset | DataType | Name |
| :--- | :--- | :--- |
| `0x00` | [`Enum<type>`](../Enums/type) | **type** |
| `0x10` | `key` | **sound** |
| `0x14` | `bool` | **bIs3D** |
| `0x15` | `bool` | **retrigger** |
| `0x16` | `bool` | **hardStop** |
| `0x17` | `bool` | **isVoiceover** |
| `0x18` | `float` | **voiceLifetime** |
| `0x1C` | `nullable` | **triggerVolume** |

