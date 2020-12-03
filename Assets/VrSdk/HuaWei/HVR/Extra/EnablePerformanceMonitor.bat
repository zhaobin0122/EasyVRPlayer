adb shell setenforce 0
adb shell chmod 777 /sys/class/thermal/thermal_zone0/temp
adb shell chmod 777 /sys/class/devfreq/gpufreq/cur_freq
adb shell chmod 777 /sys/class/power_supply/Battery/current_now
adb shell chmod 777 /sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq
adb shell setprop hvr.unity.prop 2
pause