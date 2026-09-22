#!/usr/bin/env bash
# Fires a burst of concurrent donations through Kong -> Campaigns.Api -> RabbitMQ ->
# Donations.Worker, then polls the public panel until every one lands, to demonstrate the
# pipeline under real concurrent load (for the hackathon demo video) and to verify, end to end,
# that no donation gets lost or double-counted under load — the same atomicity/idempotency
# guarantees documented in docs/justificativa-banco.pdf, now proven at volume instead of with
# a handful of requests.
#
# Usage:
#   ./scripts/load-test-donations.sh [TOTAL] [CONCURRENCY] [BASE_URL]
#
# Defaults: TOTAL=3000 CONCURRENCY=50 BASE_URL=http://127.0.0.1:8000 (Kong proxy port-forward)
#
# Prereqs: `kubectl port-forward svc/kong 8000:8000 -n conexao-solidaria` running in another
# terminal (or pass a different BASE_URL, e.g. a direct campaigns-api port-forward on 5011).

set -euo pipefail

TOTAL="${1:-3000}"
CONCURRENCY="${2:-50}"
BASE_URL="${3:-http://127.0.0.1:8000}"

NGO_EMAIL="admin@esperancasolidaria.org"
NGO_PASSWORD="Admin@12345"
DONOR_EMAIL="loadtest-donor@example.com"
DONOR_CPF="73597530311"
DONOR_PASSWORD="LoadTest@12345"

echo "== Conexão Solidária — teste de carga de doações =="
echo "TOTAL=$TOTAL CONCURRENCY=$CONCURRENCY BASE_URL=$BASE_URL"
echo

echo "-> login NgoManager"
NGO_LOGIN=$(curl -s -X POST "$BASE_URL/auth/login" -H "Content-Type: application/json" \
  -d "{\"email\":\"$NGO_EMAIL\",\"password\":\"$NGO_PASSWORD\"}")
NGO_TOKEN=$(echo "$NGO_LOGIN" | grep -oE '"accessToken":"[^"]+"' | cut -d'"' -f4)
if [ -z "$NGO_TOKEN" ]; then
  echo "Falha no login do NgoManager. Resposta: $NGO_LOGIN" >&2
  exit 1
fi

echo "-> criando campanha dedicada ao teste de carga"
TITLE="Teste de Carga $(date +%s)"
CREATE=$(curl -s -X POST "$BASE_URL/campaigns" -H "Content-Type: application/json" -H "Authorization: Bearer $NGO_TOKEN" \
  -d "{\"title\":\"$TITLE\",\"description\":\"Gerada por scripts/load-test-donations.sh\",\"startDate\":\"2026-01-01T00:00:00Z\",\"endDate\":\"2027-01-01T00:00:00Z\",\"financialGoal\":999999999}")
CAMPAIGN_ID=$(echo "$CREATE" | grep -oE '"id":"[^"]+"' | cut -d'"' -f4)
if [ -z "$CAMPAIGN_ID" ]; then
  echo "Falha ao criar campanha. Resposta: $CREATE" >&2
  exit 1
fi
echo "   CampaignId=$CAMPAIGN_ID (meta financeira alta de propósito, só pra não bloquear o teste)"

echo "-> garantindo doador de teste (registra se ainda não existir)"
curl -s -X POST "$BASE_URL/donors" -H "Content-Type: application/json" \
  -d "{\"fullName\":\"Load Test Donor\",\"email\":\"$DONOR_EMAIL\",\"cpf\":\"$DONOR_CPF\",\"password\":\"$DONOR_PASSWORD\"}" > /dev/null

DONOR_LOGIN=$(curl -s -X POST "$BASE_URL/auth/login" -H "Content-Type: application/json" \
  -d "{\"email\":\"$DONOR_EMAIL\",\"password\":\"$DONOR_PASSWORD\"}")
DONOR_TOKEN=$(echo "$DONOR_LOGIN" | grep -oE '"accessToken":"[^"]+"' | cut -d'"' -f4)
if [ -z "$DONOR_TOKEN" ]; then
  echo "Falha no login do doador de teste. Resposta: $DONOR_LOGIN" >&2
  exit 1
fi

echo
echo "-> disparando $TOTAL doações de R\$ 1,00 cada, $CONCURRENCY em paralelo"
START=$(date +%s)

seq 1 "$TOTAL" | xargs -P "$CONCURRENCY" -I{} curl -s -o /dev/null -w '%{http_code}\n' \
  -X POST "$BASE_URL/donations" -H "Content-Type: application/json" -H "Authorization: Bearer $DONOR_TOKEN" \
  -d "{\"campaignId\":\"$CAMPAIGN_ID\",\"donationAmount\":1}" \
  > /tmp/load-test-status-codes.txt

END=$(date +%s)
ELAPSED=$((END - START))
ACCEPTED=$(grep -c '^202$' /tmp/load-test-status-codes.txt || true)
FAILED=$((TOTAL - ACCEPTED))

echo "   $ACCEPTED/$TOTAL aceitas (202) em ${ELAPSED}s ($(( TOTAL / (ELAPSED > 0 ? ELAPSED : 1) )) req/s no caminho síncrono)"
if [ "$FAILED" -gt 0 ]; then
  echo "   AVISO: $FAILED requisições não retornaram 202 — códigos vistos:"
  sort /tmp/load-test-status-codes.txt | uniq -c
fi

echo
echo "-> aguardando o Donations.Worker drenar a fila (polling do painel público)"
DRAIN_START=$(date +%s)
for i in $(seq 1 120); do
  AMOUNT=$(curl -s "$BASE_URL/campaigns/public" | grep -o "\"title\":\"$TITLE\"[^}]*\"amountRaised\":[0-9.]*" | grep -oE '"amountRaised":[0-9.]+' | cut -d: -f2)
  AMOUNT="${AMOUNT%.*}"
  if [ "${AMOUNT:-0}" -ge "$ACCEPTED" ] 2>/dev/null; then
    break
  fi
  sleep 1
done
DRAIN_END=$(date +%s)

echo "   AmountRaised final: ${AMOUNT:-0} (esperado: $ACCEPTED) — drenou em $((DRAIN_END - DRAIN_START))s"
echo

if [ "${AMOUNT:-0}" = "$ACCEPTED" ]; then
  echo "OK: todas as doações aceitas foram aplicadas exatamente uma vez cada, sem perda sob concorrência."
else
  echo "ATENÇÃO: valor final ($AMOUNT) difere do esperado ($ACCEPTED) — o Worker pode ainda estar processando; rode de novo em alguns segundos ou aumente o timeout de polling."
fi

rm -f /tmp/load-test-status-codes.txt
