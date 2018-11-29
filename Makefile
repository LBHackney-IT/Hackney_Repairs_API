
.PHONY: build
build:
	docker build -t hackney-repairs-api .

.PHONY: run-locally-compiled
run-locally-compiled:
	@echo -e "\033[31mThis does not do a NuGet restore.\e[0m "
	docker run --rm -i -t -v $(CURDIR)/HackneyRepairs/bin/Debug/netcoreapp2.1/HackneyRepairs.dll:/app/HackneyRepairs.dll hackney-repairs-api dotnet HackneyRepairs.dll
    
