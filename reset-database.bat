@echo off
echo ========================================
echo  ОНОВЛЕННЯ БАЗИ ДАНИХ З ПРАВИЛЬНИМИ ЦІНАМИ
echo ========================================
echo.

echo [1/4] Видалення старої бази даних...
dotnet ef database drop --force

echo.
echo [2/4] Застосування міграцій...
dotnet ef database update

echo.
echo [3/4] База даних створена!
echo.
echo Тепер запустіть додаток командою:
echo    dotnet run
echo.
echo Seeder автоматично додасть українські препарати з цінами.
echo.
echo Облікові записи:
echo    Admin:      admin@pharmacy.com / Admin123!
echo    Pharmacist: pharmacist@pharmacy.com / Pharm123!
echo    User:       user@pharmacy.com / User123!
echo.
echo ========================================
pause
