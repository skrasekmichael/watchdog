param(
	[switch]$NoBuild
)

if (-not $NoBuild) {
	dotnet publish ./Watchdog.Api/ --os linux --arch x64 -c Release /p:PublishProfile=DefaultContainer
}

$containerName = "watchdog-api"

if (docker ps -a --format "{{.Names}}" | Select-String -Pattern "^$containerName$" -Quiet) {
	docker stop $containerName
	docker rm $containerName
}

docker run -d --name $containerName -p 8080:8080 -p 8082:8082 watchdog-api:latest
