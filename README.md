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

# WallPin

### A lightweight command-line utility to change the Windows desktop wallpaper with support for all layout styles (Fill, Fit, Center, etc.) and context menu integration via optional shell scripts.

------------------

## 👋 Introduction

**WallPin** lets you set any supported image as your Windows desktop wallpaper. It doesn't run in the background, it doesn't consume resources, and it works completely silently without even showing a console window. If an operation fails or the arguments are invalid, the tool will display a standard Windows message box showing the exact error details.

## 💡 Why WallPin?

I decided to develop this application because Windows 10 has a deeply annoying design flaw: when you use the built-in "Set as desktop background" menu command, Windows forces a global system refresh that resets the scrollbar position in all open File Explorer windows back to the very top.

If you are working inside a directory with thousands of files and you have an image selected near the middle or the bottom, Windows completely breaks your user experience by throwing you back to the top of the list. On top of that, the built-in menu command doesn't even let you choose the wallpaper style (Fill, Fit, Center, etc.).

**WallPin** solves both issues. It updates your desktop wallpaper instantly without affecting your open File Explorer windows, leaving your selection and scrollbars exactly where they are, and gives you full control over the wallpaper layout style.

## 👌 Features

- Supports `center`, `tile`, `stretch`, `fit`, `fill`, and `span` using either their names or standard Windows IDs.
- Includes an optional batch-script file to add a clean "Set as desktop background" sub-menu directly into the Windows Explorer right-click menu for supported image files.

> [!NOTE]
> Adding these registry keys does not delete or corrupt the original built-in Windows menu commands; it simply overrides them safely. A separate batch-script file is also included to easily remove **WallPin**'s registry keys and restore built-in Windows menu commands back to its original state. 

> [!IMPORTANT]
> However, this software is provided "as-is", without any warranties. I take no responsibility for any registry issues caused by the use or misuse of this tool. Use it at your own risk.

## 🖼️ Screenshots

![screenshot](/Images/screenshot1.png)

![screenshot](/Images/screenshot2.png)

## 📝 Requirements

- Microsoft Windows OS with .NET Framework 4.8.

## 🤖 Getting Started

Download the latest release by clicking [here](https://github.com/ElektroStudios/WallPin-Set-Desktop-Wallpaper/releases/latest) and start using it.

## 🔄 Change Log

Explore the complete list of changes, bug fixes, and improvements across different releases by clicking [here](/Docs/CHANGELOG.md).

## 💪 Contributing

Your contribution is highly appreciated!. If you have any ideas, suggestions, or encounter issues, feel free to open an issue by clicking [here](https://github.com/ElektroStudios/WallPin-Set-Desktop-Wallpaper/issues/new/choose). 

Your input helps make this Work better for everyone. Thank you for your support! 🚀

## 💰 Beyond Contribution 

This work is distributed for educational purposes and without any profit motive. However, if you find value in my efforts and wish to support and motivate my ongoing work, you may consider contributing financially through the following options:

<br></br>
<p align="center"><img src="/Images/github_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Becoming my sponsor on Github:</h3>
<p align="center">You can show me your support by clicking <a href="https://github.com/sponsors/ElektroStudios/">here</a>, <br align="center">contributing any amount you prefer, and unlocking rewards!</br></p>
<br></br>

<p align="center"><img src="/Images/paypal_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Making a Paypal Donation:</h3>
<p align="center">You can donate to me any amount you like via Paypal by clicking <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=E4RQEV6YF5NZY">here</a>.</p>
<br></br>

<p align="center"><img src="/Images/envato_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Purchasing software of mine at Envato's Codecanyon marketplace:</h3>
<p align="center">If you are a .NET developer, you may want to explore '<b>DevCase Class Library for .NET</b>', <br align="center">a huge set of APIs that I have on sale. Check out the product by clicking <a href="https://codecanyon.net/item/elektrokit-class-library-for-net/19260282">here</a></br><br align="center"><i>It also contains all piece of reusable code that you can find across the source code of my open source works.</i></p>
<br></br>

<h2 align="center"><u>Your support means the world to me! Thank you for considering it!</u> 👍</h2>
