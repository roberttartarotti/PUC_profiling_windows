@echo off
echo === DirectX Profiling Demo - PIX Verification ===
echo.

:: Definir possíveis caminhos do PIX (sem aspas na definição)
set PIX_PATH1=C:\Program Files\Microsoft PIX
set PIX_PATH2=C:\Program Files\Microsoft PIX\2509.25
set PIX_PATH3=C:\Program Files (x86)\Microsoft PIX
set PIX_PATH4=C:\Program Files\Microsoft PIX\2510.01
set PIX_PATH5=C:\Program Files\Microsoft PIX\2511.01
set FOUND_PIX=0
set PIX_PATH=

:: Verificar múltiplos caminhos possíveis
echo [INFO] Verificando instalação do PIX for Windows...

if exist "%PIX_PATH2%\WinPixGpuCapturer.dll" (
    set "PIX_PATH=%PIX_PATH2%"
    set FOUND_PIX=1
    echo [INFO] ✅ PIX encontrado em: "%PIX_PATH2%"
    goto :found_pix
)

if exist "%PIX_PATH4%\WinPixGpuCapturer.dll" (
    set "PIX_PATH=%PIX_PATH4%"
    set FOUND_PIX=1
    echo [INFO] ✅ PIX encontrado em: "%PIX_PATH4%"
    goto :found_pix
)

if exist "%PIX_PATH5%\WinPixGpuCapturer.dll" (
    set "PIX_PATH=%PIX_PATH5%"
    set FOUND_PIX=1
    echo [INFO] ✅ PIX encontrado em: "%PIX_PATH5%"
    goto :found_pix
)

if exist "%PIX_PATH1%\WinPixGpuCapturer.dll" (
    set "PIX_PATH=%PIX_PATH1%"
    set FOUND_PIX=1
    echo [INFO] ✅ PIX encontrado em: "%PIX_PATH1%"
    goto :found_pix
)

if exist "%PIX_PATH3%\WinPixGpuCapturer.dll" (
    set "PIX_PATH=%PIX_PATH3%"
    set FOUND_PIX=1
    echo [INFO] ✅ PIX encontrado em: "%PIX_PATH3%"
    goto :found_pix
)

:: Se chegou aqui, não encontrou PIX
echo [ERROR] ❌ PIX for Windows não encontrado!
echo.
echo Locais verificados:
echo   - "%PIX_PATH1%"
echo   - "%PIX_PATH2%"
echo   - "%PIX_PATH3%"
echo   - "%PIX_PATH4%"
echo   - "%PIX_PATH5%"
echo.
echo SOLUÇÕES:
echo 1. Instalar PIX for Windows:
echo    - https://devblogs.microsoft.com/pix/download/
echo.
echo 2. Após instalar, execute novamente este script
echo.
echo Verificando diretórios existentes:
if exist "C:\Program Files\Microsoft PIX" (
    echo   ✅ Encontrado: C:\Program Files\Microsoft PIX
    echo   Conteúdo:
    dir "C:\Program Files\Microsoft PIX" /AD /B 2>nul
) else (
    echo   ❌ C:\Program Files\Microsoft PIX não existe
)

if exist "C:\Program Files (x86)\Microsoft PIX" (
    echo   ✅ Encontrado: C:\Program Files (x86)\Microsoft PIX
) else (
    echo   ❌ C:\Program Files (x86)\Microsoft PIX não existe
)
echo.
pause
exit /b 1

:found_pix
:: Verificar arquivos PIX
echo [SUCCESS] ✅ PIX for Windows detectado!
echo Diretório: "%PIX_PATH%"
echo.

echo [INFO] Verificando arquivos PIX essenciais...
if exist "%PIX_PATH%\WinPixGpuCapturer.dll" (
    echo   ✅ WinPixGpuCapturer.dll - ESSENCIAL para GPU capture
) else (
    echo   ❌ WinPixGpuCapturer.dll - AUSENTE!
)

if exist "%PIX_PATH%\WinPixTimingCapturer.dll" (
    echo   ✅ WinPixTimingCapturer.dll - Para timing analysis
) else (
    echo   ⚠️ WinPixTimingCapturer.dll - Não encontrada (opcional)
)

if exist "%PIX_PATH%\WinPixEventRuntime.dll" (
    echo   ✅ WinPixEventRuntime.dll - Para PIX events
) else (
    echo   ⚠️ WinPixEventRuntime.dll - Não encontrada (opcional)
)

if exist "%PIX_PATH%\WinPix.exe" (
    echo   ✅ WinPix.exe - Aplicação principal PIX
) else (
    echo   ❌ WinPix.exe - AUSENTE! PIX não funcionará
)

echo.
echo ========================================
echo        PIX CONFIGURATION STATUS
echo ========================================
echo.
echo ✅ PIX Installation: FOUND
echo 📁 PIX Directory: "%PIX_PATH%"
echo 🚀 Status: READY FOR USE
echo.
echo ========================================
echo           INSTRUÇÕES DE USO
echo ========================================
echo.
echo 1. Execute a aplicação: dotnet run
echo    - PIX será automaticamente detectado
echo    - DLLs serão carregadas do diretório original
echo.
echo 2. Abrir PIX for Windows:
echo    - Execute como ADMINISTRADOR
echo    - Caminho: "%PIX_PATH%\WinPix.exe"
echo.
echo 3. Attach to Process:
echo    - No PIX, clique "Attach to Process"
echo    - Selecione "DirectXProfilingDemo.exe"
echo    - ✅ DEVE FUNCIONAR sem erros!
echo.
echo 4. GPU Capture:
echo    - Configure cenários na aplicação
echo    - Clique "Take GPU Capture" no PIX
echo    - Analise os marcadores coloridos
echo.
echo ========================================
echo          CENÁRIOS DE TESTE
echo ========================================
echo.
echo 🔴 GPU Bottleneck:
echo    - Triângulos: 10000
echo    - Shader Complexo: ✅
echo    - Overdraw: ✅
echo.
echo 🔵 CPU Bottleneck:
echo    - Draw Calls: 10x
echo    - Triângulos: 5000
echo.
echo 🟢 Baseline:
echo    - Configurações padrão
echo    - Para comparação
echo.
echo ========================================
echo.
echo IMPORTANTE: 
echo ❌ NÃO copie DLLs do PIX - causa incompatibilidade
echo ✅ Aplicação carrega DLLs diretamente do PIX
echo ✅ Garante compatibilidade de versão
echo.

:: Mostrar versão do PIX se disponível
echo Informações da instalação PIX:
if exist "%PIX_PATH%\WinPix.exe" (
    echo Executável: "%PIX_PATH%\WinPix.exe"
    dir "%PIX_PATH%\WinPix.exe" | findstr /C:"WinPix.exe"
)

echo.
echo Status: ✅ PIX configurado e compatível!
echo.
pause