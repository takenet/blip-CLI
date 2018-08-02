# BLiP CLI

BLiP command-line (CLI) tool, used for managing data of BLiP's bots. The blip-CLI is a cross-platform command line interface helps to make easy and quickly your development flow and the execution of some repetitive tasks BLiP's API.

---

Basic usage
-----------

After download and unzip blip-cli, you can try it out using some of the [available commands](#main-features).

For instance, you can `ping` a bot using using:

`blip ping -n papagaio@msging.net`

where `-n` parameter represents the bot node

--video

Installers and Binaries
-----------------------

You can download the BLiP CLI as a zip file. The zip file contains both the .NET Core runtime and BLiP CLI binaries.

| Platform | Latest Build <br>*master*<br> |
| -------- | :----------------------------: |
| **Windows x64** | [zip](https://github.com/takenet/blip-CLI/releases/tag/0.0.1) |
| **Windows x86** | [zip](https://github.com/takenet/blip-CLI/releases/tag/0.0.1) |
| **Linux x64** | [Ubuntu zip](https://github.com/takenet/blip-CLI/releases/tag/0.0.1) |
| **macOS** | *comming soon* |


Main features
-------------
| Command | Description | Sample |
| -------- | -------- | :---------------------------- |
| **ping** | Ping some node and show elapsed time | `blip ping -n papagaio@msging.net` |
| **formatKey** | Returns authorization key (or access key) from an access key (authorization key)  | `blip formatKey -i testehttppost -a some-authorization-key` |
| **saveNode** | Save nodes informations to reuse on next requests. (Saved only locally) | `blip saveNode -n papagaio@msging.net -a some-authorization-key` |
| **copy**   | Copy data (like documents, bucket, IA) from a bot to another | `blip copy -f papagaio --fromAuthorization some-authorization-key -t papagaio --toAuthorization some-authorization-key -c document` |
| **export**   | Download the chatbot's data (like NLP model, tracks*, bucket*) to given directory. * not implemented yet| `blip export -a some-authorization-key -m nlpModel -o directory/to/download/` |

Building from source
--------------------

In order to run BLiP CLI code you must have .NET Core 2.0 (or greater) installed on machine. To download the .NET Core runtime **without** the SDK, visit https://github.com/dotnet/core-setup#daily-builds.

Is recommended use Visual Studio 2017 (version 15.5) or greater.

Questions & Comments
--------------------

For all feedback, use the Issues on this repository.

License
-------
[Apache 2.0 License](https://github.com/takenet/blip-sdk-csharp/blob/master/LICENSE)
