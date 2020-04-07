[![version](https://img.shields.io/badge/version-v1.3-blue.svg)](https://github.com/jonHuffman/UnifiedFontSize)
[![license](https://img.shields.io/badge/license-MIT-red.svg)](https://github.com/jonHuffman/ViewManager/blob/master/LICENSE)
[![version](https://img.shields.io/badge/package-download-brightgreen.svg)](https://github.com/jonHuffman/ViewManager/raw/master/UnityPackage/ViewManager_v1.3.unitypackage)  

# ViewManager
A manager for the UI lifecycle within Unity applications, ViewManager makes it easy to add and remove UI prefabs while maintaining strict separation between UI and system code.

Originally written and maintained for private use I have decided to make it public in hopes that other game developers will find it useful.

## What it Does

## How to Use
Though ultimately simple to use, the View Manager requires some setup before you can start invoking your UI. Typically the majority of the work is in the creation of your UI prefabs, these are your Views. Once your View exists you must link it in an library asset called a Registrar. Finally, you must call initialize on the View Manager system and provide a couple of dependancies (including a registrar). Once this is done iterating and adding new views becomes a quick process. 
### Creating a View
### Using the Registrar
### Initializing the ViewManager
### Adding a View
`ViewManager.Instance.AddView(View.MainMenu);`
