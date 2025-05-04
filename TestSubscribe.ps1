param(
	[string]$IpOrHostname,
	[int]$Port = 80
)
Invoke-WebRequest -Uri "http://$IpOrHostname`:$Port/api/SubscribeToYouTube" -Method POST