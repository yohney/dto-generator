# dto-generator
A visual studio plugin for generating DTO objects from entities

# Getting started

* Right click any entity (cs class) and choose option "Generate DTO"

* Pick a location - if any existing DTOs are found, most likely location will be selected. Otherwise, select location in format PROJECTNAME/Folder/SubFolder. For example, Grader.DAL/Generated/Dto or Grader.DAL/Dto

* Pick properties from entities that you want to map

* Add any custom code between ////BCC/ BEGIN CUSTOM CODE SECTION and ////ECC/ END CUSTOM CODE SECTION. Any custom code will be preserved upon regeneration.

* [See release notes for latest version](src/DtoGenerator/DtoGenerator.Vsix/release-notes.html)