Dir /B ..\Assets\Mods > name.txt

set /p a=<name.txt
echo %a%

.\adb.exe shell rm -r /sdcard/Android/data/com.CyDream.BattleTalent/files/Mods/%a%
.\adb.exe push ..\Assets\Mods\%a% /sdcard/Android/data/com.CyDream.BattleTalent/files/Mods/

Del name.txt