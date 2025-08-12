.PHONY: help helm-lint helm-template helm-install helm-upgrade helm-test helm-uninstall k8s-apply k8s-delete k8s-status validate deploy-all clean-all

CHART_NAME ?= fabricx
NAMESPACE ?= fabricx-system
RELEASE_NAME ?= fabricx
VALUES_FILE ?= ./fabricx/values.yaml

help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-25s\033[0m %s\n", $$1, $$2}'

helm-lint: ## Lint Helm chart
	helm lint ./fabricx

helm-template: ## Render Helm templates
	helm template $(RELEASE_NAME) ./fabricx --values $(VALUES_FILE) --debug

helm-template-output: ## Render templates to file
	helm template $(RELEASE_NAME) ./fabricx --values $(VALUES_FILE) > rendered-templates.yaml

helm-dry-run: ## Dry run Helm install
	helm install $(RELEASE_NAME) ./fabricx --dry-run --debug --values $(VALUES_FILE)

helm-install: ## Install Helm chart
	helm install $(RELEASE_NAME) ./fabricx --namespace $(NAMESPACE) --create-namespace --values $(VALUES_FILE)

helm-install-wait: ## Install Helm chart and wait
	helm install $(RELEASE_NAME) ./fabricx --namespace $(NAMESPACE) --create-namespace --values $(VALUES_FILE) --wait --timeout=300s

helm-upgrade: ## Upgrade Helm release
	helm upgrade $(RELEASE_NAME) ./fabricx --namespace $(NAMESPACE) --values $(VALUES_FILE)

helm-upgrade-wait: ## Upgrade Helm release with wait
	helm upgrade $(RELEASE_NAME) ./fabricx --namespace $(NAMESPACE) --values $(VALUES_FILE) --wait --timeout=300s

helm-upgrade-atomic: ## Atomic Helm upgrade with rollback
	helm upgrade $(RELEASE_NAME) ./fabricx --namespace $(NAMESPACE) --values $(VALUES_FILE) --atomic

helm-rollback: ## Rollback to previous version
	helm rollback $(RELEASE_NAME) --namespace $(NAMESPACE)

helm-test: ## Run Helm tests
	helm test $(RELEASE_NAME) --namespace $(NAMESPACE)

helm-test-logs: ## Run Helm tests with logs
	helm test $(RELEASE_NAME) --namespace $(NAMESPACE) --logs

helm-uninstall: ## Uninstall Helm release
	helm uninstall $(RELEASE_NAME) --namespace $(NAMESPACE)

helm-status: ## Show Helm release status
	helm status $(RELEASE_NAME) --namespace $(NAMESPACE)

helm-history: ## Show Helm release history
	helm history $(RELEASE_NAME) --namespace $(NAMESPACE)

helm-values: ## Show values for release
	helm get values $(RELEASE_NAME) --namespace $(NAMESPACE)

k8s-apply: ## Apply Kubernetes manifests
	kubectl apply -f kubernetes/

k8s-delete: ## Delete Kubernetes resources
	kubectl delete -f kubernetes/

k8s-status: ## Show Kubernetes resources status
	kubectl get all -n $(NAMESPACE)

k8s-pods: ## Show pods
	kubectl get pods -n $(NAMESPACE)

k8s-services: ## Show services
	kubectl get services -n $(NAMESPACE)

k8s-deployments: ## Show deployments
	kubectl get deployments -n $(NAMESPACE)

k8s-logs: ## Show logs for all pods
	kubectl logs -l app.kubernetes.io/instance=$(RELEASE_NAME) -n $(NAMESPACE)

k8s-logs-auth: ## Show auth-service logs
	kubectl logs -l app.kubernetes.io/component=auth-service -n $(NAMESPACE)

k8s-logs-agent: ## Show agent-state logs
	kubectl logs -l app.kubernetes.io/component=agent-state -n $(NAMESPACE)

k8s-logs-interaction: ## Show interaction-service logs
	kubectl logs -l app.kubernetes.io/component=interaction-service -n $(NAMESPACE)

k8s-describe-pods: ## Describe all pods
	kubectl describe pods -n $(NAMESPACE)

k8s-port-forward-auth: ## Port forward to auth-service
	kubectl port-forward svc/$(RELEASE_NAME)-auth-service 8080:8080 -n $(NAMESPACE)

k8s-port-forward-agent: ## Port forward to agent-state
	kubectl port-forward svc/$(RELEASE_NAME)-agent-state 8081:8081 -n $(NAMESPACE)

k8s-port-forward-interaction: ## Port forward to interaction-service
	kubectl port-forward svc/$(RELEASE_NAME)-interaction-service 8082:8082 -n $(NAMESPACE)

k8s-exec-auth: ## Execute into auth-service pod
	kubectl exec -it $$(kubectl get pod -l app.kubernetes.io/component=auth-service -n $(NAMESPACE) -o jsonpath='{.items[0].metadata.name}') -n $(NAMESPACE) -- /bin/sh

validate-all: helm-lint helm-template ## Validate all templates
	kubectl apply --dry-run=client -f rendered-templates.yaml

validate-k8s: ## Validate Kubernetes manifests
	kubectl apply --dry-run=client -f kubernetes/

deploy-dev: ## Deploy to development
	helm upgrade --install $(RELEASE_NAME) ./fabricx \
		--namespace $(NAMESPACE) \
		--create-namespace \
		--values $(VALUES_FILE) \
		--set authService.replicaCount=1 \
		--set agentState.replicaCount=1 \
		--set interactionService.replicaCount=1 \
		--wait

deploy-prod: ## Deploy to production
	helm upgrade --install $(RELEASE_NAME) ./fabricx \
		--namespace $(NAMESPACE) \
		--create-namespace \
		--values $(VALUES_FILE) \
		--atomic \
		--wait \
		--timeout=600s

upgrade-auth: ## Upgrade auth-service only
	helm upgrade $(RELEASE_NAME) ./fabricx \
		--namespace $(NAMESPACE) \
		--values $(VALUES_FILE) \
		--set authService.image.tag=1.1 \
		--wait

upgrade-agent-state: ## Upgrade agent-state only
	helm upgrade $(RELEASE_NAME) ./fabricx \
		--namespace $(NAMESPACE) \
		--values $(VALUES_FILE) \
		--set agentState.image.tag=1.1 \
		--wait

upgrade-interaction: ## Upgrade interaction-service only
	helm upgrade $(RELEASE_NAME) ./fabricx \
		--namespace $(NAMESPACE) \
		--values $(VALUES_FILE) \
		--set interactionService.image.tag=1.1 \
		--wait

scale-up: ## Scale up all services
	kubectl scale deployment $(RELEASE_NAME)-auth-service --replicas=5 -n $(NAMESPACE)
	kubectl scale deployment $(RELEASE_NAME)-agent-state --replicas=3 -n $(NAMESPACE)
	kubectl scale deployment $(RELEASE_NAME)-interaction-service --replicas=3 -n $(NAMESPACE)

scale-down: ## Scale down all services
	kubectl scale deployment $(RELEASE_NAME)-auth-service --replicas=1 -n $(NAMESPACE)
	kubectl scale deployment $(RELEASE_NAME)-agent-state --replicas=1 -n $(NAMESPACE)
	kubectl scale deployment $(RELEASE_NAME)-interaction-service --replicas=1 -n $(NAMESPACE)

restart-all: ## Restart all deployments
	kubectl rollout restart deployment $(RELEASE_NAME)-auth-service -n $(NAMESPACE)
	kubectl rollout restart deployment $(RELEASE_NAME)-agent-state -n $(NAMESPACE)
	kubectl rollout restart deployment $(RELEASE_NAME)-interaction-service -n $(NAMESPACE)

rollout-status: ## Check rollout status
	kubectl rollout status deployment/$(RELEASE_NAME)-auth-service -n $(NAMESPACE)
	kubectl rollout status deployment/$(RELEASE_NAME)-agent-state -n $(NAMESPACE)
	kubectl rollout status deployment/$(RELEASE_NAME)-interaction-service -n $(NAMESPACE)

test-connectivity: ## Test service connectivity
	kubectl run test-pod --image=busybox --rm -it --restart=Never -n $(NAMESPACE) -- \
		sh -c "nslookup $(RELEASE_NAME)-auth-service && wget -qO- http://$(RELEASE_NAME)-auth-service:8080/health"

test-all-services: ## Test all service endpoints
	kubectl run test-pod --image=busybox --rm -it --restart=Never -n $(NAMESPACE) -- \
		sh -c "wget --spider http://$(RELEASE_NAME)-auth-service:8080/health && \
		       wget --spider http://$(RELEASE_NAME)-agent-state:8081/health && \
		       wget --spider http://$(RELEASE_NAME)-interaction-service:8082/health"

debug-auth: ## Debug auth-service issues
	kubectl describe deployment $(RELEASE_NAME)-auth-service -n $(NAMESPACE)
	kubectl logs -l app.kubernetes.io/component=auth-service -n $(NAMESPACE) --tail=50

debug-agent: ## Debug agent-state issues
	kubectl describe deployment $(RELEASE_NAME)-agent-state -n $(NAMESPACE)
	kubectl logs -l app.kubernetes.io/component=agent-state -n $(NAMESPACE) --tail=50

debug-interaction: ## Debug interaction-service issues
	kubectl describe deployment $(RELEASE_NAME)-interaction-service -n $(NAMESPACE)
	kubectl logs -l app.kubernetes.io/component=interaction-service -n $(NAMESPACE) --tail=50

clean-namespace: ## Delete entire namespace
	kubectl delete namespace $(NAMESPACE)

clean-all: helm-uninstall clean-namespace ## Clean everything

monitoring-install: ## Install monitoring stack
	helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
	helm repo update
	helm install prometheus prometheus-community/kube-prometheus-stack -n monitoring --create-namespace

create-ingress: ## Create ingress for services
	kubectl apply -f - <<EOF
	apiVersion: networking.k8s.io/v1
	kind: Ingress
	metadata:
	  name: $(RELEASE_NAME)-ingress
	  namespace: $(NAMESPACE)
	spec:
	  rules:
	  - host: fabricx.local
	    http:
	      paths:
	      - path: /auth
	        pathType: Prefix
	        backend:
	          service:
	            name: $(RELEASE_NAME)-auth-service
	            port:
	              number: 8080
	      - path: /agent-state
	        pathType: Prefix
	        backend:
	          service:
	            name: $(RELEASE_NAME)-agent-state
	            port:
	              number: 8081
	      - path: /interaction
	        pathType: Prefix
	        backend:
	          service:
	            name: $(RELEASE_NAME)-interaction-service
	            port:
	              number: 8082
	EOF

package: ## Package Helm chart
	helm package ./fabricx

dependency-update: ## Update Helm dependencies
	helm dependency update ./fabricx

install-with-monitoring: ## Install with monitoring enabled
	helm upgrade --install $(RELEASE_NAME) ./fabricx \
		--namespace $(NAMESPACE) \
		--create-namespace \
		--values $(VALUES_FILE) \
		--set monitoring.enabled=true \
		--set monitoring.serviceMonitor.enabled=true \
		--wait

canary-deploy: ## Canary deployment for interaction-service
	helm upgrade $(RELEASE_NAME) ./fabricx \
		--namespace $(NAMESPACE) \
		--values $(VALUES_FILE) \
		--set interactionService.image.tag=canary \
		--set interactionService.replicaCount=1 \
		--wait

full-deploy: validate-all helm-install helm-test ## Full deployment pipeline