
param(
	[string]$IpOrHostname,
	[int]$Port = 80
)
$payload = @"
<?xml version="1.0" encoding="UTF-8"?>
<feed xmlns:yt="http://www.youtube.com/xml/schemas/2015" xmlns:media="http://search.yahoo.com/mrss/" xmlns="http://www.w3.org/2005/Atom">
 <link rel="self" href="http://www.youtube.com/feeds/videos.xml?channel_id=CHANNEL_ID"/>
 <id>yt:channel:CHANNEL_ID</id>
 <yt:channelId>CHANNEL_ID</yt:channelId>
 <title>Clint</title>
 <link rel="alternate" href="https://www.youtube.com/channel/CHANNEL_ID"/>
 <author>
  <name>Clint</name>
  <uri>https://www.youtube.com/channel/CHANNEL_ID</uri>
 </author>
 <published>2013-11-12T01:40:26+00:00</published>
 <entry>
  <id>yt:video:ZwtvgLH4nHk</id>
  <yt:videoId>ZwtvgLH4nHk</yt:videoId>
  <yt:channelId>CHANNEL_ID</yt:channelId>
  <title>cat slep</title>
  <link rel="alternate" href="https://www.youtube.com/watch?v=ZwtvgLH4nHk"/>
  <author>
   <name>Clint</name>
   <uri>https://www.youtube.com/channel/CHANNEL_ID</uri>
  </author>
  <published>2025-05-04T06:46:39+00:00</published>
  <updated>2025-05-04T06:48:23+00:00</updated>
  <media:group>
   <media:title>cat slep</media:title>
   <media:content url="https://www.youtube.com/v/ZwtvgLH4nHk?version=3" type="application/x-shockwave-flash" width="640" height="390"/>
   <media:thumbnail url="https://i3.ytimg.com/vi/ZwtvgLH4nHk/hqdefault.jpg" width="480" height="360"/>
   <media:description></media:description>
   <media:community>
    <media:starRating count="0" average="0.00" min="1" max="5"/>
    <media:statistics views="1"/>
   </media:community>
  </media:group>
 </entry>
</feed>
"@
Invoke-WebRequest -Uri "http://$IpOrHostname`:$Port/api/ReceiveNotification" -Method POST -Headers @{ "Content-Type" = "application/xml" } -Body $payload