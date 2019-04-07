# SpaceTools
SpaceTools is a library of tools for scraping MySpace data. 

Data is saved as JSON files, with direct file downloads for some settings.

Each operation includes full logging and will produce .log files.

Some operations process multiple profiles. Most of these processes are restartable and use .complete and .lock files to track their progress.

Files that cannot be found due to the incomplete server migration have .error files saved as placeholders. 
Filenames with slashes replace the slash with three underscores like this: ___

The library makes use of the HTMLAgility and Newtonsoft.JSON NuGet packages for processing.

# Tools
Tools included in the library are as follows.

1. **Profile Downloader:** Downloads a single profile to JSON and media files.

2. **Profiles Downloader (list):** Downloads profiles according to a list of profile names.
3. **Location Aggregator:** Traverses a directory of downloaded profile data and aggregates location information, including location of connected profiles.
4. **Crawler:** Crawls a profile graph starting at a base profile.

# hashkey
To get the hashkey for use with the library, check a request header from a manual visit to MySpace.

Here are the steps to do this in Firefox.

1. Open profile page.

2. Inspect root element.
3. Go to network tab.
4. Find POST call.
5. Inspect header on call.
6. Click Edit and Resend.
7. Copy value of <b>Hash</b> parameter.
8. The key should be roughly 250 characters long or longer.
