$containerName = "influxdb"

if (docker ps -a --format "{{.Names}}" | Select-String -Pattern "^$containerName$" -Quiet) {
    docker stop $containerName
    docker rm $containerName
}

docker run -d --name $containerName -p 8086:8086 `
	-e DOCKER_INFLUXDB_INIT_MODE=setup `
	-e DOCKER_INFLUXDB_INIT_USERNAME=user `
	-e DOCKER_INFLUXDB_INIT_PASSWORD=password `
	-e DOCKER_INFLUXDB_INIT_ORG=organization `
	-e DOCKER_INFLUXDB_INIT_BUCKET=watchdog `
	-e DOCKER_INFLUXDB_INIT_ADMIN_TOKEN=access_token `
	influxdb:2.7.6
