# EpicorExport
*Export all the things*

## Description
This is a collection of Epicor libraries used to export all the things you want to extract from your Epicor tenant.

## How?
- Open the full native Epicor client.
- Navigate to `Epicor Functions Maintenance`
    - System Management/Business Process Management/Epicor Functions Maintenance
- Under `Actions`
    - Select `Import Library`
    - Import the `*.efxj` files containing your code
- ???
- Profit

## Contributing
Make a directory for each Epicor library with the following structure:

|   Name    |   Use |
|-----------|-------|
|   lib     |   Export the library in `.efxj` format and then use a tool to *"pretty print"* the contents before commiting so the diff is human readable. (VSCode JSON tools can do this)   |
|   src     |   Copy out the source of each function in `.cs` format so it is easily readable and editable with C# IDE of choice |

## Credits
Special thanks to KLINCECUM at the epiusers forum who wrote the `BAQExport.efxj` code that started this effort:
https://www.epiusers.help/t/exporting-baq-data-from-a-function-file-or-no-file-example-library/108641

How it started: https://www.epiusers.help/t/something-im-thinking-of-working-on/103807/33

This repository was created in hopes to foster collaboration on these functions in a version controlled fashion. 
The original code was posted as a file on the forum.
