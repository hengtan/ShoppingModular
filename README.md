# 🛒 ShoppingModular

## 📌 Projeto Modular de E-commerce com .NET 9

O **ShoppingModular** é uma aplicação de e-commerce moderna, escalável e baseada em **microserviços**, construída 
com foco em **DDD**, **CQRS**, **Event Sourcing**, **Kafka**, **Redis**, **MongoDB**, **PostgreSQL** e 
com total observabilidade.

---

## 🧱 Tecnologias Utilizadas

| Tecnologia | Uso |
|-----------|-----|
| .NET 9 | Framework principal |
| DDD + CQRS + Event Sourcing | Arquitetura |
| PostgreSQL | Escrita (Write Model) |
| MongoDB | Leitura (Read Model) |
| Redis | Cache-aside (leitura com fallback) |
| Kafka | Mensageria entre serviços |
| Docker Compose | Orquestração local |
| Serilog | Logging estruturado |
| Prometheus | Métricas customizadas |
| Grafana | Dashboard de observabilidade |
| NUnit + Bogus + Moq | Testes unitários e mocks |

---

## 🚀 Funcionalidades Atuais

- 📦 Criação de pedidos (`Orders.API`)
- 🧾 Persistência em PostgreSQL (write) e MongoDB (read)
- 🧠 Cache com Redis
- 📬 Envio de eventos para Kafka após criação
- 🧪 Testes unitários com 100% de cobertura
- 📊 Métricas expostas via Prometheus
- 🪵 Logs estruturados com Serilog
- 🧰 Integração com Docker e GitHub Actions

---

## 📈 Backlog + Progresso

| Funcionalidade                                      | Status       |
|----------------------------------------------------|--------------|
| Orders.API com gravação em PostgreSQL              | ✅ Concluído |
| Projeção MongoDB + Redis (cache-aside)             | ✅ Concluído |
| Kafka Producer (orders.created)                    | ✅ Concluído |
| Orders.Consumer: salva Mongo e .txt                | ✅ Concluído |
| Testes unitários com 100% de cobertura             | ✅ Concluído |
| Prometheus + métricas customizadas                 | ✅ Concluído |
| Logs estruturados com Serilog                      | ✅ Concluído |
| Products.API + Payments.API + Notifications        | 🚧 Em desenvolvimento |
| Dashboard no Grafana com logs + métricas           | 🚧 Em desenvolvimento |
| Deploy real com Kubernetes                         | 🔜 Futuro |

---

## 🧠 Visão Futura

- Microserviços completos para produtos, pagamentos e notificações
- Comunicação via gRPC e Kafka
- Observabilidade completa com logs centralizados e alertas
- Integração com sistemas de recomendação (ML)
- Deploy em Kubernetes com CI/CD via GitHub Actions
- Orquestração de workflows com Temporal.io

---

## 🛠️ Como Rodar Localmente

```bash
# Subir ambiente com Kafka, Redis, Mongo, Postgres, etc.
docker compose up -d

# Rodar os testes
dotnet test
```

---

## 👨‍💻 Contribuições

Este projeto segue boas práticas de Clean Code, SOLID, testes automatizados e arquitetura orientada a eventos.
Ideal para quem deseja estudar padrões modernos com .NET.

---
