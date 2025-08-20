# 🚀 KGWin & KGWeb Deployment Guide

## ✅ **Current Deployment Status**

### **Successfully Deployed:**
- ✅ **KGWeb Angular Application** → `D:\Work\Office\Damco\KloudGin\Repositories\KGWin\docs\`
- ✅ **KGWin Desktop Application** → `D:\Work\Office\Damco\Deployed\KGWin\`
- ✅ **GitHub Pages Ready** → `https://sandeepk7.github.io/kgweb/`

---

## 📋 **Manual Deployment Steps**

### **1. Deploy KGWeb (Angular Application)**

```bash
# Navigate to KGWeb directory
cd KGWeb

# Install dependencies (if not already done)
npm install

# Build for production
npm run build:gh-pages
```

**What this does:**
- Builds the Angular app for production
- Outputs to `../docs/` folder
- Configures for GitHub Pages with base href `/kgweb/`
- Copies SPA routing files (`404.html`)

### **2. Deploy KGWin (Desktop Application)**

```bash
# Navigate to KGWin directory
cd KGWin

# Publish for Release
dotnet publish -c Release -o "D:\Work\Office\Damco\Deployed\KGWin"
```

**What this does:**
- Compiles the .NET WPF application
- Creates a self-contained deployment
- Includes all dependencies and runtime files

### **3. Register Custom Protocol (One-time setup)**

```bash
# Run as Administrator
D:\Work\Office\Damco\Deployed\KGWin\register-protocol.bat
```

**What this does:**
- Registers `kgwin://` protocol in Windows Registry
- Enables deep linking from web browser to desktop app

---

## 🎯 **Visual Studio Deployment Steps**

### **For KGWin Desktop Application:**

#### **Method 1: Using Visual Studio GUI**

1. **Open Solution**
   - Open `KGWin.sln` in Visual Studio
   - Ensure all projects load successfully

2. **Configure Build Settings**
   - Right-click on `KGWin` project in Solution Explorer
   - Select **Properties**
   - Go to **Build** tab
   - Set **Configuration** to `Release`
   - Set **Platform** to `Any CPU`

3. **Configure Publish Settings**
   - Right-click on `KGWin` project
   - Select **Publish**
   - Choose **Folder** as publish target
   - Set **Target location** to: `D:\Work\Office\Damco\Deployed\KGWin`
   - Click **Publish**

#### **Method 2: Using Visual Studio Command Line**

```bash
# Open Developer Command Prompt for VS
# Navigate to solution directory
cd "D:\Work\Office\Damco\KloudGin\Repositories\KGWin"

# Build and publish
msbuild KGWin.sln /p:Configuration=Release /p:Platform="Any CPU"
dotnet publish KGWin/KGWin.csproj -c Release -o "D:\Work\Office\Damco\Deployed\KGWin"
```

### **For KGWeb Angular Application:**

#### **Using Visual Studio Code (Recommended for Angular)**

1. **Open Project**
   - Open `KGWeb` folder in VS Code
   - Install recommended extensions (Angular, TypeScript)

2. **Build and Deploy**
   ```bash
   # Install dependencies
   npm install

   # Build for production
   npm run build:gh-pages
   ```

#### **Using Visual Studio (Alternative)**

1. **Open Angular Project**
   - Open `KGWeb` folder in Visual Studio
   - Install Node.js tools if prompted

2. **Configure Build**
   - Right-click on project
   - Select **Properties**
   - Configure npm scripts in package.json

---

## 🌐 **GitHub Pages Deployment**

### **Automatic Deployment (Recommended)**

1. **Push to GitHub**
   ```bash
   git add .
   git commit -m "Deploy updated application"
   git push origin main
   ```

2. **GitHub Pages Settings**
   - Go to repository Settings → Pages
   - Source: **Deploy from a branch**
   - Branch: **main** → **/docs**
   - Save

3. **Access Application**
   - URL: `https://sandeepk7.github.io/kgweb/`
   - Wait 2-5 minutes for deployment

### **Manual Deployment**

1. **Copy Files**
   ```bash
   # Copy docs folder to GitHub Pages branch
   git checkout gh-pages
   cp -r docs/* .
   git add .
   git commit -m "Update deployment"
   git push origin gh-pages
   ```

---

## 🔧 **Post-Deployment Setup**

### **1. Register Custom Protocol (One-time)**
```bash
# Run as Administrator
D:\Work\Office\Damco\Deployed\KGWin\register-protocol.bat
```

### **2. Test Deployment**
1. **Web Application**: Visit `https://sandeepk7.github.io/kgweb/`
2. **Desktop Application**: Run `D:\Work\Office\Damco\Deployed\KGWin\KGWin.exe`
3. **Deep Linking**: Click "Launch KGWin Application" in web app

### **3. Verify Functionality**
- ✅ Web app loads correctly
- ✅ Navigation works with new spacing
- ✅ Dropdown parameters work
- ✅ Desktop app launches via protocol
- ✅ SignalR communication established

---

## 📁 **Deployment Structure**

```
D:\Work\Office\Damco\KloudGin\Repositories\KGWin\
├── docs\                          # Angular Web App (GitHub Pages)
│   ├── index.html
│   ├── 404.html                   # SPA routing
│   ├── assets\
│   └── *.js, *.css files
│
└── D:\Work\Office\Damco\Deployed\KGWin\  # Desktop App
    ├── KGWin.exe
    ├── KGWin.dll
    ├── appsettings.json
    ├── register-protocol.bat
    ├── launch.bat
    ├── README.md
    └── runtime files\
```

---

## 🚨 **Troubleshooting**

### **Common Issues:**

1. **Protocol Not Working**
   - Run `register-protocol.bat` as Administrator
   - Check registry: `HKEY_CLASSES_ROOT\kgwin`

2. **Web App Not Loading**
   - Check GitHub Pages settings
   - Verify base href in build
   - Check browser console for errors

3. **Build Failures**
   - Run `npm install` in KGWeb
   - Check .NET SDK version for KGWin
   - Verify all dependencies

4. **CORS Issues**
   - Ensure KGWin is running on `http://localhost:5000`
   - Check CORS configuration in `App.xaml.cs`

---

## 📞 **Support**

For deployment issues:
1. Check console logs in browser
2. Check Visual Studio Output window
3. Verify all paths and permissions
4. Ensure Administrator privileges for protocol registration

---

**🎉 Deployment Complete! Your application is now live and ready to use!**
