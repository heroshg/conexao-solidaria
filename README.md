# Conexão Solidária

MVP de plataforma digital para a ONG fictícia "Esperança Solidária" — gestão de campanhas de arrecadação e doadores. Diagrama de arquitetura e justificativa de banco de dados estão na seção [Documentação](#documentação) abaixo.

3 serviços: `Identity.Api`, `Campaigns.Api` (Web APIs, Controllers) e `Donations.Worker` (consumer RabbitMQ, sem API de negócio).

## Documentação

- [`docs/arquitetura.png`](./docs/arquitetura.png) — diagrama de arquitetura (microsserviços, bancos, broker, observabilidade, Kong). Fonte editável em [`docs/arquitetura.html`](./docs/arquitetura.html) (SVG inline, sem dependências externas).
- [`docs/justificativa-banco.pdf`](./docs/justificativa-banco.pdf) — por que PostgreSQL e por que só 2 bancos físicos (não 1 por serviço). Fonte em [`docs/justificativa-banco.html`](./docs/justificativa-banco.html).

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) rodando
- [`kind`](https://kind.sigs.k8s.io/) e `kubectl` no PATH
- OpenSSL (já vem no Git Bash / WSL / maioria das distros Linux)

## Subir tudo no Kubernetes (kind)

Rodar a partir da raiz do repositório.

### 1. Gerar o par de chaves RSA do JWT (não é commitado — ver `.gitignore`)

`Identity.Api` assina o token, `Campaigns.Api` só recebe a chave pública para validar.

```bash
mkdir -p Identity.Api/Identity.Api/keys Campaigns.Api/Campaigns.Api/keys
openssl genrsa -out Identity.Api/Identity.Api/keys/jwt-private.pem 2048
openssl rsa -in Identity.Api/Identity.Api/keys/jwt-private.pem -pubout -out Campaigns.Api/Campaigns.Api/keys/jwt-public.pem
```

### 2. Gerar as migrations do EF Core (só na primeira vez / após mudar entidades)

```bash
dotnet tool update --global dotnet-ef

dotnet ef migrations add InitialCreate \
  --project Identity.Api/Identity.Infrastructure/Identity.Infrastructure.csproj \
  --startup-project Identity.Api/Identity.Infrastructure/Identity.Infrastructure.csproj \
  --output-dir Persistence/Migrations

dotnet ef migrations add InitialCreate \
  --project Campaigns.Api/Campaigns.Infrastructure/Campaigns.Infrastructure.csproj \
  --startup-project Campaigns.Api/Campaigns.Infrastructure/Campaigns.Infrastructure.csproj \
  --output-dir Persistence/Migrations

dotnet ef migrations add InitialCreate \
  --project Donations.Worker/Donations.Infrastructure/Donations.Infrastructure.csproj \
  --startup-project Donations.Worker/Donations.Infrastructure/Donations.Infrastructure.csproj \
  --output-dir Persistence/Migrations
```

Cada serviço aplica sua própria migration automaticamente no startup — não precisa rodar `database update` manualmente.

### 3. Build das 3 imagens Docker

```bash
docker build -f Identity.Api/Dockerfile -t conexao-solidaria/identity-api:latest Identity.Api
docker build -f Campaigns.Api/Dockerfile -t conexao-solidaria/campaigns-api:latest Campaigns.Api
docker build -f Donations.Worker/Dockerfile -t conexao-solidaria/donations-worker:latest Donations.Worker
```

### 4. Criar o cluster kind (3 nodes: 1 control-plane + 2 workers)

```bash
kind create cluster --config deploy/kind-cluster.yaml
kubectl get nodes   # os 3 devem aparecer Ready
```

### 5. Carregar as imagens no cluster

O kind não compartilha o cache de imagens do Docker Desktop — precisa carregar explicitamente.

```bash
kind load docker-image conexao-solidaria/identity-api:latest --name conexao-solidaria
kind load docker-image conexao-solidaria/campaigns-api:latest --name conexao-solidaria
kind load docker-image conexao-solidaria/donations-worker:latest --name conexao-solidaria
```

### 6. Subir a infraestrutura (namespace, Postgres, RabbitMQ, Prometheus, Grafana, NetworkPolicies)

```bash
kubectl apply -f deploy/namespace.yaml
kubectl apply -f deploy/postgres/
kubectl apply -f deploy/rabbitmq/
kubectl apply -f deploy/prometheus/
kubectl apply -f deploy/grafana/
kubectl apply -f deploy/network-policies/

kubectl wait --for=condition=Ready pod -l app=postgres -n conexao-solidaria --timeout=90s
kubectl wait --for=condition=Ready pod -l app=rabbitmq -n conexao-solidaria --timeout=90s
kubectl wait --for=condition=Ready pod -l app=prometheus -n conexao-solidaria --timeout=90s
kubectl wait --for=condition=Ready pod -l app=grafana -n conexao-solidaria --timeout=90s
```

> `deploy/network-policies/` aplica um default-deny de ingress no namespace + regras explícitas liberando só o tráfego pod-a-pod esperado (ex.: só Kong e Prometheus alcançam as APIs; ninguém além de Campaigns.Api/Donations.Worker alcança o RabbitMQ, e só na porta AMQP — a UI de management em 15672 fica fechada para outros pods). Isso **não afeta** `kubectl port-forward`, que continua funcionando normalmente para qualquer serviço.

### 7. Criar os secrets do JWT (a partir das chaves geradas no passo 1 — nunca commitados)

```bash
kubectl create secret generic jwt-signing-key -n conexao-solidaria \
  --from-file=jwt-private.pem=Identity.Api/Identity.Api/keys/jwt-private.pem

kubectl create secret generic jwt-public-key -n conexao-solidaria \
  --from-file=jwt-public.pem=Campaigns.Api/Campaigns.Api/keys/jwt-public.pem
```

### 8. Subir os 3 serviços da aplicação

```bash
kubectl apply -f Identity.Api/deploy/
kubectl apply -f Campaigns.Api/deploy/
kubectl apply -f Donations.Worker/deploy/

kubectl wait --for=condition=Ready pod -l app=identity-api -n conexao-solidaria --timeout=60s
kubectl wait --for=condition=Ready pod -l app=campaigns-api -n conexao-solidaria --timeout=60s
kubectl wait --for=condition=Ready pod -l app=donations-worker -n conexao-solidaria --timeout=60s

kubectl get pods -n conexao-solidaria -o wide
```

### 9. Subir o Kong API Gateway (bônus, opcional)

DB-less/declarativo — não bloqueante para o MVP, mas roteia por path para `Identity.Api` e `Campaigns.Api`. `Donations.Worker` fica de fora (sem API de negócio).

```bash
kubectl apply -f deploy/kong/
kubectl wait --for=condition=Ready pod -l app=kong -n conexao-solidaria --timeout=60s
```

### 10. Acessar os serviços (kind não expõe portas do cluster no host por padrão)

Em terminais separados:

```bash
kubectl port-forward svc/identity-api 5010:8080 -n conexao-solidaria
kubectl port-forward svc/campaigns-api 5011:8080 -n conexao-solidaria
kubectl port-forward svc/donations-worker 5012:8080 -n conexao-solidaria   # só /health e /metrics
kubectl port-forward svc/rabbitmq 15672:15672 -n conexao-solidaria        # management UI
kubectl port-forward svc/prometheus 9090:9090 -n conexao-solidaria        # Prometheus UI
kubectl port-forward svc/grafana 3000:3000 -n conexao-solidaria           # Grafana
kubectl port-forward svc/kong 8000:8000 -n conexao-solidaria              # Kong (bônus) — proxy
kubectl port-forward svc/kong 8001:8001 -n conexao-solidaria              # Kong (bônus) — admin API
```

- Swagger: http://localhost:5010/swagger (Identity.Api) e http://localhost:5011/swagger (Campaigns.Api)
- RabbitMQ management UI: http://localhost:15672 (usuário/senha: `donations_app`/`donations_app_dev_pw` — ver `deploy/rabbitmq/secret.yaml`; não é mais `guest`/`guest`)
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (usuário/senha: `admin`/`admin`, dev only — ver `deploy/grafana/secret.yaml` — dashboard "Conexão Solidária - Microsserviços" já vem provisionado, sem precisar montar nada na mão)
- Kong (bônus) — proxy: mesmos endpoints de `Identity.Api`/`Campaigns.Api` na porta 8000, ex.: `curl http://localhost:8000/campaigns/public`
- Kong (bônus) — admin API: http://localhost:8001 (ex.: `curl http://localhost:8001/status`). Sem autenticação por padrão nessa config DB-less — exposta de propósito para a demo mostrar o roteamento sendo consultado; não é assim que ficaria numa implantação real.

### 11. Credenciais do seed inicial (`NgoManager`)

Criadas automaticamente no primeiro start do `Identity.Api` — não há endpoint público para cadastro de `NgoManager`.

```
email: admin@esperancasolidaria.org
senha: Admin@12345
```

## Testando a ponta a ponta (exemplo com curl)

```bash
# 1. Login como gestor
NGO_TOKEN=$(curl -s -X POST http://localhost:5010/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@esperancasolidaria.org","password":"Admin@12345"}' \
  | grep -oP '(?<="accessToken":")[^"]+')

# 2. Criar uma campanha
curl -s -X POST http://localhost:5011/campaigns \
  -H "Content-Type: application/json" -H "Authorization: Bearer $NGO_TOKEN" \
  -d '{"title":"Campanha de Inverno","description":"Agasalhos","startDate":"2026-09-02T00:00:00Z","endDate":"2026-10-01T00:00:00Z","financialGoal":10000}'

# 3. Cadastrar (POST /donors) e logar (POST /auth/login) como doador, depois enviar POST /donations
#    com o CampaignId da campanha criada acima e o token do doador

# 4. Conferir o painel público — o valor é atualizado pelo Donations.Worker de forma assíncrona
curl -s http://localhost:5011/campaigns/public
```

## Encerrar / limpar

```bash
kind delete cluster --name conexao-solidaria
```

## Testes automatizados

```bash
dotnet test ConexaoSolidaria.slnx
```

## CI/CD

`.github/workflows/ci.yml` — dispara em push/PR pra `main`. Build + testes .NET rodam sempre; a build de cada imagem Docker passa por scan de vulnerabilidades com **Trivy** (relatório completo sobe pra aba *Security* do GitHub, e falha o pipeline se achar algo `CRITICAL` com correção disponível) antes de publicar em `ghcr.io/<owner>/<repo>/<serviço>` — só a partir de um push real em `main`.

> Esse workflow só roda depois que o repositório existir no GitHub. Localmente já está com `git init` + commit inicial feitos; falta só `git remote add origin <url-do-seu-repo>` e `git push -u origin main`.
