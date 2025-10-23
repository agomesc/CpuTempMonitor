# 🚀 Instruções para Subir o Projeto para o GitHub

## Passo 1: Criar Repositório no GitHub

1. **Acesse**: [GitHub.com](https://github.com)
2. **Faça login** na sua conta
3. **Clique em "New repository"** (botão verde) ou no "+" no canto superior direito
4. **Configure o repositório**:
   - **Repository name**: `CpuTempMonitor` ou `SystemMonitor`
   - **Description**: `🖥️ Monitor de sistema em tempo real para Windows - CPU, RAM, Clock com alertas de temperatura`
   - **Visibilidade**: 
     - ✅ **Public** (recomendado para projetos open source)
     - ❌ Private (se quiser manter privado)
   - **❌ NÃO marque**: "Add a README file" (já temos um)
   - **❌ NÃO marque**: "Add .gitignore" (já temos um)
   - **❌ NÃO marque**: "Choose a license" (pode adicionar depois)

5. **Clique em "Create repository"**

## Passo 2: Conectar Repositório Local ao GitHub

Após criar o repositório, o GitHub mostrará comandos. Use estes comandos no terminal:

```bash
# Adicionar o repositório remoto (substitua SEU_USUARIO pelo seu username do GitHub)
git remote add origin https://github.com/SEU_USUARIO/CpuTempMonitor.git

# Renomear branch para 'main' (padrão atual do GitHub)
git branch -M main

# Fazer push do código
git push -u origin main
```

## Passo 3: Comandos Prontos (Execute no Terminal)

```powershell
# 1. Adicionar repositório remoto
git remote add origin https://github.com/agomesc/CpuTempMonitor.git

# 2. Renomear branch
git branch -M main

# 3. Fazer upload do código
git push -u origin main
```

**⚠️ Importante**: Substitua `agomesc` pelo seu username real do GitHub!

## Passo 4: Verificar se Funcionou

1. **Acesse**: `https://github.com/SEU_USUARIO/CpuTempMonitor`
2. **Verifique** se todos os arquivos estão lá:
   - ✅ README.md (será exibido automaticamente)
   - ✅ Program.cs
   - ✅ Install-StartupMonitor.ps1
   - ✅ Instalar.bat
   - ✅ Outros arquivos do projeto

## Passo 5: Personalizar o Repositório

### Adicionar Tags/Topics
No GitHub, na página do repositório:
1. Clique na engrenagem ⚙️ ao lado de "About"
2. Adicione **Topics**: `windows`, `monitoring`, `cpu-temperature`, `system-monitor`, `csharp`, `dotnet`
3. Adicione **Website** (se tiver)

### Adicionar Licença
1. Na página do repositório, clique em "Add file" > "Create new file"
2. Digite `LICENSE` como nome do arquivo
3. Clique em "Choose a license template"
4. Selecione **MIT License** (recomendado para projetos open source)

### Configurar Releases
1. Na aba "Releases" do repositório
2. Clique em "Create a new release"
3. **Tag**: `v1.0.0`
4. **Title**: `🚀 Monitor de Sistema v1.0.0`
5. **Description**: Copie a descrição do commit inicial
6. Anexe o executável: `bin/Release/net9.0-windows/publish/CpuTempMonitor.exe`

## 🎯 Comandos Completos para Executar Agora

Execute estes comandos um por vez no terminal do PowerShell:

```powershell
# Verificar status atual
git status

# Adicionar repositório remoto (AJUSTE O USERNAME!)
git remote add origin https://github.com/agomesc/CpuTempMonitor.git

# Verificar se foi adicionado
git remote -v

# Renomear branch para main
git branch -M main

# Fazer push
git push -u origin main
```

## 🔧 Solução de Problemas

### Erro de Autenticação
Se aparecer erro de login:
1. Use **Personal Access Token** em vez de senha
2. Gere um token em: Settings > Developer settings > Personal access tokens
3. Use o token como senha

### Repositório já existe
Se der erro de repositório existente:
```powershell
git remote set-url origin https://github.com/SEU_USUARIO/CpuTempMonitor.git
```

### Forçar push (usar apenas se necessário)
```powershell
git push -f origin main
```

---

## 📋 Checklist Final

- [ ] Repositório criado no GitHub
- [ ] Comando `git remote add origin` executado
- [ ] Comando `git push -u origin main` executado com sucesso
- [ ] README.md está sendo exibido na página do repositório
- [ ] Todos os arquivos estão visíveis no GitHub
- [ ] Topics/tags adicionadas ao repositório
- [ ] Licença adicionada (opcional)
- [ ] Release criado com executável (opcional)

🎉 **Parabéns!** Seu projeto está agora no GitHub e disponível para o mundo!