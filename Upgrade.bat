@echo off
echo Upgrade procedure starting.
set /p comportA="Enter Port:"
rem if not exist %hexfile% goto error
if %comportA%==NONE goto nodevice
echo COM port for device is detected as %comportA%.
echo.
echo Starting Upgrade.....
"%~dp0"\esptool.exe --chip esp32s2 --port "%comportA%" --baud 460800 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_freq 80m --flash_size detect 0x1000 "%~dp0\bootloader_dio_80m.bin" 0x8000 "%~dp0\partitions.bin" 0xe000 "%~dp0\boot_app0.bin" 0x10000 "%~dp0\firmware.bin"
goto upgradedone
:nodevice
echo No matching module found, you should connect the module you want to upgrade.
goto end
:nobldevice
echo Reset into bootloader failed, please try again...
goto end
:error
Echo Missing parameter or file, you should provide the full filename of an existing .hex file you want to use.
goto end
:upgradedone
echo.
echo Upgrade done!
:end
pause