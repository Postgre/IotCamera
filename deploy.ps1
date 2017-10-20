dotnet publish -r linux-arm
./tools/sshdeploy monitor -s ".\src\Blink\bin\Debug\netcoreapp2.0\linux-arm\publish" -t "/home/pi/target" -h <ip> -u pi -w <password>