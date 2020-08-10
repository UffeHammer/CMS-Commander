# CMS-Commander
A tool for converting content between different CMS systems and different versions of selected CMS systems. 

This application is intended for converting CMS data from one CMS system to another, it follows the notion that all CMS systems can be broken into these simple parts:

 - Items and fields, an item is made up from a number of fields, where each field can be of different types, but basically all fields will be converted to strings internally.
 - Every item is based on one or more templates, which also happens to be an item.
 - Every item has a parent (except root item) and zero or more children.

With these two simple types which exists as Interfaces in this project, it should be possible to implement a CMS specific versions that also exposes reader or writer functionality. Have a look at IItem.cs and IField.cs files for further clarification.

So far only Sitecore 4, 5, 6, 7, 8 and 9 are supported and only 6-9 supports writing. But hopefully people can participate and add new CMS systems to the list.

Also do not rely on Users and Roles, they are too sitecore specific to be used anywhere else.

For sitecore 6-9 install the "Sitecore webservice installer.zip" sitecore package on the website. That file gets automatically copied to the users "My documents\CMS Commander" folder, when the setup file is installed on the users computer.


Uffe Hammer
