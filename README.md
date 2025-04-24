# 🛒 ShoppingModular

Projeto de E-commerce modular com arquitetura moderna baseada em **microserviços**, **DDD**, **CQRS**, **Event Sourcing** e **Clean Architecture**, utilizando tecnologias de ponta como `.NET 9`, `Kafka`, `Redis`, `MongoDB`, `PostgreSQL`, `Prometheus`, `Grafana`, `Elastic Stack` e `Docker`.

---

## 🧱 Estrutura do Projeto

```
ShoppingModular/
├── building-blocks/            # Camadas compartilhadas (Domain, Application, Infrastructure)
├── services/                   # Microserviços (Orders.API, Products.API, Payments.API...)
│   ├── Orders.API/
│   ├── Products.API/
│   ├── Orders.Consumer/
│   └── Payments.API/ (em breve)
├── docker/                     # Docker Compose com Redis, MongoDB, PostgreSQL, Kafka, Prometheus, Grafana
├── tests/                      # NUnit + Bogus com 100% de cobertura
├── .github/workflows/          # CI/CD com GitHub Actions
└── README.md
```

---

## 🚀 Tecnologias Usadas

- **Backend**: .NET 9, C#
- **Mensageria**: Kafka + Confluent.Kafka
- **Bancos**: PostgreSQL (write), MongoDB (read), Redis (cache)
- **Arquitetura**: DDD + CQRS + Event Sourcing + Clean Architecture
- **Observabilidade**: Serilog + Elastic Stack (ELK), Prometheus + Grafana
- **Testes**: NUnit + Bogus + Coverlet (100% cobertura) + ReportGenerator
- **Deploy & CI/CD**: Docker, GitHub Actions
- **Documentação**: Swagger + ReDoc

---

## 📦 Como rodar localmente

```bash
# Subir infraestrutura
cd docker/infra
docker compose up -d

# Subir o projeto
dotnet build
dotnet run --project services/Orders.API/Orders.API.csproj
```

> Certifique-se de que o `ASPNETCORE_ENVIRONMENT=Local` esteja setado (automaticamente pelo docker).

---

## 🧪 Executar Testes

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
```

> O relatório de cobertura será gerado na pasta `coverage-report/index.html`.

---

## 📊 Dashboards e Logs

- Grafana: [http://localhost:3000](http://localhost:3000)
- Kibana: [http://localhost:5601](http://localhost:5601)
- Prometheus: [http://localhost:9090](http://localhost:9090)

---

## 🧰 Comandos Úteis

```bash
# Resetar ambiente
docker compose down -v && docker compose up -d

# Subir apenas um serviço
docker compose up orders-api

# Validar testes no CI
gh workflow run ci.yml
```

---

## 🗂️ Próximos Passos

- [x] Orders.API (com Redis, Mongo, PostgreSQL e Kafka)
- [x] Products.API (em progresso)
- [ ] Payments.API (próximo serviço)
- [ ] Notifications.API (integrações com e-mail, SMS, etc.)
- [ ] Front-end React + Tailwind (futuro)
