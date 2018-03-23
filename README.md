# Ballerup_Aviskiosk
Copyright 2018 Yoke Aps. GPLv3 licensed.

## Description
Magazine collection browser to be used in conjuction with an RBDigital subscription.

## Dependencies
The source code depends on three tools from Unity Asset Store:

- DOTween (free)
- TextMesh Pro (free)
- Embedded Browser by Zen Fulcrum (costs approx. 90$)

The user needs to install these three tools prior to checking out the source in this repository. DOTween needs to be setup via a menu-item inside unity, but the menu item will only appear after successful compilation. If you have already put this source-code in your unity project, it will prevent you from compiling and thus prevent you from setting up DOTween (which is what prevents you from compiling). So you'll end up in this catch 22, unless you first install the asset store tool and afterwards copy/checkout the source found in this repository.

The app also needs to be viewed on a vertically oriented HD-screen (1080px x 1920px) for the layouting to work correctly;

## Login Credentials
For the app to be able to login and show RBDigital Magazines you need supply a json-file with login credentials. It must be placed in the datapath and be named "login.json". The format must be as follows:
` {
	"username":"some_user",
	"password":"some_password"
} `
 

## Use
The app looks in the datapath for a folder called "MagazineCovers". A Magazinecover is an imagefile with a name following this convention:
` [MAGAZINE_NAME]_[MAGAZINE_ID]. `

- The magazine name can be anything, and is showed when you assemble a magazine correctly.
- The Magazine id needs to be the id of the magazine found in the url (magId="????") of the magezine-page in the RBDigital webpage.

The scene "kiosk2" is the one containing the final app.

### Source Code notes
The source code is not very documented, but it is quite simple. Here are a couple of notes on the structure.

- **PuzzleController** is the script with main control over the app. It is responsible for all state changes, such as going to and from the magazine-reader, showing the match-overlay, scrambling the frontpage etc.
- **ImageLoader** controls loading and parsing of images and filenames.


- **ImageLine** Represents a single horizontal line showing either top, middle or bottom segments of the cover-images. As dragging swiping happens on a single line at a time, ImageLines are responsible for the drag/swipe interaction.

- **BrowserWrapper** Is a wrapper around the ZenFulcrum Embedded Browser. It is also responsible for logging on to the RBDigital-database and navigating to the RBDigital magazine-viewer. The browser wrapper loads the magazine reader by navigating to the RBDigital webpage in the background, logging in and navigating to the magazine-viewer. This is done by looking for ids and other selectors in the RBDigital webpage, so if the structure of the RBDigital webpage changes enough, or if the navigation flow changes, the browserwrapper will have to change accordingly to be able to navigate to the magazine-viewer.