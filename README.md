<!-- Common Project Tags:
command-line 
console-applications 
desktop-app 
desktop-application 
dotnet 
dotnet-core 
netcore 
netframework 
netframework48 
tool 
tools 
vbnet 
visualstudio 
windows 
windows-app 
windows-application 
windows-applications 
wall 
walls 
wallpaper 
wallpapers 
background 
backgrounds 
image 
images 
imagefile 
imagefiles 
image-file 
image-files 
pin 
windows-10 
 -->

<div align="center">
  <img src="/Images/App.ico" width="80" alt="WallPin Logo">
  
  <h1>WallPin</h1>

### A lightweight command-line utility to change the Windows desktop wallpaper with support for all layout styles (Fill, Fit, Center, etc.) and context menu integration via optional shell scripts.

</div>

------------------

## 👋 Introduction

**WallPin** lets you set any supported image as your Windows desktop wallpaper. It doesn't run in the background, it doesn't consume resources, and it works completely silently without even showing a console window. If an operation fails or the arguments are invalid, the tool will display a standard Windows message box showing the exact error details.

## 💡 Motivation

I decided to develop this application because Windows 10 and Windows 11 has a deeply annoying design flaw: when you use the built-in "Set as desktop background" menu command, Windows forces a global system refresh that resets the scrollbar position in all open File Explorer windows back to the very top.

If you are working inside a directory with thousands of files and you have an image selected near the middle or the bottom, Windows completely breaks your user experience by throwing you back to the top of the list. On top of that, the built-in menu command doesn't even let you choose the wallpaper style (Fill, Fit, Center, etc.).

**WallPin** solves both issues. It updates your desktop wallpaper instantly without affecting your open File Explorer windows, leaving your selection and scrollbars exactly where they are, and gives you full control over the wallpaper layout style.

## 🤖 Features

- Supports `center`, `tile`, `stretch`, `fit`, `fill`, and `span` using either their names or standard Windows IDs.
- Includes an optional batch-script file to add a clean "Set as desktop background" sub-menu directly into the Windows Explorer right-click menu for supported image files.

> [!NOTE]
> Adding these registry keys does not delete or corrupt the original built-in Windows menu commands; it simply overrides them safely. A separate batch-script file is also included to easily remove **WallPin**'s registry keys and restore built-in Windows menu commands back to its original state. 

> [!IMPORTANT]
> However, this software is provided "as-is", without any warranties. I take no responsibility for any registry issues caused by the use or misuse of this tool. Use it at your own risk.

## 🖼️ Screenshots

![screenshot](/Images/screenshot2.png)

![screenshot](/Images/screenshot1.png)

## 📝 Requirements

- Microsoft Windows OS with .NET Framework 4.8.

## 🚀 Getting Started

1. Navigate to the **[Releases page](https://github.com/ElektroStudios/WallPin-Set-Desktop-Wallpaper/releases/latest)**.
2. Download the latest `.zip` archive or the `.exe` setup installer, depending on your preference.
3. If you downloaded the `.zip` archive, extract its contents to your preferred directory.
   <br> 
   If you downloaded the `.exe` file, run it and follow the installation wizard.
4. Run the executable file to launch the application.

## 🔄 Change Log

Explore the complete list of changes, bug fixes, and improvements across different releases by clicking [here](/Docs/CHANGELOG.md).

## 💪 Contributing

Your contribution is highly appreciated!. If you have any ideas, suggestions, or encounter issues, feel free to open an issue by clicking [here](https://github.com/ElektroStudios/WallPin-Set-Desktop-Wallpaper/issues/new/choose). 

Your input helps make this Work better for everyone. Thank you for your support! 🚀

## 💰 Beyond Contribution

This work is distributed for educational purposes and without any profit motive. However, if you find value in my efforts and wish to support and motivate my ongoing work, you may consider contributing financially through the following options:

| Platform | How to Support |
| :---: | :--- |
| <a href="https://github.com/sponsors/ElektroStudios/"><img src="/Images/github_circle.png" width="64"></a> | **[Become my sponsor on GitHub](https://github.com/sponsors/ElektroStudios/)**<br>You can show me your support by contributing any amount you prefer, and unlocking rewards! |
| <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=E4RQEV6YF5NZY"><img src="/Images/paypal_circle.png" width="64"></a> | **[Make a PayPal Donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=E4RQEV6YF5NZY)**<br>You can donate to me any amount you like via PayPal. |
| <a href="https://codecanyon.net/item/elektrokit-class-library-for-net/19260282"><img src="/Images/envato_circle.png" width="64"></a> | **[Purchase my software at Envato's CodeCanyon](https://codecanyon.net/item/elektrokit-class-library-for-net/19260282)**<br>If you are a .NET developer, you may want to explore **DevCase Class Library for .NET**, a huge set of APIs I have on sale. *It also contains all pieces of reusable code that you can find across the source code of my open-source works.* |

<br>
<div align="center">
  <b>Your support means the world to me! Thank you for considering it! 🤗💗</b>
</div>

------------------

## ⚠️ Disclaimer

This software and its associated repository are provided strictly on an "as is" basis, without warranties of any kind, whether express or implied. This includes, but is not limited to, any implied warranties of merchantability, reliability, or fitness for a particular purpose.

The authors and copyright holders assume no liability for any direct, indirect, incidental, or consequential damages—including data loss or system errors—arising from the use, misuse, or inability to use this software. You are solely responsible for determining the appropriateness of using this tool and assume all associated risks.

Furthermore, this project operates entirely independently. The utilization of any third-party libraries or components within this software does not imply any affiliation with, or endorsement or approval by, their respective original authors.

This software may interact with third-party services, websites, or platforms. It is the user's sole responsibility to ensure that such use complies with the applicable terms of service, laws, and regulations. The authors do not endorse, and are not responsible for, any misuse of this software to violate third-party terms of service or applicable law.

By using this software, you agree to indemnify and hold harmless the authors from any claims, damages, or liabilities arising from your use or misuse of it.

This project is licensed under the **Apache License, Version 2.0**. See the  [License](./LICENSE) file for details.
