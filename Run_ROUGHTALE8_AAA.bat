@echo off
cd /d "%~dp0"

:: 1. Tutup instans pythonw lama jika ada agar port 8090 langsung bersih dan tidak duplikat
taskkill /f /im pythonw.exe >nul 2>nul

:: 2. Buka browser langsung ke game
start "" "http://127.0.0.1:8090/index_aaa.html"

:: 3. Jalankan server lokal di background secara hening (0 jendela terminal)
start "" pythonw "%~dp0serve_aaa.py" --no-browser

:: 4. Tutup script launcher ini seketika
exit
