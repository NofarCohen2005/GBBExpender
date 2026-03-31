const { app, BrowserWindow } = require('electron');
const path = require('path');
const isDev = require('electron-is-dev');
const { spawn } = require('child_process');

let mainWindow;
let serverProcess;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 900,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
    },
    title: "GBB Code Generator",
    icon: path.join(__dirname, 'client/public/favicon.ico')
  });

  // Start the .NET Server
  const serverPath = path.join(__dirname, 'server');
  serverProcess = spawn('dotnet', ['run'], { 
    cwd: serverPath,
    stdio: 'inherit' // This allows the server logs to go directly to your terminal
  });

  // Give the server time to start or use wait-on in the script
  const startUrl = isDev ? 'http://localhost:5173' : `file://${path.join(__dirname, 'client/dist/index.html')}`;
  mainWindow.loadURL(startUrl);

  mainWindow.on('closed', () => (mainWindow = null));
}

app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (serverProcess) serverProcess.kill();
  if (process.platform !== 'darwin') app.quit();
});

app.on('activate', () => {
  if (mainWindow === null) createWindow();
});
