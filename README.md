# README #

Static image disk storage based on ASP .NET MVC4

### What is this repository for? ###

* Upload images to disk storage
* Make cropping using HttpHandler (to improve perfomance on cropping). Usefull for galleries and previews.
* Cache cropping files on disk
* SUpports only .png, .jpg, .jpeg extensions

### How do I get set up? ###

* Setup directory to store images and web prefix in web.config
* Start app, 
* On /home/index you can upload image (using PLUpload, but you can implement another uploader) - for example "/data/myimage.png"
* Storage will set unique name for file and save it inside storage folder
* Access to file using your web prefix "/data/myimage.png"
* Access to cropped file using "/data/myimage.png?w=100&h=150" - you will got cropped center of file of 100x150 dimensions. Also this file will be saved to disk cache, so next time you request it, file will be returned from cache 