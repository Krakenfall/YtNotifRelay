# YouTube Notification Relay

This project is designed to provide direct notifications for YouTube video uploads from a given channel to a specific Discord webhook. It uses Azure Functions to host an HTTP listener for subscribing, unsubscribing, and receiving push notifications from Google's YouTube PubSub service.

Currently, the application is unfinished and in development, but will have these features when complete:
* An HTTP Endpoint for subscribing to a provided YouTube channel's upload feed
* An HTTP Endpoint for receiving updates published to the upload feed
* Re-publishing of video notifications to a Discord webhook with nice embed format
* Filtering of Live Stream publish events (only relay notifications for live stream go-live)
* Full bicep for configuring and deploying to Azure Functions
* A YAML pipeline for building and deploying to Azure Functions

There's probably more.

# Getting Started
## Prerequisites
* VisualStudio 2022+
* Visual Studio Azure features installed

## Subscribing and Simulating the YouTube feed publisher
To start developing and testing the YtNotifRelay, 

1. Open the localsettings.json file and add these variables to the `Values` property:
```
"YT_TOPIC_URL": "https://www.youtube.com/feeds/videos.xml?channel_id=<CHANNEL_ID>",
"YT_HUB_URL": "https://pubsubhubbub.appspot.com/subscribe",
"DISCORD_NOTIFY_WEBHOOK_URL": "<DISCORD_CHANNEL_WEBHOOK>"
```
1. Start a debug session in Visual Studio
1. Once startup is complete, open a terminal session in the repository root
1. Use this command to POST to the subscribe
	```
	.\TestSubscribe.ps1 -IpOrHostname "<hostname>" -Port 9001
	```
1. Check the debug console session where functions are logged during execution for subscription success
1. Observe an upload to your targeted channel or
1. Use this command to simulate receving Notifications
	```
	.\TestNotif.ps1 -IpOrHostname "<hostname>" -Port 9001
	```
1. You should see a discord notification show up in your Discord server.

# Future Development

## Immediate Goals
* Handle Channel topic URLs in the POST to the subscribe and unsubscribe functions
* Get YouTube's publish to successfully POST to the ReceiveNotification function
* Add authorization and authentication 
* Add verbosity configurability in logging or use App Insights
* Bicep and YAML pipelines

## Stretch Goals
* Allow Discord webhooks to be provided on POST
* Store subscribed channels and webhooks securely for use and unsubscribing (Azure Tables?)
