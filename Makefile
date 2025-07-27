.PHONY: start stop tests results clean help

help:
	@echo "Available targets:"
	@echo "  start               - Start both payment processor and main application containers"
	@echo "                        Use 'make start STATS=true' to show docker stats after startup"
	@echo "  stop                - Stop both main application and payment processor containers"
	@echo "  tests               - Run k6 load tests against the application"
	@echo "  results             - View partial test results using bat"
	@echo "  clean               - Stop and clean up all containers, volumes, and networks"
	@echo "  help                - Show this help message"

start:
	@echo "\nStarting payment processor containers...\n"
	docker-compose -f payment-processor/docker-compose-arm64.yml up -d --build
	@echo "\nPayment processors started. Starting main application containers...\n"
	docker-compose up -d --build
	@if [ "$(STATS)" = "true" ]; then \
		echo "\nStarting docker stats...\n"; \
		docker stats; \
	fi

stop:
	@echo "\nStopping main application containers...\n"
	docker-compose down
	@echo "\nMain application stopped. Stopping payment processor containers...\n"
	docker-compose -f payment-processor/docker-compose-arm64.yml down

tests:
	@echo "\nRunning k6 load tests...\n"
	sudo docker run --rm -i \
		--network host \
		-e K6_WEB_DASHBOARD=true \
		-e K6_WEB_DASHBOARD_EXPORT=/results/report.html \
		-v $$(pwd)/rinha-test:/scripts \
		-v $$(pwd)/rinha-test/results:/results \
		grafana/k6 run \
		/scripts/rinha.js

results:
	@echo "\nViewing partial test results...\n"
	bat rinha-test/results/partial-results.json

clean:
	@echo "\nRemoving unused containers, networks, and volumes...\n"
	-docker system prune -a --volumes
	@echo "\nCleanup complete!"