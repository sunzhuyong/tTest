@echo off
echo ========================================
echo   模拟炒股系统启动中...
echo ========================================

echo.
echo [1] 启动 Web 服务...
start "Trading Web" cmd /k "cd /d %~dp0TradingSimulator && dotnet run -- --web"

echo [2] 等待服务启动...
timeout /t 3 /nobreak > nul

echo [3] 打开浏览器...
start http://localhost:5000/Web/index.html

echo.
echo ========================================
echo   系统已启动!
echo   Web界面: http://localhost:5000/Web/index.html
echo   API文档: http://localhost:5000/swagger
echo ========================================
echo.
echo 按任意键退出 (服务会在后台继续运行)...
pause > nul
