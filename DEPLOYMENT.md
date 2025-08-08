# Karata Deployment Guide

This document describes the complete deployment pipeline for `karata`, from triggering a deployment in GitHub to running the new release on your Ubuntu server. It covers prerequisites, environment configuration, deployment flow, and rollback strategy.

---

## 1. Deployment Flow Overview

```mermaid
sequenceDiagram
    participant Tooling
    participant GitHub
    participant GitHubActions as GitHub Actions
    participant Server as Your Server

    Tooling->>GitHub: Create Deployment
    GitHub-->>Tooling: Deployment Created
    GitHub->>GitHubActions: Deployment Event
    GitHubActions->>GitHub: Deployment Status
    GitHubActions->>Server: SSH+Deploys
    Server-->>GitHubActions: Deploy Completed
    GitHubActions->>GitHub: Deployment Status
```

In short:

1. A deployment is **requested** via API, GitHub UI, or CLI.
2. GitHub creates a Deployment object and fires a `deployment` event.
3. A GitHub Actions workflow runs the build, uploads files to the server, and executes the deployment script.
4. The server updates the running version, performs a health check, and cleans up old releases.
5. GitHub is updated with the final deployment status.

---

## 2. Server Requirements

Before the first deployment, ensure your Ubuntu server meets these requirements:

* **Operating System:** Ubuntu 20.04 LTS or newer
* **Runtime:**

  * ASP.NET 9.0 Runtime
  * PostgreSQL with Timescale
* **Hardware:**

  * Minimum 1 GB RAM
* **Networking:**

  * Outbound access to fetch dependencies (NuGet, DB)
  * Inbound HTTP(S) via reverse proxy (Nginx, Caddy, Apache)
* **Reverse Proxy:**

  * Configured to forward `X-Forwarded-For`, `X-Forwarded-Proto` headers
  * TLS termination and certificate renewal handled (e.g., Let’s Encrypt)
* **Systemd Service:**

  * `karata.service` configured to run `/opt/karata/current` build
* **SSH Access:**

  * Key-based authentication for GitHub Actions to deploy

---

## 3. Environment Configuration

The application depends on environment variables to run in production:

* `ASPNETCORE_ENVIRONMENT=Production`
* `ASPNETCORE_URLS=http://localhost:5000`
* `DATABASE_URL=postgres://user:pass@host:5432/dbname`
* Optional: any feature flags, cache endpoints, API keys

**Tip:** Store these in a systemd service unit file, `.env` file or `/etc/environment`, not in source control.

---

## 4. Database Setup

* **Type:** PostgreSQL with TimescaleDB extension
* **Permissions:**

  * User must have `CREATE`, `ALTER`, `DROP`, `INSERT`, `UPDATE`, `DELETE` for migrations to run
* **Migrations:**

  * The deployment workflow runs `dotnet ef database update` or during deployment
  * Migrations are idempotent and safe to run multiple times

---

## 5. Creating a Deployment

You can request a deployment in multiple ways:

### 5.1 API

```bash
gh api repos/<owner>/<repo>/deployments \
  -X POST \
  -F ref="main" \
  -F environment="production" \
  -f required_contexts[]
```

### 5.2 GitHub Actions Trigger

Run the **Request Deployment** workflow manually:

```bash
gh workflow run "Request deployment" -f ref=main
```

### 5.3 GitHub UI

* Go to **Actions → Request Deployment**
* Click **Run workflow** and specify the branch or commit

---

## 6. Deployment Execution

Once a deployment is created, the **Deploy** workflow runs:

1. Marks deployment as **in\_progress**
2. Checks out the code at the requested commit
3. Builds the app (`dotnet build`)
4. Runs tests (`dotnet test`)
5. Publishes to `app/` folder (`dotnet publish -c Release`)
6. Uploads to the server via SCP into `/opt/karata/<commit_hash>`
7. Runs `deploy_karata.sh` with the new release path

---

## 7. Server-side Deployment Script (`deploy_karata.sh`)

The script performs:

1. **Symlink Update**
   Updates `/opt/karata/current` to point to the new release folder

2. **Service Restart**
   Runs:

   ```bash
   sudo systemctl restart karata.service
   ```

3. **Health Check**
   Waits `10` seconds, then requests:

   ```
   https://karata.app/health
   ```

   If not `HTTP 200`, rolls back to the previous release.

4. **Cleanup**
   Keeps the two most recent releases, deletes older ones.

---

## 8. Rollback Behavior

* If health check fails:

  * Symlink is reverted to previous release
  * Service is restarted
* If no previous release exists:

  * Deployment fails with no rollback

---

## 9. Monitoring & Logs

* **Deployment Status:**
  Visible in GitHub UI under **Deployments** tab
* **Application Logs:**
  Check via:

  ```bash
  journalctl -u karata.service -f
  ```
* **Health Endpoint:**
  Must return `200 OK` when app is healthy

---

## 10. Security Considerations

* Store all secrets in GitHub Actions Secrets
* Use key-based SSH auth only
* Restrict deploy SSH key to deployment tasks (no root access unless required)
* Ensure health check endpoint is not publicly exploitable for sensitive info

---

If you like, I can also include **an example `karata.service` systemd unit** so your documentation is a true end-to-end setup guide. That would make it easy for someone to replicate the full environment without guesswork.
