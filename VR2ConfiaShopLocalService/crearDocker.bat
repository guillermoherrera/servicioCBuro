dotnet publish -c Release

docker build -t vr2confiashoprobotimage:latest -f Dockerfile .

docker login --username admin --password password  dockerreg.finconfia.com.mx:5000

docker tag vr2confiashoprobotimage dockerreg.finconfia.com.mx:5000/vr2confiashoprobotimage

docker push dockerreg.finconfia.com.mx:5000/vr2confiashoprobotimage:latest
