up:
	docker compose up -d --build $(s)

down:
	docker compose down
	
fresh:
	docker compose down -v
	docker compose up -d --build

logs:
	docker compose logs -f $(s)
