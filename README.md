# BLiP CLI

BLiP command-line (CLI) tool is used for managing data of BLiP's bots. The blip-CLI is a cross-platform command line interface which helps making your development flow and repetitive tasks easier and quicker using some BLiP's APIs.

## Prerequisites

There is no prerequisites in order to use BLiP CLI.

## Installing

You can download below the zip file which contains both the .NET Core runtime and BLiP CLI binaries. This will get you a copy of BLiP CLI up and running on your local machine. The builds are always from the *master branch*.

| Platform | Latest Build |
| -------- | :----------------------------: |
| **Windows x64** | [zip](https://github.com/takenet/blip-CLI/releases/download/v0.4.0/win10-x64.zip) |
| **Windows x86** | [zip](https://github.com/takenet/blip-CLI/releases/download/v0.4.0/win10-x86.zip) |
| **Linux x64** | [Ubuntu zip](https://github.com/takenet/blip-CLI/releases/download/v0.4.0/ubuntu-x64.zip) |
| **macOS** | *coming soon* |

## Basic Usage

After downloading and unziping, you can open the command-line inside the *publish* folder and try the BLiP-CLI out using some of the [available commands](#main-features).

You must always type ***blip*** when running a command. The following one returns all BLiP-CLI features and you can check a particular one entering the feature name after *help*.

Returns all features:
```
blip help
```

Returns information about the *analyse* command:
```
blip help analyse
```

## Main features

|Command | Description |
| -------- | ---------------------------- |
| **[NLP Analyse](#nlp-analyse)** | Test your chatbot NLP model using a newline separated phrases as input, reporting results (intents and entitites) in a local file. **ATTENTION:** It causes costs, so check your AI provider beforehand.  |
| **[NLP Analyse with Content](#nlp-analyse-with-content)** | *Only for chatbot using Take.ContentProvider*. Test your chatbot NLP model using a newline separated phrases as input, reporting results (intents, entitites and **answers**) in a local file. <br/>**ATTENTION:** It causes costs, so check your AI provider beforehand. |
| **[Copy](#copy)**   | Copy data (like documents, bucket, AIModel) from a bot to another. |
| **[Export](#export)**   | Download the chatbot's data (like NLP model, tracks*, bucket*) to given directory. \**not implemented yet* |
| **[Export to Excel](#export-to-excel)** | Export chatbot data (like NLP model, tracks*, bucket*) to given directory in excel format. <br/>\**not implemented yet* |
| **[NLP Import](#nlp-import)** | Import a NLP model from local files to given chatbot in BLiP. |
| **[Format Key](#format-key)** | Returns authorization key (or access key) from an access key (authorization key)  |
| **[QR Code](##qr-code-only-for-chatbots-published-on-fb-messenger)** | Generates a payload-enabled (optional) QR Code for a chatbot published on Facebook Messenger. |
| **[Save Node](#save-node)** | Save nodes informations to reuse on next requests. (Saved only locally) |
| **[Ping](#ping)** | Ping some node and show elapsed time. |

<br/>

### NLP Analyse
Test your chatbot NLP model using a newline separated phrases as input, reporting results (intents and entitites) in a local file
The NLP Analyse command main goal is help blip developers to test their AI models quickly.  It has 3 differents ways of using it:  
**Analyse single phrase**: `analyse -a {bot_key} -i "Test phrase!" -o D:\Path\To\Output\File.txt`  
  
**Analyse file with phrases**: `analyse -a {bot_key} -i D:\Path\To\Input\File.txt -o D:\Path\To\Output\File.txt`  
  
**Analyse using another chatbot model**: `analyse -a {bot_key} -i BotKey:{another_bot_ket} -o D:\Path\To\Output\File.txt`  

There are 3 observations: 
-The *{bot_key}* must be without the word 'Key';
- If your path has any whitespace, it must be inside of quotation marks;
- It also has the `--raw` parameter which writes, on the output report file, the raw log of NLP Analysis Response for each analyzed input.

### NLP Analyse with Content
The content check is an option for chatbots using *Take.ContentProvider API*. It gives not only the intents and entities returned by the AI provider but also the answer previously filled into the ContentProvider, matching the combination intent -> entities to give the right one. The *{bot_key}* must be without the word 'Key'.

```
analyse -a {bot_key} -i D:\Path\To\Input\File.txt -o D:\Path\To\Output\File.txt --check
```

### Copy
This is a handy feature for copying important information from one chatbot to another. It can be used with documents, bucket and AIModel. **Note:** The parameter `--force` is used for deleting intents, entities and answers from the target chatbot before copying. If not set, it'll be copied only the new things. The *{bot_key}* must be without the word 'Key'.

```
copy -c document -f papagaio --fromAuthorization {bot_key} -t tucano --toAuthorization {bot_key}
```
### Export
The Export command lets you export data from a specific chatbot. As of right now, only the NLP Model can be exported where the intents, entities and answers are given in 3 CSV files. The *{bot_key}* must be without the word 'Key'.

```
export -m NLPModel -a {bot_key} -o D:\Output\Path
```

### Export to Excel
The Export can also gather all information in an excel file with 4 worksheets: Intents, Questions, Answers, Entities. <br/>The *{bot_key}* must be without the word 'Key'.

```
export -m NLPModel -a {bot_key} -o D:\Output\Path -x FileName (w/out extension)
```

### NLP Import
The NLP Import can be used to import intents, entities and answers into a specific chatbot. The files must be CSV files. **Note:** These answers are the ones inside of each intent, there are different from the answers in the *ContentProvider API*. <br/>The *{bot_key}* must be without the word 'Key'.

```
nlp-import -a {bot_key} --ip D:\Path\To\Input\File.csv --ep D:\Path\To\Input\File.csv --ap D:\Path\To\Input\File.csv
```

### Format Key
This command returns the chatbot Access Key which is required when connecting to any BLiP internal API or using of the SDKs. [More information here.](https://docs.blip.ai/#sending-commands "BLip Documentation") The *{bot_key}* must be without the word 'Key'.

```
formatKey -a {bot_key} -i chatbottest
```

### QR Code (only for chatbots published on FB Messenger)
The FB Messenger has a feature called QR Code which lets the user scan this code and be redirected to the chatbot. It also has the payload option, so that the developer can transfer the user to a specific point when the QR code is scanned. **Note:** if set, the parameter `-d` downloads a copy of the QR Code. The *{bot_key}* must be without the word 'Key'.

```
qrcode -a {bot_key} -n papagaio
```

### Save Node
This feature saves the chatbot connection information relating it to a node. Next time you run another command, you won't need to pass the connection key, only the chatbot note. **Note:** it cannot be used in any AI-related command. In case of having any problem running an AI command, delete the file publish > *settings.blip*. The *{bot_key}* must be without the word 'Key'.

```
saveNode -a {bot_key} -n papagaio@msging.net 
```

### Ping
Ping a node(chatbot) and show elapesd time. Simple as that!

```
ping -n papagaio
```

## Building from source

In order to run BLiP CLI code you must have .NET Core 2.0 (or greater) installed on machine. To download the .NET Core runtime **without** the SDK, visit https://github.com/dotnet/core-setup#daily-builds. It's recommended to use Visual Studio 2017 (version 15.5) or greater.

### Generating BLiP Executable from source

The BLiP executables may be the `release` or `debug` version for any of the following OS versions (except macOS):

| System | Runtime |
| -------- | :----------------------------: |
| **Windows 10 x64** | `win10-x64` |
| **Windows 10 x86** | `win10-x86` |
| **Windows 8.1 x64** | `win81-x64` |
| **Windows 8.1 x86** | `win81-x86` |
| **Ubuntu x64** | `ubuntu-x64` |
| **macOS** | *coming soon* |

It can be built using some of the following tools:

### **`dotnet`**

Base command:
```sh
dotnet publish src/Take.Blip.CLI/Take.Blip.CLI/Take.Blip.CLI.csproj --framework netcoreapp2.0 --runtime %runtime% --configuration %config%
```

Example:
```sh
dotnet publish src/Take.Blip.CLI/Take.Blip.CLI/Take.Blip.CLI.csproj --framework netcoreapp2.0 --runtime win10-x64 --configuration release
```

## Questions & Comments

For any feedback, please use the Issues on this repository.

## How to contribute

Please read [CONTRIBUTING.md](https://github.com/takenet/blip-CLI/blob/master/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Contributors

See also the list of [contributors](https://github.com/takenet/blip-CLI/graphs/contributors) who participated in this project.

## License

[Apache 2.0 License](https://github.com/takenet/blip-sdk-csharp/blob/master/LICENSE)
