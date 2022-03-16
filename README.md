# Auto-Backup to Anonfiles.com
Simple Auto Backup to anonfiles and send links to discord
**Download link** : https://github.com/csnon/Auto-Backup/releases/tag/v1

## 📸 Photo
![Screenshot](https://cdn.discordapp.com/attachments/950210834120998912/953581281378177034/unknown.png)
![Screenshotw](https://cdn.discordapp.com/attachments/875520426351165450/953583008194113546/unknown.png)

## ⭕ Nuget package
```
System.IO.Compression.ZipFile
```

## 💧 More info
you can send file using send();
```c#
 FileInfo F3 = new FileInfo(zipfile3); // checking file info
 long size3 = F3.Length;
 
 if (size3 < 8388608)
     {
       SendFile("**\n**", cek1, "rar", @file3, "winrar", "Non-Backup | Auto backup", textBox3.Text);
     }

```

## Subscribe https://youtube.com/c/csnon
