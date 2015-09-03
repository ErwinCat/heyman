# Heyman

Simple app for create XMPP chat bot helper

> Sorry for my english

### How it's work:


1. You specify XMPP account, create list of commands and run **Heyman.exe** application on host.
2. **Heyman.exe** connect to XMPP server and wait input messages from any user.
3. **Heyman.exe** application has received the message and trying to find command.
4. If command not found, application send answer with help message about all existing commands
5. If command found, **Heyman.exe** run command, redirect all messages to **stdin** and send all data from **stdout** to user until application closed


## Quick start

 1. Compile project
 2. Create `config.yaml` at root file
 3. run ` /usr/bin/mono Heyman.exe config.yaml` or `Heyman.exe config.yaml`

## Configuration

Example configuration to run at Mono:
```yaml

# XMPP client configuration
Xmpp:
  User: xmpp_username
  Password: xmpp_password
  Server: xmpp.server.host
# List of commands  
Commands:
- Title: "weather"
  Regex: "^(weather|weath)"
  Description: "tell weather int your city"
  Arguments: "/srv/heyman/hey/Hey.exe weather -message='$MESSAGE' -u='$USER_ID' -n='$USER_NAME'"
  FileName: "/usr/bin/mono"
  EndLine: "|"

- Title: "restart"
  Regex: "restart"
  Description: "restart Ubuntu server"
  Arguments: "-r now"
  FileName: "/usr/bin/shutdown "
  UserName: root
  WorkingDirectory: ./

# Localization strings  
Localization:
  HelpHeader: "Hi there! List of my commands:"
  
```

Command fields description:
- **Title** - display name for user
- **Regex** - regex expression for matching command
- **Description** - description for user
- **FileName** - app file path to execute WITHOUT arguments
- **UserName** - user name 
- **EndLine** - specifies end of line for message. See [EndLine-symbol]
- **WorkingDirectory** - specifies working directory 
- **Arguments** - arguments for app. You can use '$<ARG_NAME>' macros to insert some data to arguments
 
### EndLine symbol

1. **Heyman** application wait for '/n' from command **stdout** and send message user. If **EndLine** not specified, command can send only one-line message. But if you specify **EndLine**, **Heyman** split string by this symbol. 

2. **Heyman** listen new messages from users and write it to **stdin**. User message can contain multiple lines with '\n' symbols. This symbol may be replaced by special string, specified in command config.

### Arguments macros:

- **$MESSAGE** - input message from client
- **$USER_ID** - user XMPP user id (JID). For example: "heyman@jabber.com/Other"
- **$USER_NAME** - user name. For example: "Чат бот"

### Run with Supervisor

Exmaple supervisor config file `./scripts/supervisor.conf`

When start app by supervisor, **stdin** encoding was different, from console start encoding. Fixed by adding line
```
environment=LC_ALL='en_US.UTF-8',LANG='en_US.UTF-8' 
```
to supervisor config file.

## Folder structure

```
asv.heyman
├── scripts/                   # scripts folder
│   ├── supervisor.conf/       # default config for supervisor
│   └── restart.sh/            # script for restart service
├── src/                       # source code folder
├── LICENCE                    # code licence
└── README.md                  # project description
```

## Versioning

Project is maintained under [the Semantic Versioning guidelines](http://semver.org/).

## Changes
All notable changes to this project will be documented here

### [1.0.0] - 2015/09/03

#### Added
 - add README
 - add Heyman XMPP bot

## Copyright and license

Code released under [the MIT license](<source to mit licence>). Docs is licensed under a [Creative Commons — CC BY 3.0](http://creativecommons.org/licenses/by/3.0/).