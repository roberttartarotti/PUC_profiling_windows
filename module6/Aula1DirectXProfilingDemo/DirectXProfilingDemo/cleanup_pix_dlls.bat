@echo off
echo === Limpeza de DLLs PIX Copiadas ===
echo.
echo Este script remove DLLs do PIX que foram copiadas anteriormente
echo e que podem causar incompatibilidade de versão.
echo.

echo [INFO] Removendo DLLs PIX copiadas...

if exist "bin\Debug\net10.0-windows\WinPixGpuCapturer.dll" (
    del "bin\Debug\net10.0-windows\WinPixGpuCapturer.dll"
    echo   ? Removido: bin\Debug\net10.0-windows\WinPixGpuCapturer.dll
) else (
    echo   ?? Não encontrado: bin\Debug\net10.0-windows\WinPixGpuCapturer.dll
)

if exist "bin\Release\net10.0-windows\WinPixGpuCapturer.dll" (
    del "bin\Release\net10.0-windows\WinPixGpuCapturer.dll"
    echo   ? Removido: bin\Release\net10.0-windows\WinPixGpuCapturer.dll
) else (
    echo   ?? Não encontrado: bin\Release\net10.0-windows\WinPixGpuCapturer.dll
)

if exist "bin\Debug\net10.0-windows\WinPixTimingCapturer.dll" (
    del "bin\Debug\net10.0-windows\WinPixTimingCapturer.dll"
    echo   ? Removido: bin\Debug\net10.0-windows\WinPixTimingCapturer.dll
)

if exist "bin\Release\net10.0-windows\WinPixTimingCapturer.dll" (
    del "bin\Release\net10.0-windows\WinPixTimingCapturer.dll"
    echo   ? Removido: bin\Release\net10.0-windows\WinPixTimingCapturer.dll
)

if exist "bin\Debug\net10.0-windows\WinPixEventRuntime.dll" (
    del "bin\Debug\net10.0-windows\WinPixEventRuntime.dll"
    echo   ? Removido: bin\Debug\net10.0-windows\WinPixEventRuntime.dll
)

if exist "bin\Release\net10.0-windows\WinPixEventRuntime.dll" (
    del "bin\Release\net10.0-windows\WinPixEventRuntime.dll"
    echo   ? Removido: bin\Release\net10.0-windows\WinPixEventRuntime.dll
)

echo.
echo [SUCCESS] ? Limpeza concluída!
echo.
echo Agora a aplicação carregará as DLLs diretamente
echo do diretório de instalação do PIX, garantindo
echo compatibilidade de versão.
echo.
echo Próximos passos:
echo 1. Execute: dotnet run
echo 2. PIX carregará do diretório original
echo 3. Attach funcionará sem problemas de compatibilidade
echo.
pause