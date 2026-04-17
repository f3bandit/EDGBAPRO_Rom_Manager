# EDGBAPRO Rom Manager

This build uses **ROM-content save detection** matching the original Python version.

## Detection behavior

The scanner reads each `.gba` ROM and searches for these signatures:

- `EEPROM_V` -> EEPROM
- `SRAM_V` -> SRAM
- `SRAM_F_V` -> SRAM
- `FLASH_V` -> FLASH
- `FLASH512_V` -> FLASH
- `FLASH1M_V` -> FLASH

If no signature is found, it defaults to **SRAM**, matching the Python app.

## Notes

- Ignores `goomba.gba`
- Ignores `pocketnes.gba`
- Uses `DataGridView` for the list to avoid the disappearing save-type text seen in the owner-drawn ListView build

## Build

```powershell
dotnet build
```

## Publish

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```
